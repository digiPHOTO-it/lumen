using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Transactions;
using log4net;
using Digiphoto.Lumen.Database;
using System.Data.Objects;
using Digiphoto.Lumen.src.Database;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging;
using System.Reflection;

namespace Digiphoto.Lumen.Servizi.Scaricatore {

	class ElaboratoreFotoAcquisite {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( ElaboratoreFotoAcquisite ) );

		private List<FileInfo> _listaFiles;
		private ParamScarica _paramScarica;

		public int conta {
			get;
			private set;
		}

		public ElaboratoreFotoAcquisite( List<FileInfo> list, ParamScarica paramScarica ) {

			this._listaFiles = list;
			this._paramScarica = paramScarica;
		}


		public void elaboora() {

			_giornale.Debug( "Sto per lavorare le " + _listaFiles.Count + " foto appena acquisite" );
	

			foreach( FileInfo fileInfo in _listaFiles ) {

				Fotografia foto = aggiungiEntitaFoto( fileInfo );

				creaProvinoImmagine( fileInfo.FullName, foto );

				// Quando sono a posto con la foto, sollevo un evento per avvisare tutti
				NuovaFotoMsg msg = new NuovaFotoMsg( foto );
				LumenApplication.Instance.bus.Publish( msg );
			}

			_giornale.Info( "Terminato di lavorare " + _listaFiles.Count + " foto appena acqusite" );
		}

		/**
		 * Siccome alcuni attributi Immagine non risiedono nel db,
		 * li devo gestire sul filesystem.
		 * Siccome è la priima volta, li creo.
		 * 
		 * TODO rendere questa funzionalità generica e comune sia a quando viene creata la foto per la prima volta,
		 *      sia quando la carico da disco esistente, sia quando faccio "torna originale".
		 *      Occhio che forse l'evento di creazione nuovo provino va lanciato qui dentro.
		 */
		private void creaProvinoImmagine( Fotografia foto ) {
			creaProvinoImmagine( PathUtil.nomeCompletoFoto( foto ), foto );
		}

		private void creaProvinoImmagine( string nomeFileFoto, Fotografia foto ) {

			IGestoreImmagineSrv gis = LumenApplication.Instance.getGestoreImmaginiSrv();


			Immagine immagineGrande = gis.load( nomeFileFoto );
			foto.imgOrig = immagineGrande;

			// TODO l'immagine risultante non ho ancora deciso se e come la gestirò.
			// per ora non faccio niente
			foto.imgRisultante = null; // immagineGrande;

			Immagine immaginePiccola = gis.creaProvino( immagineGrande );
			foto.imgProvino = immaginePiccola;
			gis.save( immaginePiccola, PathUtil.nomeCompletoProvino( foto ) );

		}

		/**
		 * dato il nome del file della immagine, creo l'oggetto Fotografia e lo aggiungo al suo contenitore
		 * (in pratica faccio una insert nel database).
		 */
		private Fotografia aggiungiEntitaFoto( FileInfo fileInfo ) {

			// Ad ogni foto persisto.
			// Se per esempio ho 500 foto da salvare, non posso permettermi che se una salta, perdo anche le altre 499 !
			bool success = false;
			Fotografia foto = null;

			using (TransactionScope transaction = new TransactionScope()) {

				LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
				try {

					// potrebbe essere staccato. Lo riattacco */
					Fotografo fotografo = _paramScarica.flashCardConfig.fotografo;
					OrmUtil.AttachToOrGet<Fotografo>( objContext, "Fotografi", ref fotografo );

					Evento evento = _paramScarica.flashCardConfig.evento;
					OrmUtil.AttachToOrGet<Evento>( objContext, "Eventi", ref evento );


					foto = new Fotografia();
					foto.id = Guid.NewGuid();
					// foto.dataOraScatto =   TODO prendere dai dati exif.
					foto.dataOraAcquisizione = fileInfo.CreationTime;
					foto.fotografo = fotografo;
					foto.evento = evento;
					foto.didascalia = _paramScarica.flashCardConfig.didascalia;

					// il nome del file, lo memorizzo solamente relativo
					// scarto la parte iniziale di tutto il path togliendo il nome della cartella di base delle foto.
					// Questo perché le stesse foto le devono vedere altri computer della rete che
					// vedono il percorso condiviso in maniera differente.
					foto.nomeFile = PathUtil.nomeRelativoFoto( fileInfo );

					// TODO leggere un pò di dati exif (in particolare la data-ora di scatto, orientazione )
					caricaMetadatiImmagine( foto );

					objContext.Fotografie.AddObject( foto );
					
					objContext.SaveChanges();

					// Mark the transaction as complete.
					transaction.Complete();
					success = true;
					++conta;
					
					_giornale.Debug( "Inserita nuova foto: " + foto.ToString() + " ora sono " + conta );

				} catch( Exception ee ) {
					_giornale.Error( "Non riesco ad inserire una foto. Nel db non c'è ma nel filesystem si: " + fileInfo, ee );
				}


				if( success )
					objContext.AcceptAllChanges();
			}

			return foto;
		}

		/** Leggendo l'immagine indicata dall'attributo "nomeFile", 
		 * cerco di caricare almeno la data di scatto, e qualche altro
		 * dato interessante
		 */
		private void caricaMetadatiImmagine( Fotografia foto ) {
			// TODO
			// throw new NotImplementedException();
		}
	
	}
}
