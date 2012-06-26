using log4net;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Servizi.Stampare;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Printing;
using System.Windows.Markup;
using System.Text;
using Digiphoto.Lumen.Model;
using System.IO;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.Imaging.Wic.Stampe {

	public class EsecutoreStampaProvini : IEsecutoreStampa {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( EsecutoreStampaWic ) );

		private EsitoStampa _esito;

		private long _conta = 0;

		public EsecutoreStampaProvini() {
		}

		/**
		 * Attenzione:
		 * questo metodo deve ritornare l'esito della stampa, quindi non deve essere asincrono.
		 * Deve essere sicronizzato
		 */
		public EsitoStampa esegui( LavoroDiStampa lavoroDiStampa ) {

			LavoroDiStampaProvini _lavoroDiStampa;
			_lavoroDiStampa = (LavoroDiStampaProvini)lavoroDiStampa;

			_giornale.Debug( "Sto per avviare il lavoro di stampa: " + lavoroDiStampa.ToString() );

			_conta++;

			try {

				// Ricavo l'immagine da stampare
				//IImmagine immagineDaStampare = _lavoroDiStampa.fotografia.imgRisultante != null ? _lavoroDiStampa.fotografia.imgRisultante : _lavoroDiStampa.fotografia.imgOrig;

				
				//BitmapSource bmp = ((ImmagineWic)immagineDaStampare).bitmapSource;

				// Come print-server uso me stesso
				using( PrintServer ps1 = new PrintServer() ) {

					// Ricavo la coda di stampa (cioè la stampante) e le sue capacità.
					using( PrintQueue coda = ps1.GetPrintQueue( lavoroDiStampa.param.nomeStampante ) ) {

						PrintCapabilities capabilities = coda.GetPrintCapabilities();

						// Imposto la stampante (così che mi carica le impostazioni)
						PrintDialog dialog = new PrintDialog();
						dialog.PrintQueue = coda;

						Size areaStampabile = new Size( dialog.PrintableAreaWidth, dialog.PrintableAreaHeight );

						// Imposto qualche attributo della stampa
						if( 1 == 0 ) { // Meglio non essere più realisti del re.
							dialog.PrintTicket.OutputQuality = OutputQuality.Photographic;
							dialog.PrintTicket.PhotoPrintingIntent = PhotoPrintingIntent.PhotoBest;
						}

						// Compongo il titolo della stampa che comparirà nella descrizione della riga nello spooler di windows
						StringBuilder titolo = new StringBuilder();
						titolo.AppendFormat( "Intestazione={0} Righe={1} Colonne={2}",
							_lavoroDiStampa.param.intestazione+" "+Configurazione.UserConfigLumen.codicePuntoVendita,
							_lavoroDiStampa.param.numeroRighe,
							_lavoroDiStampa.param.numeroColonne
							);

						if( _giornale.IsDebugEnabled ) {
							titolo.Append( " #" );
							titolo.Append( _conta );
						}

						// Eventuale rotazione dell'orientamento dell'area di stampa
						// Devo decidere in anticipo se la stampante va girata. Dopo che ho chiamato Print non si può più fare !!!
						bool _ruotareStampante = false;

						if( _ruotareStampante ) {

							if( capabilities.PageOrientationCapability.Contains( PageOrientation.Landscape ) && capabilities.PageOrientationCapability.Contains( PageOrientation.Portrait ) ) {
								// tutto ok
								dialog.PrintTicket.PageOrientation = (dialog.PrintTicket.PageOrientation == PageOrientation.Landscape ? PageOrientation.Portrait : PageOrientation.Landscape);
							} else
								_giornale.Warn( "La stampante " + lavoroDiStampa.param.nomeStampante + " non accetta cambio orientamento landscape/portrait" );

							// Quando giro la stampante, non mi si girano anche le dimensioni. Ci penso da solo.
							areaStampabile = ProiettoreArea.ruota( areaStampabile );
						}

						//
						// ----- gestisco il numero di copie
						//
						int cicliStampa = 1;
						if( lavoroDiStampa.param.numCopie > 1 ) {
							// Se la stampante gestisce le copie multiple, faccio un invio solo.
							if( capabilities.MaxCopyCount >= lavoroDiStampa.param.numCopie )
								dialog.PrintTicket.CopyCount = lavoroDiStampa.param.numCopie;
							else
								cicliStampa = lavoroDiStampa.param.numCopie;
						}

						//
						// ----- Preparo la realizzazione grafica da mandare in output
						//

						// Ora creo il documento che andrò a stampare.
						// L'uso di un FixedDocument, mi permetterà di interagire con misure, dimensioni e margini
						FixedDocument document = new FixedDocument();
						document.DocumentPaginator.PageSize = new Size( dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);

						// Creo una pagina della grandezza massima
						FixedPage page1 = new FixedPage();
						page1.Width = document.DocumentPaginator.PageSize.Width;
						page1.Height = document.DocumentPaginator.PageSize.Height;
						page1.VerticalAlignment = VerticalAlignment.Center;
						page1.HorizontalAlignment = HorizontalAlignment.Center;

						Canvas c = new Canvas();
						c.Background = new SolidColorBrush(Colors.Transparent);
						c.Width = page1.Width;
						c.Height = page1.Height;
						c.HorizontalAlignment = HorizontalAlignment.Left;
						c.VerticalAlignment = VerticalAlignment.Top;

						Image img = null;

						double x = 1;
						double y = 1;

						int countFotoRighe = 0;
						int countFotoColonne = 0;

						int sizeLato = Configurazione.UserConfigLumen.pixelLatoProvino;

						if (_lavoroDiStampa.param.numeroColonne > 0)
						{
							sizeLato = (int)c.Width / _lavoroDiStampa.param.numeroColonne;
						}

						foreach (Fotografia foto in _lavoroDiStampa.fotografie)
						{
							// Ricavo l'immagine da stampare
							IImmagine fotina = foto.imgProvino;

							img = new Image();
							
							BitmapSource bmp1 = ((ImmagineWic)fotina).bitmapSource;

							img.Source = bmp1;
							img.Width = sizeLato;
							img.Height = sizeLato;
							img.HorizontalAlignment = HorizontalAlignment.Left;
							img.VerticalAlignment = VerticalAlignment.Top;
							img.Stretch = Stretch.UniformToFill;

							if ((int)(img.Width*(x)) >= (int)c.Width && 
								countFotoColonne <= _lavoroDiStampa.param.numeroColonne)
							{	
								x = 1;
								y++;
								countFotoColonne++;
							}
							// Devo cambiare pagina
							if ((int)(img.Height*(y)) >= (int)c.Height && 
								countFotoRighe >= _lavoroDiStampa.param.numeroRighe)
							{
								FixedPage pages = new FixedPage();
								pages.Width = document.DocumentPaginator.PageSize.Width;
								pages.Height = document.DocumentPaginator.PageSize.Height;
								pages.VerticalAlignment = VerticalAlignment.Center;
								pages.HorizontalAlignment = HorizontalAlignment.Center;

								pages.Children.Add(imageToCavas(c, pages.Width, pages.Height));

								//
								//eventualiStampigli(pages, _lavoroDiStampa);

								// add the page to the document
								PageContent pagesContent = new PageContent();
								((IAddChild)pagesContent).AddChild(pages);

								document.Pages.Add(pagesContent);

								x = 1;
								y = 1;

								countFotoRighe = 0;
								countFotoColonne = 0;
							}

							img.SetValue(Canvas.TopProperty, (Double)(img.Height*(y-1)));
							img.SetValue(Canvas.LeftProperty, (Double)(img.Width*(x-1)));

							x++;
							countFotoRighe++;

							c.Children.Add(img);

						}

						page1.Children.Add(imageToCavas(c, page1.Width, page1.Height));

						//
						//eventualiStampigli( page1, _lavoroDiStampa );

						// add the page to the document
						PageContent page1Content = new PageContent();
						((IAddChild)page1Content).AddChild( page1 );

						document.Pages.Add(page1Content);

						//
						// ----- STAMPA per davvero
						//
						for( int ciclo = 0; ciclo < cicliStampa; ciclo++ ) {

							dialog.PrintDocument( document.DocumentPaginator, titolo.ToString() );

							_esito = EsitoStampa.Ok;
						}

						_giornale.Debug( "Stampa completata" );
					}
				}
			} catch( Exception ee ) {
				_esito = EsitoStampa.Errore;
				_giornale.Error( "Stampa fallita", ee );
			}
		
			_giornale.Info( "Completato lavoro di stampa. Esito = " + _esito + " lavoro = " + lavoroDiStampa.ToString() );
			return _esito;
		}


		private Image imageToCavas(Canvas c, double width, double height)
		{
			//Create a Bitmap and render the content of the canvas
			// save current canvas transform
			Transform transform = c.LayoutTransform;

			// get size of control
			Size sizeOfControl = new Size(c.ActualWidth, c.ActualHeight);
			// measure and arrange the control
			c.Measure(sizeOfControl);
			// arrange the surface
			c.Arrange(new Rect(sizeOfControl));

			// craete and render surface and push bitmap to it
			//RenderTargetBitmap renderBitmap = new RenderTargetBitmap((Int32)sizeOfControl.Width, (Int32)sizeOfControl.Height, 96d, 96d, PixelFormats.Pbgra32);
			RenderTargetBitmap renderBitmap = new RenderTargetBitmap((Int32)c.Width, (Int32)c.Height, 96d, 96d, PixelFormats.Pbgra32);
			// now render surface to bitmap
			renderBitmap.Render(c);

			// encode png data
			PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
			// puch rendered bitmap into it
			pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));



			// Creo una immagine che contiene la bitmap da stampare
			Image image = new Image();
			image.Width = width;
			image.Height = height;
			image.VerticalAlignment = VerticalAlignment.Center;
			image.HorizontalAlignment = HorizontalAlignment.Center;

			image.BeginInit();
			image.Source = pngEncoder.Frames[0];
			image.EndInit();
			image.Stretch = Stretch.Uniform;
			image.StretchDirection = StretchDirection.Both;

			return image;
		}

		private static void eventualiStampigli( FixedPage page1, LavoroDiStampaProvini lavoroDiStampa ) {

			SolidColorBrush coloreFg = new SolidColorBrush( Colors.LightGray );
			SolidColorBrush coloreBg = new SolidColorBrush( Colors.White );

			// Numero della foto
			if( lavoroDiStampa.param.stampigli.numFoto ) {
				TextBlock textNumero = new TextBlock();
				//textNumero.Text = lavoroDiStampa.fotografia.numero.ToString();
				textNumero.FontSize = 6; // 30pt text
				textNumero.Foreground = coloreFg;
				textNumero.Background = coloreBg;
				FixedPage.SetTop( textNumero, 1 );
				FixedPage.SetLeft( textNumero, 1 );
				page1.Children.Add( textNumero );
			}

			// Giornata
			if( lavoroDiStampa.param.stampigli.giornata ) {
				TextBlock textGiorno = new TextBlock();
				//textGiorno.Text = lavoroDiStampa.fotografia.giornata.ToString( "d" );
				textGiorno.FontSize = 6; // 30pt text
				textGiorno.Foreground = coloreFg;
				textGiorno.Background = coloreBg;
				FixedPage.SetBottom( textGiorno, 1 );
				FixedPage.SetLeft( textGiorno, 1 );
				page1.Children.Add( textGiorno );
			}

			// Operatore
			if( lavoroDiStampa.param.stampigli.operatore ) {
				TextBlock textOperatore = new TextBlock();
				//textOperatore.Text = lavoroDiStampa.fotografia.fotografo.iniziali;
				textOperatore.FontSize = 6; // 30pt text
				textOperatore.Foreground = coloreFg;
				textOperatore.Background = coloreBg;
				FixedPage.SetBottom( textOperatore, 1 );
				FixedPage.SetRight( textOperatore, 1 );
				page1.Children.Add( textOperatore );
			}

		}

		/**
		 * Se la foto e la stampante non sono orientate nello stesso verso,
		 * allora devo uniformarle.
		 */
		private static bool determinaRotazione( Size areaStampabile, IImmagine immagineDaStampare ) {

			bool _ruotareStampante = false;

			// Entrambe orizzontali.
			// Entrambe verticali
			if( !ProiettoreArea.isStessoOrientamento( areaStampabile, immagineDaStampare ) ) {

				// Ok sono dissimili.
				_ruotareStampante = true;

			}
			return _ruotareStampante;
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
