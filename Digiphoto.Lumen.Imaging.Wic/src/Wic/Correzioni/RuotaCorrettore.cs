using Digiphoto.Lumen.Imaging.Correzioni;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Digiphoto.Lumen.Imaging.Correzioni;
using System;
using System.Windows;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {

	/// <summary>
	/// 
	/// </summary>
	public class RuotaCorrettore : Correttore {


		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			BitmapSource bmpsource = ((ImmagineWic)immagineSorgente).bitmapSource;
			RuotaCorrezione ruotaCorrezione = (RuotaCorrezione)correzione;

			BitmapSource newBmp;
			if( ruotaCorrezione.isAngoloRetto )
				newBmp = rotazioneSemplice( bmpsource, ruotaCorrezione.gradi );
			else
				newBmp = rotazioneComplessa( bmpsource, ruotaCorrezione.gradi );

			return new ImmagineWic( newBmp );
		}


		/// <summary>
		/// Questa rotazione funziona se l'angolo è retto (multiplo di 90°)
		/// </summary>
		/// <param name="bmpSorgente"></param>
		/// <param name="gradi">deve essere multiplo di 90</param>
		/// <returns></returns>
		private BitmapSource rotazioneSemplice( BitmapSource bmpSorgente, double gradi ) {

			// Create the TransformedBitmap to use as the Image source.
			TransformedBitmap tb = new TransformedBitmap();

			// Properties must be set between BeginInit and EndInit calls.
			tb.BeginInit();

			tb.Source = bmpSorgente;
			RotateTransform transform = new RotateTransform( gradi );
			tb.Transform = transform;

			tb.EndInit();

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


	}
}
