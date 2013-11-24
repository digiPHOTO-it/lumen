using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Transactions;
using log4net;
using Digiphoto.Lumen.Core.Database;
using System.Data.Objects;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging;
using System.Reflection;
using Digiphoto.Lumen.Config;
using ExifLib;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Servizi.EntityRepository;

namespace Digiphoto.Lumen.Servizi.Scaricatore {

	class ElaboratoreFotoAcquisite {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( ElaboratoreFotoAcquisite ) );

		private IList<FileInfo> _listaFiles;

		private ParamScarica _paramScarica;

		private Fotografo _fotografo;
		private Evento _evento;

		public int conta {
			get;
			private set;
		}

		public ElaboratoreFotoAcquisite( IList<FileInfo> list, ParamScarica paramScarica ) {

			this._listaFiles = list;
			this._paramScarica = paramScarica;
		}

        public int numeroFotoAcquisite(){
            return _listaFiles.Count;
        }


		/// <summary>
		/// Il parametro passato "tempoScarico" deve essere ribaltato su tutte le foto, perché 
		/// serve a creare una relazione implicita 1-n tra lo ScaricoCard e Fotografia
		/// </summary>
		/// <param name="tempoScarico"></param>
		public void elabora( DateTime tempoScarico ) {

			// carico il fotografo che rimane uguale per tutta questa sessione di elaborazione
			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
			_fotografo = objContext.Fotografi.Single<Fotografo>( ff => ff.id == _paramScarica.flashCardConfig.idFotografo );


			// carico l'evento che rimane uguale per tutta questa sessione di elaborazione			
			if( _paramScarica.flashCardConfig.idEvento != null && _paramScarica.flashCardConfig.idEvento != Guid.Empty )
				_evento = objContext.Eventi.SingleOrDefault( e => e.id == _paramScarica.flashCardConfig.idEvento );


			_giornale.Debug( "Sto per lavorare le " + _listaFiles.Count + " foto appena acquisite di " + _fotografo.id );

			int ultimoNumFoto = NumeratoreFotogrammi.incrementaNumeratoreFoto( _listaFiles.Count );
			int conta = 0;

			ScaricoFotoMsg scaricoFotoMsg = new ScaricoFotoMsg(this, "Notifica progresso");
			scaricoFotoMsg.fase = FaseScaricoFoto.Provinatura;
			scaricoFotoMsg.esitoScarico = new EsitoScarico();
			scaricoFotoMsg.esitoScarico.totFotoScaricate = _listaFiles.Count;
			scaricoFotoMsg.sorgente = _paramScarica.cartellaSorgente != null ? _paramScarica.cartellaSorgente : _paramScarica.nomeFileSingolo;
			scaricoFotoMsg.showInStatusBar = false;


			foreach( FileInfo fileInfo in _listaFiles ) {

				Fotografia foto = aggiungiFoto( fileInfo, ++conta + ultimoNumFoto, tempoScarico );

				AiutanteFoto.creaProvinoFoto( fileInfo.FullName, foto );


				// Libero la memoria occupata dalle immagini, altrimenti esplode.
				AiutanteFoto.disposeImmagini( foto, IdrataTarget.Tutte );


				// Se lavoro con una singola foto, allora lancio l'evento che mi dice che è pronta.
				if( String.IsNullOrEmpty(_paramScarica.nomeFileSingolo) == false ) {
					// Quando sono a posto con la foto, sollevo un evento per avvisare tutti
					// Siccome questa operazione è un pò onerosa, per il momento la abilito
					// soltanto sulla singola foto. Se ne scarico 1000 di foto, non voglio lanciare 1000 eventi!!!
					NuovaFotoMsg msg = new NuovaFotoMsg( this, foto );
					LumenApplication.Instance.bus.Publish( msg );
				}

				if (conta % 20 == 0)
				{
					scaricoFotoMsg.esitoScarico.totFotoProvinateProg = conta;
					LumenApplication.Instance.bus.Publish(scaricoFotoMsg);
				}

			}

			if (conta != 0)
			{
				scaricoFotoMsg.esitoScarico.totFotoProvinateProg = conta;
				scaricoFotoMsg.esitoScarico.totFotoProvinate = conta;
				LumenApplication.Instance.bus.Publish(scaricoFotoMsg);
			}

			_giornale.Info( "Terminato di lavorare " + _listaFiles.Count + " foto appena acqusite" );

			incrementaTotaleFotoScaricate( tempoScarico );
		}

