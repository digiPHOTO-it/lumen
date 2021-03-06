﻿using log4net;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Servizi.Stampare;
using System;
using System.Linq;
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
using Digiphoto.Lumen.Util;
using System.IO;
using System.Windows.Threading;
using Digiphoto.Lumen.Config;
using System.Text.RegularExpressions;

namespace Digiphoto.Lumen.Imaging.Wic.Stampe {

	public class EsecutoreStampaWic : IEsecutoreStampa {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( EsecutoreStampaWic ) );

		private EsitoStampa _esito;

		private long _conta = 0;

		public EsecutoreStampaWic() {
		}

		

		/**
		 * Attenzione:
		 * questo metodo deve ritornare l'esito della stampa, quindi non deve essere asincrono.
		 * Deve essere sicronizzato
		 */
		public EsitoStampa esegui( LavoroDiStampa lavoroDiStampa ) {

			LavoroDiStampaFoto _lavoroDiStampa = lavoroDiStampa as LavoroDiStampaFoto;

			_giornale.Debug( "Sto per avviare il lavoro di stampa: " + lavoroDiStampa.ToString() );

			_conta++;

			try {

				string nomeFileFoto = AiutanteFoto.idrataImmagineDaStampare( _lavoroDiStampa.fotografia );

				// Ricavo l'immagine da stampare
				IImmagine immagine = _lavoroDiStampa.fotografia.imgRisultante != null ? _lavoroDiStampa.fotografia.imgRisultante : _lavoroDiStampa.fotografia.imgOrig;

				// Gestisco una eccezione specifica, in questo modo ho un messaggio chiaro di cosa è andato storto.
				if( immagine == null )
					throw new FileNotFoundException( "fotografia = " + _lavoroDiStampa.fotografia, _lavoroDiStampa.fotografia.nomeFile );

				// Devo clonare l'immagine perché negli atri thread potrebbero eseguire delle dispose che me la svuotano
				using( IImmagine immagineDaStampare = (IImmagine)immagine.Clone() ) {

					// TODO BLUCA provo a duplicare l'immagine per evitare l'errore che è di proprietà del thread chiamante.
					// BitmapSource bmp = new WriteableBitmap( ((ImmagineWic)immagineDaStampare).bitmapSource );
					// bmp.Freeze();

					BitmapSource bmp = ((ImmagineWic)immagineDaStampare).bitmapSource;

                    var match = Regex.Match(lavoroDiStampa.param.nomeStampante, @"(?<machine>\\\\.*?)\\(?<queue>.*)");
                    PrintServer ps1 = null;
                    if (match.Success)
                    {
                        // Come print-server uso il server di rete
                        ps1 = new PrintServer(match.Groups["machine"].Value);
                    }
                    else
                    {
                        // Come print-server uso me stesso
                        ps1 = new PrintServer();
                    }
                    using ( ps1 ) {
                        PrintQueue coda = null;
                        if (match.Success)
                        {
                            coda = ps1.GetPrintQueue(match.Groups["queue"].Value);
                        }
                        else
                        {
                            coda = ps1.GetPrintQueue(lavoroDiStampa.param.nomeStampante);
                        }

                        // Ricavo la coda di stampa (cioè la stampante) e le sue capacità.
                        using ( coda ) {

							PrintCapabilities capabilities = null;
							try {
								capabilities = coda.GetPrintCapabilities();
							} catch( Exception ) {
								// Le stampanti shinko non supportano
							}

							// Imposto la stampante (così che mi carica le impostazioni)
							PrintDialog dialog = new PrintDialog();
							dialog.PrintQueue = coda;

							Size areaStampabile = new Size( dialog.PrintableAreaWidth, dialog.PrintableAreaHeight );

							// Imposto qualche attributo della stampa
							bool piuRealistaDelRe = false;
							if( piuRealistaDelRe ) { // Meglio non essere più realisti del re.
								dialog.PrintTicket.OutputQuality = OutputQuality.Photographic;
								dialog.PrintTicket.PhotoPrintingIntent = PhotoPrintingIntent.PhotoBest;
							}

							// Compongo il titolo della stampa che comparirà nella descrizione della riga nello spooler di windows
							StringBuilder titolo = new StringBuilder();
							titolo.AppendFormat( "foto N.{0} Oper={1} gg={2}",
												 _lavoroDiStampa.fotografia.etichetta,
												 _lavoroDiStampa.fotografia.fotografo.iniziali,
												 String.Format( "{0:dd-MMM}", _lavoroDiStampa.fotografia.dataOraAcquisizione ) );
#if DEBUG
							titolo.Append( " #" );
							titolo.Append( DateTime.Now.ToString( "mmssss" ) );  // Uso un numero univoco per evitare doppioni per il doPdf altrimenti mi chiede sempre di sovrascrivere
#endif
							// Eventuale rotazione dell'orientamento dell'area di stampa
							// Devo decidere in anticipo se la stampante va girata. Dopo che ho chiamato Print non si può più fare !!!
							bool _ruotareStampante = false;
							if( _lavoroDiStampa.param.autoRuota )
								if( !ProiettoreArea.isStessoOrientamento( areaStampabile, immagineDaStampare ) )
									_ruotareStampante = true;

							if( _ruotareStampante ) {

								if( capabilities != null && capabilities.PageOrientationCapability.Contains( PageOrientation.Landscape ) && capabilities.PageOrientationCapability.Contains( PageOrientation.Portrait ) ) {
									// tutto ok
									dialog.PrintTicket.PageOrientation = (dialog.PrintTicket.PageOrientation == PageOrientation.Landscape ? PageOrientation.Portrait : PageOrientation.Landscape);
								} else
									_giornale.Debug( "La stampante " + lavoroDiStampa.param.nomeStampante + " non accetta cambio orientamento landscape/portrait" );

								// Quando giro la stampante, non mi si girano anche le dimensioni. Ci penso da solo.
								areaStampabile = ProiettoreArea.ruota( areaStampabile );
							}

							//
							// ----- gestisco il numero di copie
							//
							int cicliStampa = 1;
							if( lavoroDiStampa.param.numCopie > 1 ) {
								// Se la stampante gestisce le copie multiple, faccio un invio solo.
								if( capabilities != null && capabilities.MaxCopyCount >= lavoroDiStampa.param.numCopie )
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
							document.DocumentPaginator.PageSize = new Size( dialog.PrintableAreaWidth, dialog.PrintableAreaHeight );


							// Creo una pagina della grandezza massima
							FixedPage page1 = new FixedPage();
							page1.Width = document.DocumentPaginator.PageSize.Width;
							page1.Height = document.DocumentPaginator.PageSize.Height;
							page1.VerticalAlignment = VerticalAlignment.Center;
							page1.HorizontalAlignment = HorizontalAlignment.Center;


							// Per fare in modo che l'immagine venga centrata bene automaticamente, e non venga tagliata solo da una parte ma nel centro,
							// non devo mettere le dimensioni al componente Image, ma devo creare 
							// una Grid più esterna con le dimensioni precise.
							Grid grid = new Grid();
							grid.Height = page1.Height;
							grid.Width = page1.Width;


							// Creo una immagine che contiene la bitmap da stampare
							Image image = new Image();
							//						image.BeginInit();
							image.VerticalAlignment = VerticalAlignment.Center;
							image.HorizontalAlignment = HorizontalAlignment.Center;

							//						BitmapSource clone = bmp.Clone();
							//						clone.Freeze();
							image.Source = bmp;

							if( _lavoroDiStampa.param.autoZoomNoBordiBianchi )
								image.Stretch = Stretch.UniformToFill;
							else
								image.Stretch = Stretch.Uniform;
							image.StretchDirection = StretchDirection.Both;

							//						image.EndInit();

							grid.Children.Add( image );
							page1.Children.Add( grid );

							//
							eventualiStampigli( page1, _lavoroDiStampa );


							// add the page to the document
							PageContent page1Content = new PageContent();
							page1Content.Child = page1;
							document.Pages.Add( page1Content );

							//
							// ----- STAMPA per davvero
							//
							for( int ciclo = 0; ciclo < cicliStampa; ciclo++ ) {

								dialog.PrintDocument( document.DocumentPaginator, titolo.ToString() );

								_esito = EsitoStampa.Ok;
							}
							_giornale.Debug( "Stampa completata" );

							// Per cercare di liberare memoria più possibile svuoto le pagine  forzatamente a mano.
							// Pare che il GC non riesce a pulire.
							foreach( var fixedPage in document.Pages.Select( pageContent => pageContent.Child ) ) {
								fixedPage.Children.Clear();
							}

						} // using printqueue
					} // using printserver
				} // using iimagine

			} catch( Exception ee ) {
				_esito = EsitoStampa.Errore;
				_giornale.Error( "Stampa fallita", ee );
			} finally {
				// Rilascio le immagini idratate altrimenti in loop vado in outOfMemory
				AiutanteFoto.disposeImmagini( _lavoroDiStampa.fotografia, IdrataTarget.Risultante );
				AiutanteFoto.disposeImmagini( _lavoroDiStampa.fotografia, IdrataTarget.Originale );

				CoreUtil.abraCadabra();   //   :-)
			}
		
			
			_giornale.Info( "Completato lavoro di stampa. Esito = " + _esito + " lavoro = " + lavoroDiStampa.ToString() );
			return _esito;
		}



		private static void eventualiStampigli( FixedPage page1, LavoroDiStampaFoto lavoroDiStampa ) {

			SolidColorBrush coloreFg = new SolidColorBrush( Colors.Black );
			SolidColorBrush coloreBg = new SolidColorBrush( Colors.White );

			int stampigliMarginBotton = Configurazione.UserConfigLumen.stampigliMarginBottom;
			int stampigliMarginTop = Configurazione.UserConfigLumen.stampigliMarginBottom;
			int stampigliMarginRight = Configurazione.UserConfigLumen.stampigliMarginRight;
			int stampigliMarginLeft = Configurazione.UserConfigLumen.stampigliMarginRight;


			// Prima era 30 poi 16 poi l'ho reso esterno
			int fontSize = Configurazione.UserConfigLumen.fontSizeStampaFoto;
			if( fontSize <= 0 )
				fontSize = 14; // Default


			// Numero della foto
			if( lavoroDiStampa.param.stampigli.numFoto ) {
				TextBlock textNumero = new TextBlock();
				textNumero.Text = lavoroDiStampa.fotografia.etichetta;
				textNumero.FontSize = fontSize; // 30pt text
				textNumero.Foreground = coloreFg;
				textNumero.Background = coloreBg;
				FixedPage.SetBottom( textNumero, stampigliMarginBotton );
				FixedPage.SetRight( textNumero, stampigliMarginRight );
				page1.Children.Add( textNumero );
			}

			// Giornata
			if( lavoroDiStampa.param.stampigli.giornata ) {
				TextBlock textGiorno = new TextBlock();
				textGiorno.Text = lavoroDiStampa.fotografia.giornata.ToString( "d" );
				textGiorno.FontSize = fontSize; // 30pt text
				textGiorno.Foreground = coloreFg;
				textGiorno.Background = coloreBg;
				FixedPage.SetBottom( textGiorno, stampigliMarginBotton );
				FixedPage.SetLeft( textGiorno, stampigliMarginLeft );
				page1.Children.Add( textGiorno );
			}

			// Operatore
			if( lavoroDiStampa.param.stampigli.operatore ) {
				TextBlock textOperatore = new TextBlock();
				textOperatore.Text = lavoroDiStampa.fotografia.fotografo.iniziali;
				textOperatore.FontSize = fontSize; // 30pt text
				textOperatore.Foreground = coloreFg;
				textOperatore.Background = coloreBg;
				FixedPage.SetTop( textOperatore, stampigliMarginTop );
				FixedPage.SetRight( textOperatore, stampigliMarginRight );
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



		/// <summary>
		/// Mi dice quale tipo di parametri è in grado di gestire.
		/// </summary>
		public Type tipoParamGestito {
			get {
				return typeof( ParamStampaFoto );
			}
		}


		private BitmapSource CreateImageSource( Stream stream ) {
			BitmapImage bi = new BitmapImage();
			bi.BeginInit();
			bi.CacheOption = BitmapCacheOption.OnLoad;
			bi.StreamSource = stream;
			bi.EndInit();
			bi.Freeze();
			BitmapSource prgbaSource = new FormatConvertedBitmap( bi, PixelFormats.Pbgra32, null, 0 );
			WriteableBitmap bmp = new WriteableBitmap( prgbaSource );
			int w = bmp.PixelWidth;
			int h = bmp.PixelHeight;
			int[] pixelData = new int[w * h];
			//int widthInBytes = 4 * w;
			int widthInBytes = bmp.PixelWidth * (bmp.Format.BitsPerPixel / 8); //equals 4*w
			bmp.CopyPixels( pixelData, widthInBytes, 0 );
			bmp.WritePixels( new Int32Rect( 0, 0, w, h ), pixelData, widthInBytes, 0 );
			bi = null;
			return bmp;
		}

	}
}
