using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Util;
using log4net;

namespace Digiphoto.Lumen.Imaging.Wic.Documents {

	public class ProviniDocPaginator : DocumentPaginator, IDisposable {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( ProviniDocPaginator ) );

		public ProviniDocPaginator( LavoroDiStampaProvini lsp, Size pageSize ) : base() {

			this.lavoroDiStampaProvini = lsp;
			this.pageSize = pageSize;

			numFotPerPag = lavoroDiStampaProvini.param.numeroRighe * lavoroDiStampaProvini.param.numeroColonne;

			pageCount = calcolaNumeroPagine();

			lavoroDiStampaProvini.param.numPag = (short)pageCount;

			immaginiPaginaPrecedente = new List<IImmagine>();
		}

		int calcolaNumeroPagine() {

			int totPagine = 0;

			if( lavoroDiStampaProvini.param.rompePerGiorno ) {

				// Per sapere quante pagine saranno, devo contare una ad una le foto
				DateTime ggPrec = DateTime.MinValue;
				int contaFotoStessoGiorno = 0;

				foreach( var foto in lavoroDiStampaProvini.fotografie ) {

					bool saltoPag = false;

					// Se ho almeno una foto, allora ho almeno una pagina
					if( totPagine == 0 )
						totPagine = 1;

					// Se cambia giorno, allora salto pagina...
					if( foto.giornata != ggPrec && ggPrec != DateTime.MinValue )
						saltoPag = true;

					// Se nello stesso giorno supero il num di foto x giorno...
					if( ++contaFotoStessoGiorno > numFotPerPag )
						saltoPag = true;

					if( saltoPag ) {
						++totPagine;
						contaFotoStessoGiorno = 1;
					}

					ggPrec = foto.giornata;
				}

			} else {
				totPagine = numFotPerPag > 0 ? (int)Math.Ceiling( (decimal)lavoroDiStampaProvini.fotografie.Count / numFotPerPag ) : 1;
			}


			return totPagine;
		}


		private void inizializza() {

			larghezzaEffettiva = this.PageSize.Width - lavoroDiStampaProvini.param.margini.left - lavoroDiStampaProvini.param.margini.right;
            altezzaEffettiva = this.PageSize.Height - lavoroDiStampaProvini.param.margini.top - lavoroDiStampaProvini.param.margini.bottom;

			if( lavoroDiStampaProvini.param.numeroRighe > 0 && lavoroDiStampaProvini.param.numeroColonne > 0 ) {
				sizeLatoH = (int)((altezzaEffettiva - testataH) / lavoroDiStampaProvini.param.numeroRighe);

				sizeLatoW = (int)(larghezzaEffettiva / lavoroDiStampaProvini.param.numeroColonne);

				numFotPerPag = lavoroDiStampaProvini.param.numeroRighe * lavoroDiStampaProvini.param.numeroColonne;
			} else {
				double volume = (larghezzaEffettiva) * (altezzaEffettiva - testataH);
				double volumeProvino = volume / lavoroDiStampaProvini.fotografie.Count;
				double c = larghezzaEffettiva / (altezzaEffettiva - testataH);

				// Utilizzo le foto con il lato lungo
				sizeLatoW = (int)Math.Sqrt( volumeProvino / c );
				sizeLatoH = (int)(sizeLatoW * c);
				double volumeProvino2 = sizeLatoH * sizeLatoW;
				double errore = volumeProvino - volumeProvino2;

				int numFotH = (int)Math.Ceiling( (altezzaEffettiva - testataH) / sizeLatoH );

				int numFotW = (int)Math.Ceiling( larghezzaEffettiva / sizeLatoW );

				sizeLatoH = (int)((altezzaEffettiva - testataH) / numFotH);

				sizeLatoW = (int)(larghezzaEffettiva / numFotW);
			}

			// --

			// Determino se usare le foto grandi o piccole
			if( Configurazione.UserConfigLumen.tecSogliaStampaProvini == 0 )
				Configurazione.UserConfigLumen.tecSogliaStampaProvini = -3;

			if( Configurazione.UserConfigLumen.tecSogliaStampaProvini == -1 )
				usoGrande = true;
			else if( Configurazione.UserConfigLumen.tecSogliaStampaProvini == -2 )
				usoGrande = false;
			else if( Configurazione.UserConfigLumen.tecSogliaStampaProvini == -3 )
				usoGrande = true;
			else if( Configurazione.UserConfigLumen.tecSogliaStampaProvini > 0 )
				usoGrande = (numFotPerPag <= Configurazione.UserConfigLumen.tecSogliaStampaProvini);

			inizializzato = true;
		}

