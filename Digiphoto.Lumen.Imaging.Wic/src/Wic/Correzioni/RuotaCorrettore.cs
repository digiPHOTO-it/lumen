using Digiphoto.Lumen.Imaging.Correzioni;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System;
using System.Windows;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {

	/// <summary>
	/// 
	/// </summary>
	public class RuotaCorrettore : Correttore {


		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			BitmapSource bmpsource = ((ImmagineWic)immagineSorgente).bitmapSource;
			Ruota ruotaCorrezione = (Ruota)correzione;

			BitmapSource newBmp;
			if( ruotaCorrezione.isAngoloRetto )
				newBmp = rotazioneSemplice( bmpsource, ruotaCorrezione.gradi );
			else
				newBmp = rotazioneSulPosto( bmpsource, ruotaCorrezione.gradi );
//				newBmp = rotazioneComplessa( bmpsource, ruotaCorrezione.gradi );

			return new ImmagineWic( newBmp );
		}

		/// <summary>
		/// Questa rotazione produce una immagine grande quando richiesto dalle dimensioni indicate
		/// In pratica esegue una rotazione con fulcro centrale, e senza allargare il canvas.
		/// </summary>
		/// <param name="bmpSorgente"></param>
		/// <param name="gradi"></param>
		/// <returns></returns>
		private BitmapSource rotazioneSulPosto( BitmapSource bmpSorgente, double gradi ) {
			return rotazioneSulPosto( bmpSorgente, gradi, bmpSorgente.Width, bmpSorgente.Height );
		}

		private BitmapSource rotazioneSulPosto( BitmapSource bmpSorgente, double gradi , double newW, double newH ) {


			RotateTransform rtx = new RotateTransform( gradi );
			rtx.CenterX = newW / 2;
			rtx.CenterY = newH / 2;

/*
			Canvas c = new Canvas();
			c.Background = new SolidColorBrush( Colors.Orange );
			c.Width = newW;
			c.Height = newH;
			c.HorizontalAlignment = HorizontalAlignment.Left;
			c.VerticalAlignment = VerticalAlignment.Top;

			WriteableBitmap wb = new WriteableBitmap( bmpSorgente );

			Image fotona = new Image();
			fotona.HorizontalAlignment = HorizontalAlignment.Left;
			fotona.VerticalAlignment = VerticalAlignment.Top;
			fotona.BeginInit();
			fotona.Source = wb;
			fotona.EndInit();


			c.Children.Add( fotona );

			// Imposto la posizione della foto all'interno del canvas della cornice.
//			Canvas.SetLeft( fotona, newRect.Left * factorX );
//			Canvas.SetTop( fotona, newRect.Top * factorY );
			Canvas.SetLeft( fotona, 20 );
			Canvas.SetTop( fotona, 20 );
*/


			// Target Rect for the resize operation
			Rect rect = new Rect( 0, 0, newW, newH );
			// Rect newRect = Geometrie.proporziona( rectFotina, rectFondo, sizeMaschera );


			// Create a DrawingVisual/Context to render with
			DrawingVisual drawingVisual = new DrawingVisual();
			
			using( DrawingContext drawingContext = drawingVisual.RenderOpen() ) {

				drawingContext.DrawRectangle( new SolidColorBrush( Colors.White ),  new Pen( Brushes.White, 0 ), rect );

				drawingVisual.Transform = rtx;
				drawingContext.DrawImage( bmpSorgente, rect );
			}


/*
			DrawingVisual s = new DrawingVisual();
			s.Children.Add
			RectangleGeometry rg = new RectangleGeometry(new Rect(0, 0, newW, newH));
			GeometryDrawing rgd = new GeometryDrawing(Brushes.Yellow, null, rg);
			DrawingGroup dg = new DrawingGroup();
			dg.Children.Add(rgd);
			using( DrawingContext dc = s.RenderOpen() ) {
			   dc.DrawDrawing(dg);
			}

			rgd.Brush = Brushes.Magenta;
			rg.Transform = new RotateTransform(42);
			dg.Children.Remove( rgd );
*/


			
			// Use RenderTargetBitmap to resize the original image with Default DPI values as 96
			RenderTargetBitmap resizedImage = new RenderTargetBitmap( (int)rect.Width, (int)rect.Height, 96, 96,PixelFormats.Default);
			resizedImage.Render( drawingVisual );
			// Return the resized image
			return resizedImage;
		}


		/// <summary>
		/// Questa rotazione funziona se l'angolo è retto (multiplo di 90°)
		/// </summary>
		/// <param name="bmpSorgente"></param>
		/// <param name="gradi">deve essere multiplo di 90</param>
		/// <returns></returns>
		private BitmapSource rotazioneSemplice( BitmapSource bmpSorgente, double gradi ) {

			// Create the TransformedBitmap to use as the Image source.
			RotateTransform transform = new RotateTransform( gradi );

			TransformedBitmap tb = new TransformedBitmap( bmpSorgente, transform );

			return tb;
		}


		private BitmapSource rotazioneComplessa( BitmapSource sourceImage, double angle ) {

			// The original bitmap needs to be drawn onto a new bitmap which will probably be bigger 
			// because the corners of the original will move outside the original rectangle.
			// An easy way (OK slightly 'brute force') is to calculate the new bounding box is to calculate the positions of the 
			// corners after rotation and get the difference between the maximum and minimum x and y coordinates.
			double wOver2 = sourceImage.PixelWidth / 2;
			double hOver2 = sourceImage.PixelHeight / 2;

			float radians = -(float)(angle / 180.0 * Math.PI);


			// Get the coordinates of the corners, taking the origin to be the centre of the bitmap.

			Point [] corners = new Point []{
				new Point(-wOver2, -hOver2),
				new Point(+wOver2, -hOver2),
				new Point(+wOver2, +hOver2),
				new Point(-wOver2, +hOver2)
			};

			for( int i = 0; i < 4; i++ ) {
				Point p = corners [i];
				Point newP = new Point( (p.X * Math.Cos( radians ) - p.Y * Math.Sin( radians )), (p.X * Math.Sin( radians ) + p.Y * Math.Cos( radians )) );
				corners [i] = newP;
			}

			// Find the min and max x and y coordinates.
			double minX = corners [0].X;
			double maxX = minX;
			double minY = corners [0].Y;
			double maxY = minY;
			for( int i = 1; i < 4; i++ ) {
				Point p = corners [i];
				minX = Math.Min( minX, p.X );
				maxX = Math.Max( maxX, p.X );
				minY = Math.Min( minY, p.Y );
				maxY = Math.Max( maxY, p.Y );
			}

			// Get the size of the new bitmap.
			Size newSize = new Size( maxX - minX, maxY - minY );

			double x = -(newSize.Width - sourceImage.PixelWidth) / 2;
			double y = -(newSize.Height - sourceImage.PixelHeight) / 2;

			return rotazioneComplessa( sourceImage, angle, x, y, (int)newSize.Width, (int)newSize.Height );
		}

		private BitmapSource rotazioneComplessa( BitmapSource sourceImage, double angle, double startX, double startY,  int width, int height ) {

			TransformGroup transformGroup = new TransformGroup();
			RotateTransform rotateTransform = new RotateTransform( angle );
			rotateTransform.CenterX = sourceImage.PixelWidth / 2.0;
			rotateTransform.CenterY = sourceImage.PixelHeight / 2.0;
			transformGroup.Children.Add( rotateTransform );
			TranslateTransform translateTransform = new TranslateTransform();
			translateTransform.X = -startX;
			translateTransform.Y = -startY;
			transformGroup.Children.Add( translateTransform );

			DrawingVisual vis = new DrawingVisual();
			DrawingContext cont = vis.RenderOpen();
			cont.DrawRectangle( Brushes.White, null, new Rect( 0, 0, width, height ) );

			cont.PushTransform( transformGroup );

			cont.DrawImage( sourceImage, new Rect( new Size( sourceImage.PixelWidth, sourceImage.PixelHeight ) ) );
			cont.Close();

			RenderTargetBitmap rtb = new RenderTargetBitmap( width, height, 96d, 96d, PixelFormats.Default );
			rtb.Render( vis );

			return rtb;
		}

		public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType ) {

			return sourceType == typeof( RotateTransform );
		}

		public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value ) {

			if( value is RotateTransform )
				return new Ruota {
					 gradi = (float)((RotateTransform)value).Angle
				};
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + value.GetType() + " valore=" + value );
		}

		public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType ) {

			return destinationType.IsAssignableFrom( typeof( RotateTransform ) );
		}

		public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object objCorrezione, Type destinationType ) {

			if( objCorrezione is Ruota )
				return new RotateTransform {
					Angle = ((Ruota)objCorrezione).gradi
				};
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + objCorrezione.GetType() + " valore=" + objCorrezione );
		}

	}
}
