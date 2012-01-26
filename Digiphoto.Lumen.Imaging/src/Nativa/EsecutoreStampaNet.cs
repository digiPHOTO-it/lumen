using log4net;
using Digiphoto.Lumen.Imaging;
using System.Drawing.Printing;
using System.Drawing;
using Digiphoto.Lumen.Servizi.Stampare;
using System.Drawing.Drawing2D;
using System;
using System.Diagnostics;
using System.Threading;

namespace Digiphoto.Lumen.Imaging.Nativa {

	public class EsecutoreStampaNet : IEsecutoreStampa {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( EsecutoreStampaNet ) );

		private LavoroDiStampa _lavoroDiStampa;
		private bool _ruotareStampante = false;
		private EsitoStampa _esito;

		/** Mi serve per attendere la fine della stampa
		 * Per spiegazione vedere
		 * http://msdn.microsoft.com/en-us/library/system.threading.manualresetevent.aspx
		 * http://stackoverflow.com/questions/492020/c-net-blocking-and-waiting-for-an-event
		 */
		private ManualResetEvent fineStampaManualResetEvent;

		public EsecutoreStampaNet() {
			fineStampaManualResetEvent = new ManualResetEvent(false);
		}

		#region Proprietà
		/**
		 * Ricavo l'immagine che andrà veramente in stampa
		 */
		private IImmagine immagineDaStampare {
			get {
				// TODO: non è vero. Sistemare quando ci saranno le correzioni
				return _lavoroDiStampa.fotografia.imgOrig;
			}
		}

		private Image imageDaStampare {
			get {
				return ((ImmagineNet)immagineDaStampare).image;
			}
		}
		#endregion


		/**
		 * Attenzione:
		 * questo metodo deve ritornare l'esito della stampa, quindi non deve essere asincrono.
		 * Deve essere sicronizzato
		 */
		public EsitoStampa esegui( LavoroDiStampa lavoroDiStampa ) {

			_lavoroDiStampa = lavoroDiStampa;
			_giornale.Debug( "Sto per avviare il lavoro di stampa: " + lavoroDiStampa.ToString() );


			try {	        
		
				// Creo il documento di stampa
				PrintDocument pd = new PrintDocument();
				pd.PrinterSettings.PrinterName = lavoroDiStampa.param.nomeStampante;
				pd.DocumentName = "foto N." + lavoroDiStampa.fotografia.numero + " Oper=" + lavoroDiStampa.fotografia.fotografo.iniziali + " gg=" + String.Format( "{0:dd-MMM}", lavoroDiStampa.fotografia.dataOraAcquisizione );
				if( lavoroDiStampa.param.idRigaCarrello.Equals( Guid.Empty ) == false )
					pd.DocumentName += " [" + lavoroDiStampa.param.idRigaCarrello + "]";

				pd.PrintPage += new PrintPageEventHandler( mioPrintPageEventHandler );
				pd.EndPrint += new PrintEventHandler( fineStampaEventHandler );

				// Eventuale rotazione dell'orientamento dell'area di stampa
				determinaRotazione( pd );


				// ----- gestisco il numero di copie
				int cicliStampa = 1;
				if( lavoroDiStampa.param.numCopie > 1 ) {
					// Se la stampante gestisce le copie multiple, faccio un invio solo.
					if( pd.PrinterSettings.MaximumCopies >= lavoroDiStampa.param.numCopie )
						pd.PrinterSettings.Copies = lavoroDiStampa.param.numCopie;
					else
						cicliStampa = lavoroDiStampa.param.numCopie;
				}


				//
				// ----- STAMPA per davvero
				//
				for( int ciclo = 0; ciclo < cicliStampa; ciclo++ ) {
					pd.Print();
				}

				// Attendo che il thread di stampa finisca davvero
				fineStampaManualResetEvent.WaitOne();

				_giornale.Debug( "Stampa completata" );
					
			} catch( Exception ee ) {
				_esito = EsitoStampa.Errore;
				_giornale.Error( "Stampa fallita", ee );
			}

			_giornale.Info( "Completato lavoro di stampa. Esito = " + _esito + " lavoro = " + lavoroDiStampa.ToString() );
			return _esito;
		}



		/**
		 * Evento che viene rilanciato per stampare tutte le pagine
		 */
		void mioPrintPageEventHandler( object sender, PrintPageEventArgs e ) {

			try {

				// Ricavo l'immagine da stampare
				IImmagine immagineDaStampare = _lavoroDiStampa.fotografia.imgOrig;


				// Ricavo tutta l'area stampabile
				RectangleF areaStampabile = e.PageSettings.PrintableArea;

				// Non so perchè, ma quando giro la stampante, non mi si girano anche le dimensioni. Ci penso da solo.
				if( _ruotareStampante )
					areaStampabile = ProiettoreArea.ruota( areaStampabile );

				//
				// ----- Calcolo le aree di proiezione
				//

				ProiettoreArea proiettore = new ProiettoreArea( areaStampabile );
				proiettore.autoCentra = true;    // Questo lo decido io d'ufficio. Non avrebbe senso altrimenti.
				proiettore.autoZoomToFit = _lavoroDiStampa.param.autoZoomToFit;
				proiettore.autoRotate = _lavoroDiStampa.param.autoRuota;

				Proiezione proiezione = proiettore.calcola( immagineDaStampare );


				//
				// ----- Inizio a stampare
				//

				// la rotazione non dovrebbe mai avvenire perchè ho già girato la stampante in anticipo
				Debug.Assert( !proiezione.effettuataRotazione );

				Graphics grpx = e.Graphics;

				//set graphics attributes.
				grpx.SmoothingMode = SmoothingMode.AntiAlias;
				grpx.InterpolationMode = InterpolationMode.HighQualityBicubic;
				grpx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				grpx.PixelOffsetMode = PixelOffsetMode.HighQuality;

				Image image = ((ImmagineNet)immagineDaStampare).image;
				grpx.DrawImage( image, proiezione.dest, proiezione.sorg, GraphicsUnit.Pixel );

				_esito = EsitoStampa.Ok;

			} catch( Exception ee ) {
				_giornale.Error( "Lavoro di stampa fallito: " + _lavoroDiStampa, ee );
				_esito = EsitoStampa.Errore;
			}

		}

		void fineStampaEventHandler( object sender, PrintEventArgs e ) {
			fineStampaManualResetEvent.Set();
		}

		/**
		 * Se la foto e la stampante non sono orientate nello stesso verso,
		 * allora devo uniformarle.
		 */
		private void determinaRotazione( PrintDocument pd ) {

			_ruotareStampante = false;

			// Devo decidere in anticipo se la stampante va girata. Dopo che ho chiamato Print non si può più fare !!!
			if( !_lavoroDiStampa.param.autoRuota )
				return;

			// Entrambe orizzontali.
			if( pd.PrinterSettings.DefaultPageSettings.Landscape && immagineDaStampare.orientamento == Orientamento.Orizzontale )
				return;

			// Entrambe verticali
			if( !pd.PrinterSettings.DefaultPageSettings.Landscape && immagineDaStampare.orientamento == Orientamento.Verticale )
				return;

			// Ok sono diverse.
			pd.DefaultPageSettings.Landscape = (!pd.DefaultPageSettings.Landscape);
			_ruotareStampante = true;
		}


		public bool asincrono {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}
	}
}
