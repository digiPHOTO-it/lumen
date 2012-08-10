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
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Collections.Generic;
using System.Collections;
using Digiphoto.Lumen.Util;
using System.Globalization;

namespace Digiphoto.Lumen.Imaging.Wic.Stampe {

	public class EsecutoreStampaProvini : IEsecutoreStampa {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( EsecutoreStampaWic ) );

		private EsitoStampa _esito;

		private long _conta = 0;

		private int testataH = 30;
		
		private int margin = 8;

		public EsecutoreStampaProvini() {
		}

		/**
		 * Attenzione:
		 * questo metodo deve ritornare l'esito della stampa, quindi non deve essere asincrono.
		 * Deve essere sicronizzato
		 */
		public EsitoStampa esegui( LavoroDiStampa lavoroDiStampa ) {

			LavoroDiStampaProvini _lavoroDiStampa = lavoroDiStampa as LavoroDiStampaProvini;

			_giornale.Debug( "Sto per avviare il lavoro di stampa: " + lavoroDiStampa.ToString() );

			_conta++;

			try {
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
							_lavoroDiStampa.param.intestazione+" "+Configurazione.infoFissa.idPuntoVendita,
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

						int sizeLatoH = Configurazione.infoFissa.pixelProvino;
						int sizeLatoW = Configurazione.infoFissa.pixelProvino;

						int numFotPag = _lavoroDiStampa.fotografie.Count;

						if (_lavoroDiStampa.param.numeroRighe > 0 && _lavoroDiStampa.param.numeroColonne > 0)
						{
							sizeLatoH = (int)((document.DocumentPaginator.PageSize.Height - testataH) / _lavoroDiStampa.param.numeroRighe);

							sizeLatoW = (int)(document.DocumentPaginator.PageSize.Width / _lavoroDiStampa.param.numeroColonne);

							numFotPag = _lavoroDiStampa.param.numeroRighe * _lavoroDiStampa.param.numeroColonne;
							}
						else
						{	
							double volume = (document.DocumentPaginator.PageSize.Width) * (document.DocumentPaginator.PageSize.Height - testataH);
							double volumeProvino = volume / _lavoroDiStampa.fotografie.Count;
							double c = document.DocumentPaginator.PageSize.Width /(document.DocumentPaginator.PageSize.Height - testataH) ;

							// Utilizzo le foto con il lato lungo
							sizeLatoW = (int)Math.Sqrt(volumeProvino / c);
							sizeLatoH = (int)(sizeLatoW * c);
							double volumeProvino2 = sizeLatoH * sizeLatoW;
							double errore = volumeProvino - volumeProvino2;

							int numFotH = (int)Math.Ceiling((document.DocumentPaginator.PageSize.Height - testataH) / sizeLatoH);

							int numFotW = (int)Math.Ceiling(document.DocumentPaginator.PageSize.Width / sizeLatoW);

							sizeLatoH = (int)((document.DocumentPaginator.PageSize.Height - testataH) / numFotH);

							sizeLatoW = (int)(document.DocumentPaginator.PageSize.Width / numFotW);

							}

						int numPag = numFotPag > 0 ? (int)Math.Ceiling((decimal)_lavoroDiStampa.fotografie.Count / numFotPag) : 1;

						_lavoroDiStampa.param.numPag = (short)numPag;

						List<Fotografia> list = (List<Fotografia>)_lavoroDiStampa.fotografie;

						IEnumerator<Fotografia> iterator = _lavoroDiStampa.fotografie.GetEnumerator();

						for (int pagStampate = 0; pagStampate < numPag; pagStampate++)
						{
							int numFoto = 0;
							IList<Fotografia> fotos = new List<Fotografia>();
							while (numFoto < numFotPag && iterator.MoveNext())
							{
								fotos.Add(iterator.Current);
								numFoto++;
							}

							stampaUnaPagina(document, fotos, _lavoroDiStampa.param, sizeLatoW, sizeLatoH);

						}

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

		private void stampaUnaPagina(FixedDocument document, IList<Fotografia> fotos, ParamStampaProvini param, int sizeLatoW, int sizeLatoH)
		{
			FixedPage page = new FixedPage();
			page.Width = document.DocumentPaginator.PageSize.Width;
			page.Height = document.DocumentPaginator.PageSize.Height;
			page.VerticalAlignment = VerticalAlignment.Center;
			page.HorizontalAlignment = HorizontalAlignment.Center;

			var c = new Canvas();
			c.Background = new SolidColorBrush(Colors.Transparent);
			c.Width = page.Width;
			c.Height = page.Height;
			c.HorizontalAlignment = HorizontalAlignment.Left;
			c.VerticalAlignment = VerticalAlignment.Top;

			double x = 1;
			double y = 1;

			foreach (Fotografia foto in fotos)
		{
				//Devo cambiare Riga
				if ((double)(sizeLatoW * (x)) >= (double)c.Width )
		{
					x = 1;
					y++;
				}
				aggiungiImmagineAlCanvas(c, param, foto, x, y, sizeLatoW, sizeLatoH);
				x++;
			}
			page.Children.Add(c);

			// add the page to the document
			PageContent pageContent = new PageContent();
			((IAddChild)pageContent).AddChild(page);

			document.Pages.Add(pageContent);
		}

		private void aggiungiImmagineAlCanvas(Canvas c, ParamStampaProvini param, Fotografia foto, double x, double y, int sizeLatoW, int sizeLatoH)
		{
			// Ricavo l'immagine da stampare
			IImmagine fotina = AiutanteFoto.idrataImmagineGrande(foto);

			Image img = new Image();

			BitmapSource bmp1 = ((ImmagineWic)fotina).bitmapSource;

			img.Width = sizeLatoW - margin;
			img.Height = sizeLatoH - margin;

			img.HorizontalAlignment = HorizontalAlignment.Center;
			img.VerticalAlignment = VerticalAlignment.Center;
			img.BeginInit();
			img.Source = bmp1;
			img.EndInit();
			img.Stretch = Stretch.Uniform;
			img.StretchDirection = StretchDirection.Both;

			img.SetValue(Canvas.TopProperty, (Double)(sizeLatoH * (y - 1)) + testataH);
			img.SetValue(Canvas.LeftProperty, (Double)(sizeLatoW * (x - 1) + margin/2));

			c.Children.Add(img);

			eventualiStampigli(c, x, y, sizeLatoH, sizeLatoW, param, foto);
		}

		private void eventualiStampigli( Canvas c, double x, double y, int sizeLatoH, int sizeLatoW, ParamStampaProvini param,Fotografia foto) {

			SolidColorBrush coloreFg = new SolidColorBrush( Colors.LightGray );
			SolidColorBrush coloreBg = new SolidColorBrush( Colors.White );

			double riquadroH = 0;
			double riquadroW = 0;

			// Intestazione
			if (true)
			{
				TextBlock textIntestazione = new TextBlock();
				textIntestazione.Text = param.intestazione + "   " + Configurazione.infoFissa.descrizPuntoVendita;
				textIntestazione.FontSize = testataH * 65 / 100; 
				textIntestazione.Foreground = new SolidColorBrush(Colors.Black); ;
				textIntestazione.Background = new SolidColorBrush(Colors.Orange); ;
				textIntestazione.SetValue(Canvas.TopProperty, 1.0);
				textIntestazione.SetValue(Canvas.LeftProperty, 1.0);
				c.Children.Add(textIntestazione);
			}

			// Riquadro
			if (true)
			{
				Rectangle riquadro = new Rectangle();
				riquadro.Height = sizeLatoH - margin / 4;
				riquadro.Width = sizeLatoW - margin / 4;

				riquadroH = riquadro.Height;
				riquadroW = riquadro.Width;

				riquadro.StrokeThickness = 1;
				riquadro.Stroke = System.Windows.Media.Brushes.Black;

				riquadro.SetValue(Canvas.TopProperty, (Double)(sizeLatoH * (y - 1)) + testataH);
				riquadro.SetValue(Canvas.LeftProperty, (Double)(sizeLatoW * (x - 1)) + margin / 8);
				c.Children.Add(riquadro);
			}

			// Marchia Provini
			if(param.macchiaProvini)
			{
				TextBlock textMacchiaProvini = new TextBlock();
				textMacchiaProvini.Text = "DIGIPHOTO.IT";
				textMacchiaProvini.FontSize = sizeLatoH / 10; // 30pt text
				textMacchiaProvini.Foreground = coloreFg; ;
				textMacchiaProvini.Background = new SolidColorBrush(Colors.Transparent); ;
				textMacchiaProvini.SetValue(Canvas.TopProperty, (Double)(sizeLatoH * (y)) - 2 * textMacchiaProvini.FontSize + testataH - sizeLatoH / 2);
				textMacchiaProvini.SetValue(Canvas.LeftProperty, (Double)(sizeLatoW * (x - 1)) + riquadroW - MeasureString(textMacchiaProvini).Width);
				c.Children.Add(textMacchiaProvini);
			}

			// Numero della foto
			if( true )
			{
				TextBlock textNumero = new TextBlock();
				textNumero.Text = foto.etichetta;
				textNumero.FontSize = sizeLatoH / 10; // 30pt text
				textNumero.Foreground = new SolidColorBrush(Colors.Black); ;
				textNumero.Background = new SolidColorBrush(Colors.Orange); ;
				textNumero.SetValue(Canvas.TopProperty, (Double)(sizeLatoH * (y)) - 2 * textNumero.FontSize + testataH);
				textNumero.SetValue(Canvas.LeftProperty, (Double)(sizeLatoW * (x - 1)) + riquadroW - MeasureString(textNumero).Width);
				c.Children.Add(textNumero);
			}

			// Giornata
			if( true ) 
			{
				TextBlock textGiorno = new TextBlock();
				textGiorno.Text = foto.giornata.ToString("d");
				textGiorno.FontSize = sizeLatoH / 30; // 30pt text
				textGiorno.Foreground = coloreFg;
				textGiorno.Background = coloreBg;
				textGiorno.SetValue(Canvas.TopProperty, (Double)(sizeLatoH * (y - 1)) + testataH);
				textGiorno.SetValue(Canvas.LeftProperty, (Double)(sizeLatoW * (x - 1)) + riquadroW - MeasureString(textGiorno).Width);
				c.Children.Add(textGiorno);
			}

			// Operatore
			if( true ) 
			{
				TextBlock textOperatore = new TextBlock();
				StringBuilder operatore = new StringBuilder();
				textOperatore.Text = foto.fotografo.iniziali;
				textOperatore.FontSize = sizeLatoH / 30; // 30pt text
				textOperatore.Foreground = coloreFg;
				textOperatore.Background = coloreBg;
				textOperatore.SetValue(Canvas.TopProperty, (Double)(sizeLatoH * (y - 1)) + testataH);
				textOperatore.SetValue(Canvas.LeftProperty, (Double)(sizeLatoW * (x - 1)));
				c.Children.Add(textOperatore);
			}

		}

		private Size MeasureString(TextBlock textBlock)
		{
			var formattedText = new FormattedText(
				textBlock.Text,
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),textBlock.FontSize,Brushes.Black);
			return new Size(formattedText.Width, formattedText.Height);
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