		/** Quando ho finito di scaricar le foto, aggiorno il totale in apposita tabella */
		private void incrementaTotaleFotoScaricate( DateTime tempoScarico ) {

			ScaricoCard scaricoCard = new ScaricoCard();
			scaricoCard.id = Guid.NewGuid();
			scaricoCard.totFoto = (short)numeroFotoAcquisite();

			scaricoCard.fotografo = this._fotografo;
			scaricoCard.tempo = tempoScarico;  // Deve essere uguale al tempo indicato sulla fotografia	
			scaricoCard.giornata = LumenApplication.Instance.stato.giornataLavorativa;

			IEntityRepositorySrv<ScaricoCard> erep = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<ScaricoCard>>();
			erep.addNew( scaricoCard );
			erep.saveChanges();
		}



		/**
		 * dato il nome del file della immagine, creo l'oggetto Fotografia e lo aggiungo al suo contenitore
		 * (in pratica faccio una insert nel database).
		 */
		private Fotografia aggiungiFoto( FileInfo fileInfo, int numFotogramma, DateTime tempoScarico ) {

			// Ad ogni foto persisto.
			// Se per esempio ho 500 foto da salvare, non posso permettermi che se una salta, perdo anche le altre 499 !
			bool success = false;
			Fotografia foto = null;

			using (TransactionScope transaction = new TransactionScope()) {

				LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
				try {

					foto = new Fotografia();
					foto.id = Guid.NewGuid();
					foto.dataOraAcquisizione = tempoScarico;
					foto.fotografo = _fotografo;
					foto.evento = _evento;
					foto.didascalia = _paramScarica.flashCardConfig.didascalia;
					foto.numero = numFotogramma;
					foto.faseDelGiorno = (short?) _paramScarica.faseDelGiorno;
					foto.giornata = LumenApplication.Instance.stato.giornataLavorativa;

					// il nome del file, lo memorizzo solamente relativo
					// scarto la parte iniziale di tutto il path togliendo il nome della cartella di base delle foto.
					// Questo perché le stesse foto le devono vedere altri computer della rete che
					// vedono il percorso condiviso in maniera differente.
					foto.nomeFile = PathUtil.nomeRelativoFoto( fileInfo );

					caricaMetadatiImmagine( fileInfo.FullName, foto );

					objContext.Fotografie.Add( foto );
					
					objContext.SaveChanges();

					// Mark the transaction as complete.
					transaction.Complete();
					success = true;
					++conta;
					
					_giornale.Debug( "Inserita nuova foto: " + foto.ToString() + " ora sono " + conta );

				} catch( Exception ee ) {
					_giornale.Error( "Non riesco ad inserire una foto. Nel db non c'è ma nel filesystem si: " + fileInfo, ee );
				}


				// TODO forse non serve neanche. Credo che venga salvato tutto ugualmente.
				// TODO provare a togliere.
				if( success )
					objContext.ObjectContext.AcceptAllChanges();
			}

			return foto;
		}

		/** Leggendo l'immagine indicata dall'attributo "nomeFile", 
		 * cerco di caricare almeno la data di scatto, e qualche altro
		 * dato interessante
		 */
		private void caricaMetadatiImmagine( string nomeFile, Fotografia foto ) {

			try {

				// Instantiate the reader
				ExifReader reader = new ExifReader( nomeFile );

				DateTime dateTime;
				if( reader.GetTagValue<DateTime>( ExifTags.DateTime, out dateTime ) ) {
					foto.dataOraScatto = dateTime;
				}

				// Gestisco eventuale auto rotazione
				if( Configurazione.UserConfigLumen.autoRotazione ) {
					ushort orientamento;
					if( reader.GetTagValue<ushort>( ExifTags.Orientation, out orientamento ) ) {

						Ruota ruota = null;
						if( orientamento == 6 ) {
							ruota = new Ruota( 90f );
						} else if( orientamento == 8 ) {
							ruota = new Ruota( -90f );
						}

						if( ruota != null ) {
							fotoRitoccoSrv.autoRuotaSuOriginale( foto, ruota );
						}
					}
				}

			} catch( Exception ) {
				// Pazienza se non ho informazioni exif vado avanti ugualmente.
			}
		}

		IFotoRitoccoSrv fotoRitoccoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IFotoRitoccoSrv>();
			}
		}

	}
}
