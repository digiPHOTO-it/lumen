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

			// -- creo un contenitore che mi permette di avere il controllo completo del posizionamento 
			Panel contenitore = new Grid();
			contenitore.Width = ww;
			contenitore.Height = hh;
//			contenitore.VerticalAlignment = VerticalAlignment.Center;
//			contenitore.HorizontalAlignment = HorizontalAlignment.Center;

			Image image = new Image();
			image.BeginInit();
			image.Source = bmpSorgente;
			image.EndInit();

			contenitore.Children.Add( image );


			/*
						Rect rectContenitore = new Rect( 0, 0, scritta.rifContenitoreW, scritta.rifContenitoreH );
						Rect rectScrittaOrig = new Rect( scritta.left, scritta.top, scritta.width, scritta.height );

						Size nuovaSizeFoto = new Size( ww, hh );

						Rect newRect = Geometrie.proporziona( rectScrittaOrig, rectContenitore, nuovaSizeFoto );
			*/
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
			Canvas.SetZIndex( viewBox, 10 );


			contenitore.Children.Add( viewBox );


			var size = new Size( ww, hh );
			contenitore.Measure( size );
			contenitore.Arrange( new Rect( size ) );


			TransformGroup gruppo = new TransformGroup();


			// ROTAZIONE
			if( scritta.rotazione != null ) {
				RotateTransform rot = new RotateTransform( scritta.rotazione.gradi );

				// La rotazione avviene sempre nel centro dell'elemento
				var l = Double.IsNaN( Canvas.GetLeft( viewBox ) ) ? 0 : Canvas.GetLeft( viewBox );
				var t = Double.IsNaN( Canvas.GetTop( viewBox ) ) ? 0 : Canvas.GetTop( viewBox );
				rot.CenterX = l + (viewBox.ActualWidth / 2);
				rot.CenterY = t + (viewBox.ActualHeight / 2);
				//				TransformGroup tg = new TransformGroup();
				//				tg.Children.Add( rot );
				gruppo.Children.Add( rot );
			}

			// TRASLAZIONE (move)
			if( scritta.traslazione != null ) {

				// devo riproporzionare le coordinate di origine a quelle attuali.

				// spostaX : oldW = newSpostaH : newW
				// 
				var newOffsetX = contenitore.ActualWidth * scritta.traslazione.offsetX / scritta.traslazione.rifW;
				var newOffsetY = contenitore.ActualHeight * scritta.traslazione.offsetY / scritta.traslazione.rifH;

				TranslateTransform tre = new TranslateTransform( newOffsetX, newOffsetY );
				gruppo.Children.Add( tre );
			}


			if( scritta.zoom != null ) {
				ScaleTransform stx = new ScaleTransform( scritta.zoom.fattore, scritta.zoom.fattore );
				gruppo.Children.Add( stx );

				stx.CenterX = (viewBox.RenderSize.Width / 2);
				stx.CenterY = (viewBox.RenderSize.Height / 2);
			}

			if( gruppo.Children.Count > 0 )
				viewBox.RenderTransform = gruppo;




			contenitore.InvalidateMeasure();
			contenitore.InvalidateArrange();

			contenitore.Measure( size );
			contenitore.Arrange( new Rect( size ) );



			/*
						Rect rectSotto = new Rect( 0, 0, bmpSorgente.PixelWidth, bmpSorgente.PixelHeight );
						ImageDrawing drawingSotto = new ImageDrawing( bmpSorgente, rectSotto );
			*/
			RenderTargetBitmap bmp = new RenderTargetBitmap( ww, hh, 96, 96, PixelFormats.Pbgra32 );



			bmp.Render( contenitore );

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

		}


		public static Scritta creaScrittaDefault() {
			Scritta scritta = new Scritta();

			scritta.testo = Configurazione.infoFissa.descrizPuntoVendita != null ? Configurazione.infoFissa.descrizPuntoVendita : "Testo di esempio";

			scritta.fontSize = 60;
			scritta.fillColor = "#0000FF";  // blue
			scritta.fontFamily = "Verdana";

			scritta.strokeThickness = 1;
			scritta.strokeColor = "#FF0000"; // red 
			
			return scritta;
		}
	}
}
