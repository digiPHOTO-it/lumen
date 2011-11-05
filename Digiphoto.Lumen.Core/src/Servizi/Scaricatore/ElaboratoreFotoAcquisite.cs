﻿using System;
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

		private IList<FileInfo> _listaFiles;

		private ParamScarica _paramScarica;

		private Fotografo _fotografo;

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


		public void elabora() {

			// carico il fotografo che rimane uguale per tutta questa sessione di elaborazione
			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
			_fotografo = objContext.Fotografi.Single<Fotografo>( ff => ff.id == _paramScarica.flashCardConfig.idFotografo );

			_giornale.Debug( "Sto per lavorare le " + _listaFiles.Count + " foto appena acquisite di " + _fotografo.id );

			int ultimoNumFoto = NumeratoreFotogrammi.incrementaNumeratoreFoto( _listaFiles.Count );
			int conta = 0;

			foreach( FileInfo fileInfo in _listaFiles ) {

				Fotografia foto = aggiungiEntitaFoto( fileInfo, ++conta + ultimoNumFoto );

				RitoccoUtil.creaProvinoFoto( fileInfo.FullName, foto );

				// Quando sono a posto con la foto, sollevo un evento per avvisare tutti
				NuovaFotoMsg msg = new NuovaFotoMsg( foto );
				LumenApplication.Instance.bus.Publish( msg );
			}

			_giornale.Info( "Terminato di lavorare " + _listaFiles.Count + " foto appena acqusite" );

			incrementaTotaleFotoScaricate();
		}

		/** Quando ho finito di scaricar le foto, aggiorno il totale in apposita tabella */
		private void incrementaTotaleFotoScaricate() {

			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
			ScaricoCard scaricoCard = new ScaricoCard();
			scaricoCard.id = Guid.NewGuid();
			scaricoCard.totFoto = (short)numeroFotoAcquisite();

			scaricoCard.fotografo = this._fotografo;
			scaricoCard.tempo = DateTime.Now;

			objContext.ScarichiCards.AddObject( scaricoCard );

			objContext.SaveChanges();
		}



		/**
		 * dato il nome del file della immagine, creo l'oggetto Fotografia e lo aggiungo al suo contenitore
		 * (in pratica faccio una insert nel database).
		 */
		private Fotografia aggiungiEntitaFoto( FileInfo fileInfo, int numFotogramma ) {

			// Ad ogni foto persisto.
			// Se per esempio ho 500 foto da salvare, non posso permettermi che se una salta, perdo anche le altre 499 !
			bool success = false;
			Fotografia foto = null;

			using (TransactionScope transaction = new TransactionScope()) {

				LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
				try {

					Evento evento = null;
					if( _paramScarica.flashCardConfig.idEvento != null && _paramScarica.flashCardConfig.idEvento != Guid.Empty )
						evento = objContext.Eventi.FirstOrDefault<Evento>( ee => ee.id == _paramScarica.flashCardConfig.idEvento );

					foto = new Fotografia();
					foto.id = Guid.NewGuid();
					// foto.dataOraScatto =   TODO prendere dai dati exif.
					foto.dataOraAcquisizione = fileInfo.CreationTime;
					foto.fotografo = _fotografo;
					foto.evento = evento;
					foto.didascalia = _paramScarica.flashCardConfig.didascalia;
					foto.numero = numFotogramma;
					foto.faseDelGiorno = (short) _paramScarica.faseDelGiorno;
					foto.giornata = LumenApplication.Instance.stato.giornataLavorativa;

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
