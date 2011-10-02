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
		private IProvinatore _provinatore;

		public int conta {
			get;
			private set;
		}

		public ElaboratoreFotoAcquisite( List<FileInfo> list, ParamScarica paramScarica ) {

			this._listaFiles = list;
			this._paramScarica = paramScarica;


			this._provinatore = ImagingFactory.Instance.creaProvinatore();
		}


		public void elaboora() {

			_giornale.Debug( "Sto per lavorare le " + _listaFiles.Count + " foto appena acquisite" );
	

			foreach( FileInfo fileInfo in _listaFiles ) {

				Fotografia foto = aggiungiEntitaFoto( fileInfo );

				creaProvino( fileInfo );

				// Quando sono a posto con la foto, sollevo un evento per avvisare tutti
				NuovaFotoMsg msg = new NuovaFotoMsg( foto );
				LumenApplication.Instance.bus.Publish( msg );
			}

			_giornale.Info( "Terminato di lavorare " + _listaFiles.Count + " foto appena acqusite" );
		}

		private void creaProvino( FileInfo fiFoto ) {


			

			// TODO
			//ProviniUtil provUtil = new ProviniUtil();
			//string nomeFileProvino = provUtil.creaProvino( fiFoto );

		
		}

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
					// foto.dataOraScatto =   TODO
					foto.dataOraAcquisizione = fileInfo.CreationTime;
					foto.fotografo = fotografo;
					foto.evento = evento;
					foto.didascalia = _paramScarica.flashCardConfig.didascalia;

					// il nome del file, lo memorizzo solamente relativo
					int iniz = LumenApplication.Instance.configurazione.getCartellaBaseFoto().Length;

					// scarto la parte iniziale di tutto il path togliendo il nome della cartella di base delle foto.
					foto.nomeFile = fileInfo.FullName.Substring( iniz+1 );

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