		DocumentPage lastLoadedPage = null;
		private LavoroDiStampaProvini lavoroDiStampaProvini;
		private const int testataH = 30;
		private const int margin = 8;
		private int numFotPerPag;
		private int pageCount;
		private int sizeLatoH = Configurazione.infoFissa.pixelProvino;
		private int sizeLatoW = Configurazione.infoFissa.pixelProvino;
		private bool inizializzato = false;
		private bool usoGrande;
		private double larghezzaEffettiva;      // Questa è la larghezza del canvas che contiente le foto.
		private double altezzaEffettiva;    // Questa è la larghezza del canvas che contiente le foto.
	

		public override DocumentPage GetPage( int pageNumber ) {

			if( !inizializzato )
				inizializza();

			rilasciaRisorsePaginaPrecendete( pageNumber );

			// Ricavo le foto da stampare
			var fotos = ricavaFotoDellaPagina( pageNumber );

			// Creo il contenitore di tutte le foto
			Canvas canvas = new Canvas();
			canvas.Background = new SolidColorBrush( Colors.Transparent );

			canvas.Width = larghezzaEffettiva;
			canvas.Height = altezzaEffettiva;
			canvas.HorizontalAlignment = HorizontalAlignment.Left;
			canvas.VerticalAlignment = VerticalAlignment.Top;

			double x = 1;
			double y = 1;

			foreach( Fotografia foto in fotos ) {
				//Devo cambiare Riga
				if( (double)(sizeLatoW * (x)) >= (double)canvas.Width ) {
					x = 1;
					y++;
				}
				aggiungiImmagineAlCanvas( canvas, foto, x, y );
				x++;
			}

			Size sizeEffettiva = new Size( larghezzaEffettiva, altezzaEffettiva );
			Point origine = new Point( lavoroDiStampaProvini.param.margini.left, lavoroDiStampaProvini.param.margini.top );
            Rect rectEffettivo = new Rect( origine, sizeEffettiva );
			canvas.Arrange( rectEffettivo );
			canvas.UpdateLayout();

			DocumentPage documentPage = new DocumentPage( canvas, this.pageSize, Rect.Empty, rectEffettivo );
			return documentPage;
		}

		private IEnumerable<Fotografia> ricavaFotoDellaPagina( int paginaDaStampare ) {

			IEnumerable<Fotografia> fotos = null;

			if( lavoroDiStampaProvini.param.rompePerGiorno ) {

				// Per sapere quante pagine saranno, devo contare una ad una le foto
				DateTime ggPrec = DateTime.MinValue;
				List<Fotografia> lista = new List<Fotografia>();
				int contaFotoStessoGiorno = 0;
				int pagCorrente = 0; // Le conta in base 0.

				foreach( var foto in lavoroDiStampaProvini.fotografie ) {

					bool saltoPag = false;

					// Se cambia giorno, allora salto pagina...
					if( foto.giornata != ggPrec && ggPrec != DateTime.MinValue )
						saltoPag = true;

					// Se nello stesso giorno supero il num di foto x giorno...
					if( ++contaFotoStessoGiorno > numFotPerPag )
						saltoPag = true;

					if( saltoPag ) {
						++pagCorrente;
						contaFotoStessoGiorno = 1;
					}

					ggPrec = foto.giornata;

					if( pagCorrente == paginaDaStampare )
						lista.Add( foto );

					// Se ho riempito la pagina, esco
					if( lista.Count == numFotPerPag )
						break;
				}

				fotos = lista.AsEnumerable();

			} else {

				// Le pagine sono tutte uguali
				int toSkip = (numFotPerPag * (paginaDaStampare));  // Numero di foto da skippare nella paginazione

				// Prendo solo le foto interessate da questa pagina
				fotos = lavoroDiStampaProvini.fotografie.Skip( toSkip ).Take( numFotPerPag );
			}

			return fotos;
		}

