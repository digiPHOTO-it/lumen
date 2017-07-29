using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.PresentationFramework;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.IO;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {

	public class ScrittaCorrettore : Correttore {

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			Scritta scritta = (Scritta)correzione;

			// Ricavo la bitmap sorgente
			BitmapSource bmpSorgente = ((ImmagineWic)immagineSorgente).bitmapSource;
			
			var ww = bmpSorgente.PixelWidth;
			var hh = bmpSorgente.PixelHeight;

			// -- creo un 
			Canvas c = new Canvas();

			Image image = new Image();
			image.Width = ww;
			image.Height = hh;
			image.BeginInit();
			image.Source = bmpSorgente;
			image.EndInit();

			c.Children.Add( image );

			Rect rectContenitore = new Rect( 0, 0, scritta.rifContenitoreW, scritta.rifContenitoreH );
			Rect rectScrittaOrig = new Rect( scritta.left, scritta.top, scritta.width, scritta.height );

			Size nuovaSizeFoto = new Size( ww, hh );

			Rect newRect = Geometrie.proporziona( rectScrittaOrig, rectContenitore, nuovaSizeFoto );

			TextPath textPath = new TextPath();
			textPath.Text = scritta.testo;
			textPath.FontFamily = new FontFamily( scritta.fontFamily );
			textPath.FontSize = scritta.fontSize;
			if( scritta.fillImage == null )
				textPath.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString( scritta.fillColor );
			textPath.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString( scritta.strokeColor );
			textPath.StrokeThickness = scritta.strokeThickness;

			Viewbox viewBox = new Viewbox();
			viewBox.Child = textPath;

			viewBox.Width = newRect.Width;
			viewBox.Height = newRect.Height;
			viewBox.SetValue( Canvas.LeftProperty, newRect.Left );
			viewBox.SetValue( Canvas.TopProperty, newRect.Top );


			c.Children.Add( viewBox );

			var size = new Size( ww, hh );
			c.Measure( size );
			c.Arrange( new Rect( size ) );

			Rect rectSotto = new Rect( 0, 0, bmpSorgente.PixelWidth, bmpSorgente.PixelHeight );
			ImageDrawing drawingSotto = new ImageDrawing( bmpSorgente, rectSotto );

			RenderTargetBitmap bmp = new RenderTargetBitmap( ww, hh, 96, 96, PixelFormats.Pbgra32 );

						
			bmp.Render( c );

			BmpBitmapEncoder enc = new BmpBitmapEncoder();
			enc.Frames.Add( BitmapFrame.Create( bmp ) );
			byte[] imagebit; //You can save your copy data in byte[].
			using( MemoryStream stream = new MemoryStream() ) {
				enc.Save( stream );
				imagebit = stream.ToArray();
				stream.Close();
			}


			var bitmap = new BitmapImage();
			using( var stream = new MemoryStream( imagebit ) ) {
				bitmap.BeginInit();
				bitmap.StreamSource = stream;
				bitmap.CacheOption = BitmapCacheOption.OnLoad;
				bitmap.EndInit();
				bitmap.Freeze();
			}

			return new ImmagineWic( bitmap );

#if false





			var size = new Size( bmpSorgente.PixelWidth, bmpSorgente.PixelHeight );
			c.Measure( size );
			c.Arrange( new Rect( size ) );
			RenderTargetBitmap bmp = new RenderTargetBitmap( 250, 100, 96, 96, PixelFormats.Pbgra32 );
			bmp.Render( c );



			// La immagine mi servirà da sfondo
			Rect rectSotto = new Rect( 0, 0, bmpSorgente.PixelWidth, bmpSorgente.PixelHeight );
			ImageDrawing drawingSotto = new ImageDrawing( bmpSorgente, rectSotto );

			TextPath textPath = new TextPath();
			textPath.Text = "Prova 1 ciao mare";
			textPath.FontFamily = new FontFamily( "Verdana" );
			textPath.FontSize = 40;
			textPath.Fill = Brushes.Red;
			textPath.Stroke = Brushes.Blue;
			textPath.StrokeThickness = 3;
			textPath.Width = 800;
			textPath.Height = 100;

			var formattedText = new FormattedText( "POLLLLLLLLLOOOOOO", Thread.CurrentThread.CurrentUICulture,
		FlowDirection.LeftToRight,
			new Typeface(
				new FontFamily( "Verdana" ),
				FontStyles.Normal,
				FontWeights.Normal, 
				FontStretches.Normal ), 20, Brushes.Yellow );

			int w = (int)rectSotto.Width;
			int h = (int)rectSotto.Height;
			// Renderizzo l'immagine finita, in una bitmap in modo da poterla ritornare.
			RenderTargetBitmap rtb = new RenderTargetBitmap( w, h, 96d, 96d, PixelFormats.Default );

			System.Windows.Media.Pen pen = new System.Windows.Media.Pen( System.Windows.Media.Brushes.Pink, 1 );

			DrawingVisual dv = new DrawingVisual();
			using( DrawingContext ctx = dv.RenderOpen() ) {

				// Prima renderizzo l'immagine
				ctx.DrawDrawing( drawingSotto );

				// Poi disegno sopra il testo
				ctx.DrawGeometry( Brushes.Green, pen, textPath.RenderedGeometry );

				ctx.DrawText( formattedText, new Point( 10, 20 ) );
			}
			rtb.Render( dv );

			return new ImmagineWic( rtb );
#endif
		}

		/// <summary>
		/// Mi dice se il logo in questione, va posizionato in modo automatico (nei 4 angoli)
		/// oppure se è posizionato in modo manuale, cioè l'utente lo ha spostato a mano.
		/// </summary>
		/// <param name="logo"></param>
		/// <returns></returns>
		public static bool isLogoPosizionatoManualmente( Logo logo ) {
			return logo.traslazione != null;
		}

		public static Rect calcolaCoordinateLogo( int wi, int hi, int wl, int hl, Logo logoCorrezione ) {

			Rect posiz = Rect.Empty;

			// La traslazione devo gestirla in modo indiretto.
			// Infatti questa ha 
			// Chiamando il suo correttore non funziona. Ed è anche ovvio. Come faccio a traslare una immagine su se stessa ? Ci vuole un contenitore di riferimento.
			if( isLogoPosizionatoManualmente( logoCorrezione ) ) {
				// Posizionamento manuale. Devo riproporzionare le coordinate alla immagine di destinazione
				// TODO occorre valorizzare opportunamente "posiz".
			} else {
				// Posizionamento automatico. Calcolo io la posizione in base all'angolo specificato
				posiz = calcolaCoordinateLogoAutomatiche( wi, hi, wl, hl, logoCorrezione );
			}

			return posiz;
		}


		public static Rect calcolaCoordinateLogoAutomatiche( int wi, int hi, int wl, int hl, Logo logoCorrezione ) {

			// calcolo un margine del 5% da lasciare a sinistra/destra - alto/basso
			int percMargine, pixMargineW, pixMargineH;
			float f_rappoLogo;
			percMargine = 5;  // 5 percento di spazio tra il logo ed il bordo

			double retL;
			double retT;
			double retW;
			double retH;

			// Trasformo la percentuale in pixel
			pixMargineW = wi / 100 * percMargine;
			pixMargineH = hi / 100 * percMargine;

			f_rappoLogo = (float)wl / (float)hl;

			// --- riproporziono il logo all'immagine.
			if( f_rappoLogo >= 1 ) {
				retW = Math.Max( wi, hi ) * ((short)logoCorrezione.pcCopri) / 100;
				retH = retW / f_rappoLogo;
			} else {
				retH = Math.Max( wi, hi ) * ((short)logoCorrezione.pcCopri) / 100;
				retW = retH * f_rappoLogo;
			}

			if( logoCorrezione.posiz == Logo.PosizLogo.NordOvest || logoCorrezione.posiz == Logo.PosizLogo.SudOvest ) {
				// Ovest (sinistra)
				retL = pixMargineW;
			} else {
				// Est (destra)
				retL = wi - retW - pixMargineW;
			}

			if( logoCorrezione.posiz == Logo.PosizLogo.NordOvest || logoCorrezione.posiz == Logo.PosizLogo.NordEst ) {
				// Nord (alto)
				retT = pixMargineH;
			} else {
				// Sud (basso)
				retT = hi - retH - pixMargineH;
			}

			return new Rect( retL, retT, retW, retH );
		}



		public static Scritta creaScrittaDefault() {
			Scritta scritta = new Scritta();

			scritta.fontSize = 60;
			scritta.testo = Configurazione.infoFissa.descrizPuntoVendita != null ? Configurazione.infoFissa.descrizPuntoVendita : "Testo di esempio";

			scritta.fontFamily = "Verdana";

			scritta.strokeThickness = 1;
			scritta.strokeColor = "#FF0000"; // red 

			scritta.fillColor = "#0000FF";  // blue

			return scritta;
		}
	}
}
