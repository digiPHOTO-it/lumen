using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Util;
using log4net;

namespace Digiphoto.Lumen.Imaging.Wic.Documents {

	public class TesseraDocPaginator : DocumentPaginator, IDisposable {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( ProviniDocPaginator ) );

		public TesseraDocPaginator( LavoroDiStampaTessera lsp, Size pageSize ) : base() {

			this.lavoroDiStampaTessera = lsp;
			this.pageSize = pageSize;

			numFotPerPag = lavoroDiStampaTessera.paramStampaTessera.numRighe * lavoroDiStampaTessera.paramStampaTessera.numColonne;

			// una pagina per ogni fotografia da stampare
			pageCount = 1;

			immaginiPaginaPrecedente = new List<IImmagine>();
		}

		int margSx = 0;
		int margDx = 0;
		int margTop = 0;
		int margBot = 0;

		private void inizializza() {

			larghezzaEffettiva = this.PageSize.Width - margSx - margDx;
            altezzaEffettiva = this.PageSize.Height - margTop - margBot;

			// Le dimensioni della foto-tessera sono indicate in millimetri. Le converto in pixel sapendo che abbiamo 96 pixel per inch (1 inch = 25,4 mm)
			sizeLatoW = Convert.ToInt32( this.lavoroDiStampaTessera.paramStampaTessera.mmWFoto / 25.4 * 96.0 );
			sizeLatoH = Convert.ToInt32( this.lavoroDiStampaTessera.paramStampaTessera.mmHFoto / 25.4 * 96.0 );
			
			inizializzato = true;
		}

		DocumentPage lastLoadedPage = null;
		private LavoroDiStampaTessera lavoroDiStampaTessera;
		private const int testataH = 0;
		private const int margin = 0;
		private int numFotPerPag;
		private int pageCount;
		private int sizeLatoH;
		private int sizeLatoW;
		private bool inizializzato = false;

		private double larghezzaEffettiva;      // Questa è la larghezza del canvas che contiente le foto.
		private double altezzaEffettiva;    // Questa è la larghezza del canvas che contiente le foto.
	

		public override DocumentPage GetPage( int pageNumber ) {

			if( !inizializzato )
				inizializza();

			rilasciaRisorsePaginaPrecendete( pageNumber );

			// Ricavo le foto da stampare
			var foto = ricavaFotoDellaPagina( pageNumber );

			// Creo il contenitore di tutte le foto
			Canvas canvas = new Canvas();
			canvas.Background = new SolidColorBrush( Colors.Transparent );

			canvas.Width = larghezzaEffettiva;
			canvas.Height = altezzaEffettiva;
			canvas.HorizontalAlignment = HorizontalAlignment.Left;
			canvas.VerticalAlignment = VerticalAlignment.Top;

			for( int riga = 1; riga <= lavoroDiStampaTessera.paramStampaTessera.numRighe; riga++ ) {

				for( int col = 1; col <= lavoroDiStampaTessera.paramStampaTessera.numColonne; col++ ) {

					aggiungiImmagineAlCanvas( canvas, foto, riga, col );

				}
			}

			Size sizeEffettiva = new Size( larghezzaEffettiva, altezzaEffettiva );
			Point origine = new Point( margSx, margTop );
            Rect rectEffettivo = new Rect( origine, sizeEffettiva );
			canvas.Arrange( rectEffettivo );
			canvas.UpdateLayout();

			DocumentPage documentPage = new DocumentPage( canvas, this.pageSize, Rect.Empty, rectEffettivo );
			return documentPage;
		}

		private Fotografia ricavaFotoDellaPagina( int paginaDaStampare ) {

			return lavoroDiStampaTessera.fotografia;
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
			// FormuleMagiche.rilasciaMemoria();
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

		private void aggiungiImmagineAlCanvas( Canvas canvas, Fotografia foto, int riga, int col ) {

			try {

				// Ricavo l'immagine da stampare
				IImmagine immagine;

				bool usoGrande = false;
				if( usoGrande ) {

					AiutanteFoto.idrataImmagineDaStampare( foto );
					immagine = AiutanteFoto.idrataImmagineGrande( foto );

					immaginiPaginaPrecedente.Add( immagine );

				} else {
					AiutanteFoto.idrataImmaginiFoto( foto, IdrataTarget.Provino );
					immagine = foto.imgProvino;
				}


				Image img = new Image();
				img.Stretch = Stretch.UniformToFill;
				img.StretchDirection = StretchDirection.Both;

				{
					// calcolo posizione left
					int tc = lavoroDiStampaTessera.paramStampaTessera.numColonne;
					double tuc = sizeLatoW * tc;
					double spaziow = (larghezzaEffettiva - tuc) / (tc + 1);
					double left = (spaziow * col) + (sizeLatoW * (col - 1));

					img.SetValue( Canvas.LeftProperty, left );
				}

				{
					// calcolo posizione top
					int tr = lavoroDiStampaTessera.paramStampaTessera.numRighe;
					double tur = sizeLatoH * tr;
					double spazioh = (altezzaEffettiva - tur) / (tr + 1);
					double top = (spazioh * riga) + (sizeLatoH * (riga - 1));

					img.SetValue( Canvas.TopProperty, top );
				}

				// Queste due dimensioni invece sono fisse
				img.Width = sizeLatoW;
				img.Height = sizeLatoH;

				img.HorizontalAlignment = HorizontalAlignment.Center;
				img.VerticalAlignment = VerticalAlignment.Center;
				if( immagine != null ) {
					img.RenderSize = new Size( img.Width, img.Height );  // TODO tento di tenere bassa la memoria
					img.BeginInit();
					BitmapSource bs = ((ImmagineWic)immagine).bitmapSource;
					img.Source = bs;
					img.EndInit();
				}

				canvas.Children.Add( img );

			} catch( Exception ee ) {
				// Non rilancio l'eccezione perché voglio continuare a stampare
				_giornale.Error( "Impossibile caricare immagime della foto: " + foto, ee );
			}

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
				return (lavoroDiStampaTessera.fotografia != null);
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