		private void rilasciaRisorsePaginaPrecendete( int pageNumber ) {

			// Sulla prima pagina non ho ancora nulla da fare
			if( pageNumber == 0 )
				return;

			// Libero un pò di memoria, ammesso di fare ancora in tempo.
			foreach( IImmagine immagine in immaginiPaginaPrecedente )
				immagine.Dispose();
			immaginiPaginaPrecedente.Clear();

			if( lastLoadedPage != null ) {
				lastLoadedPage.Dispose();
				lastLoadedPage = null;
			}

			// Qualche cabala + incantesimi
			FormuleMagiche.rilasciaMemoria();
			// Dopo il passaggio a .net 4.6 e sqlite aggiornato, questo non funziona più e si pianta
			//			FormuleMagiche.attendiGcFinalizers();
			//			FormuleMagiche.rilasciaMemoria();
		}

		public override int PageCount {
			get {
				return pageCount;
			}
		}

		private List<IImmagine> immaginiPaginaPrecedente {
			get;
			set;
		}

		private void aggiungiImmagineAlCanvas( Canvas canvas, Fotografia foto, double x, double y ) {

			try {

				// Ricavo l'immagine da stampare
				IImmagine provino;

				if( usoGrande ) {

					AiutanteFoto.idrataImmagineDaStampare( foto );
					provino = AiutanteFoto.idrataImmagineGrande( foto );

					if( Configurazione.UserConfigLumen.tecSogliaStampaProvini == -3 ) {

						IGestoreImmagineSrv g = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
						IImmagine provino2 = g.creaProvino( provino, 1000 );
						provino = (IImmagine)provino2.Clone();
						((ImmagineWic)provino).bitmapSource.Freeze();

						AiutanteFoto.disposeImmagini( foto, IdrataTarget.Originale );
						AiutanteFoto.disposeImmagini( foto, IdrataTarget.Risultante );
					}

					immaginiPaginaPrecedente.Add( provino );

				} else {
					AiutanteFoto.idrataImmaginiFoto( foto, IdrataTarget.Provino );
					provino = foto.imgProvino;
				}



				Image img = new Image();
				img.Stretch = Stretch.Uniform;
				img.StretchDirection = StretchDirection.Both;

				img.SetValue( Canvas.TopProperty, (Double)(sizeLatoH * (y - 1)) + testataH );
				img.SetValue( Canvas.LeftProperty, (Double)(sizeLatoW * (x - 1) + margin / 2) );

				img.Width = sizeLatoW - margin;
				img.Height = sizeLatoH - margin;

				img.HorizontalAlignment = HorizontalAlignment.Center;
				img.VerticalAlignment = VerticalAlignment.Center;
				if( provino != null ) {
					img.RenderSize = new Size( img.Width, img.Height );  // TODO tento di tenere bassa la memoria
					img.BeginInit();
					BitmapSource bs = ((ImmagineWic)provino).bitmapSource;
					img.Source = bs;
					img.EndInit();
				}

				canvas.Children.Add( img );

			} catch( Exception ee ) {
				// Non rilancio l'eccezione perché voglio continuare a stampare
				_giornale.Error( "Impossibile caricare immagime della foto: " + foto, ee );
			}

			eventualiStampigli( canvas, x, y, foto );
		}

