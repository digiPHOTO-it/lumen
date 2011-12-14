using log4net;
using Digiphoto.Lumen.Imaging;
using System.Drawing.Printing;
using System.Drawing;
using Digiphoto.Lumen.Servizi.Stampare;
using System.Drawing.Drawing2D;
using System;
using System.Diagnostics;

namespace Digiphoto.Lumen.Imaging.Nativa {

	public class EsecutoreStampaNet : IEsecutoreStampa {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( EsecutoreStampaNet ) );

		private LavoroDiStampa _lavoroDiStampa;
		private bool _ruotareStampante = false;
		private EsitoStampa _esito;

		public EsecutoreStampaNet() {
		}

		#region Proprietà
		/**
		 * Ricavo l'immagine che andrà veramente in stampa
		 */
		private Immagine immagineDaStampare {
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


		public EsitoStampa esegui( LavoroDiStampa lavoroDiStampa ) {

			_lavoroDiStampa = lavoroDiStampa;
			_giornale.Debug( "Sto per avviare il lavoro di stampa: " + lavoroDiStampa.ToString() );


			try {	        
		
				// Creo il documento di stampa
				PrintDocument pd = new PrintDocument();
				pd.PrinterSettings.PrinterName = lavoroDiStampa.param.nomeStampante;
				pd.DocumentName = "foto N." + lavoroDiStampa.fotografia.numero + " Oper=" + lavoroDiStampa.fotografia.fotografo.iniziali + " gg=" + String.Format( "{0:dd-MMM}", lavoroDiStampa.fotografia.dataOraAcquisizione );

				pd.PrintPage += new PrintPageEventHandler( mioPrintPageEventHandler );
				pd.BeginPrint += new PrintEventHandler( pd_BeginPrint );
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

			} catch( Exception ee ) {
				_esito = EsitoStampa.Errore;
				_giornale.Error( "Stampa fallita", ee );
			}

			_giornale.Info( "Completato lavoro di stampa. Esito = " + _esito + " lavoro = " + lavoroDiStampa.ToString() );
			return _esito;
		}

		void pd_BeginPrint( object sender, PrintEventArgs e ) {

			_giornale.Debug( e.PrintAction.ToString() );
		}

		/**
		 * Evento che viene rilanciato per stampare tutte le pagine
		 */
		void mioPrintPageEventHandler( object sender, PrintPageEventArgs e ) {

			try {

				// Ricavo l'immagine da stampare
				Immagine immagineDaStampare = _lavoroDiStampa.fotografia.imgOrig;


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

	}
}