		// Macchia provini
		private SolidColorBrush coloreWaterMarkFg = new SolidColorBrush( Colors.LightGray ) {
			Opacity = 0.7
		};
		private SolidColorBrush coloreWaterMarkBg = new SolidColorBrush( Colors.White );
		// Stampigli 
		private SolidColorBrush coloreStampigliBg = new SolidColorBrush( Colors.White );
		private SolidColorBrush coloreStampigliFg = new SolidColorBrush( Colors.Black );
		// Intestazione striscia
		private SolidColorBrush coloreHeaderBg = new SolidColorBrush( Colors.Cyan );
		private SolidColorBrush coloreHeaderFg = new SolidColorBrush( Colors.Black );
		// Numero della foto
		private SolidColorBrush coloreNumFotoBg = new SolidColorBrush( Colors.Orange );
		private SolidColorBrush coloreNumFotoFg = new SolidColorBrush( Colors.Black );

		private void eventualiStampigli( Canvas canvas, double x, double y, Fotografia foto ) {

			double riquadroH = 0;
			double riquadroW = 0;
			double riquadroL = 0;
			double riquadroT = 0;

			// Intestazione
			if( true ) {
				TextBlock textIntestazione = new TextBlock();

				// Intestazione con eventuale data (se richiesto)
				StringBuilder titolo = new StringBuilder();
				if( lavoroDiStampaProvini.param.rompePerGiorno )
					titolo.Append( foto.giornata.Date.ToString( "D" ) );

				if( titolo.Length > 0 && string.IsNullOrEmpty( this.lavoroDiStampaProvini.param.intestazione ) == false )
					titolo.Append( " - " );
				titolo.Append( this.lavoroDiStampaProvini.param.intestazione );

				if( titolo.Length > 0 && string.IsNullOrEmpty( Configurazione.infoFissa.descrizPuntoVendita ) == false )
					titolo.Append( " - " );
				titolo.Append( Configurazione.infoFissa.descrizPuntoVendita );

				textIntestazione.Text = titolo.ToString();


				textIntestazione.FontSize = Math.Max( 9, testataH * 65 / 100 );  // Almeno 9 pixel altimenti non si legge
				textIntestazione.Foreground = coloreHeaderFg;
				textIntestazione.Background = coloreHeaderBg;
				textIntestazione.SetValue( Canvas.TopProperty, 1.0 );
				textIntestazione.SetValue( Canvas.LeftProperty, 1.0 );
				canvas.Children.Add( textIntestazione );
			}

			// Riquadro
			if( true ) {
				Rectangle riquadro = new Rectangle();
				riquadro.StrokeThickness = 1;
				riquadro.Stroke = System.Windows.Media.Brushes.Black;

				// Calcolo le coordinate del riquadro del singolo fotogramma
				riquadroH = sizeLatoH - margin / 4;
				riquadroW = sizeLatoW - margin / 4;
				riquadroL = (Double)(sizeLatoW * (x - 1)) + margin / 8;
				riquadroT = (Double)(sizeLatoH * (y - 1)) + testataH;

				riquadro.Height = riquadroH;
				riquadro.Width = riquadroW;
				riquadro.SetValue( Canvas.TopProperty, riquadroT );
				riquadro.SetValue( Canvas.LeftProperty, riquadroL );

				canvas.Children.Add( riquadro );
			}

			// Marchia Provini
			if( this.lavoroDiStampaProvini.param.macchiaProvini ) {
				TextBlock textMacchiaProvini = new TextBlock();
				textMacchiaProvini.Width = riquadroW - 2;

				textMacchiaProvini.Text = "digiPHOTO.it";
				textMacchiaProvini.FontSize = Math.Max( 9, sizeLatoH / 8 );
				textMacchiaProvini.Foreground = coloreWaterMarkFg;
				;
				textMacchiaProvini.Background = new SolidColorBrush( Colors.Transparent );
				;
				//textMacchiaProvini.SetValue( Canvas.TopProperty, (Double)((sizeLatoH * (y)) - (2 * textMacchiaProvini.FontSize + testataH) - (sizeLatoH / 2) ));
				//textMacchiaProvini.SetValue(Canvas.LeftProperty, riquadroL + 1);
				//Centro il water-mark su entrambi gli assi!!

				textMacchiaProvini.SetValue( Canvas.TopProperty, (Double)(sizeLatoH * (y)) - ((riquadroH / 2) + (textMacchiaProvini.FontSize / 2) - testataH) );
				textMacchiaProvini.SetValue( Canvas.LeftProperty, (Double)(riquadroL + (sizeLatoW / 2 - MeasureString( textMacchiaProvini ).Width / 2) + 1) );
				canvas.Children.Add( textMacchiaProvini );
			}

			// Giornata
			if( true ) {
				TextBlock textGiorno = new TextBlock();
				textGiorno.Text = foto.giornata.ToString( "d" );
				textGiorno.FontSize = Math.Max( 9, sizeLatoH / 30 ); // 30pt text
				textGiorno.Foreground = coloreStampigliFg;
				textGiorno.Background = coloreStampigliBg;
				textGiorno.SetValue( Canvas.TopProperty, (Double)(sizeLatoH * (y)) - 2 * textGiorno.FontSize + testataH );
				textGiorno.SetValue( Canvas.LeftProperty, (Double)(sizeLatoW * (x - 1)) + riquadroW - MeasureString( textGiorno ).Width - 1 );
				canvas.Children.Add( textGiorno );
			}

			// Numero della foto
			if( true ) {
				TextBlock textNumero = new TextBlock();
				textNumero.Text = " " + foto.etichetta + " ";
				textNumero.FontSize = Math.Max( 9, sizeLatoH / 10 );
				textNumero.Foreground = coloreNumFotoFg;
				textNumero.Background = coloreNumFotoBg;
				textNumero.SetValue( Canvas.TopProperty, riquadroT + 2 );
				var margSx = riquadroW * 2 / 100;
				textNumero.SetValue( Canvas.LeftProperty, (Double)(sizeLatoW * (x - 1)) + riquadroW - MeasureString( textNumero ).Width - 3 - margSx );
				canvas.Children.Add( textNumero );
			}

			// Operatore
#if FALSO
			if( false ) {
				TextBlock textOperatore = new TextBlock();
				StringBuilder operatore = new StringBuilder();
				textOperatore.Text = foto.fotografo.iniziali;
				textOperatore.FontSize = Math.Max( 9, sizeLatoH / 30 );
				textOperatore.Foreground = coloreStampigliFg;
				textOperatore.Background = coloreStampigliBg;
				textOperatore.SetValue( Canvas.TopProperty, riquadroT + 2 );
				textOperatore.SetValue( Canvas.LeftProperty, riquadroL + 2 );
				canvas.Children.Add( textOperatore );
			}
#endif
		}


		private Size MeasureString( TextBlock textBlock ) {
			var formattedText = new FormattedText(
				textBlock.Text,
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface( textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch ), textBlock.FontSize, Brushes.Black );
			return new Size( formattedText.Width, formattedText.Height );
		}

		public void Dispose() {
			// Rilascio le risorse dell'ultima pagina stampata
			rilasciaRisorsePaginaPrecendete( pageCount );
		}


		public override bool IsPageCountValid {
			get {
				return (lavoroDiStampaProvini.fotografie != null);
			}
		}

		private Size pageSize;

		public override Size PageSize {
			get {
				return pageSize;
			}
			set {
				this.pageSize = value;
			}
		}

		public override IDocumentPaginatorSource Source {
			get {
				return null;
			}
		}
	}
}
