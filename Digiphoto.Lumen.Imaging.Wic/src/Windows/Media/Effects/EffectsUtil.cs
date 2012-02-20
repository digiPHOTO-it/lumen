using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Digiphoto.Lumen.Windows.Media.Effects {

	public class EffectsUtil {

		public static BitmapSource RenderToBitmap( FrameworkElement target ) {

			int actualWidth = (int)target.ActualWidth;
			int actualHeight = (int)target.ActualHeight;

			Rect boundary = VisualTreeHelper.GetDescendantBounds( target );
			RenderTargetBitmap renderBitmap = new RenderTargetBitmap( actualWidth, actualHeight, 96, 96, PixelFormats.Pbgra32 );

			DrawingVisual drawingVisual = new DrawingVisual();
			using( DrawingContext context = drawingVisual.RenderOpen() ) {
				VisualBrush visualBrush = new VisualBrush( target );
				context.DrawRectangle( visualBrush, null, new Rect( new Point(), boundary.Size ) );
			}

			renderBitmap.Render( drawingVisual );
			return renderBitmap;
		}

		/// <summary>
		/// Data una Bitmap e una collezione di effetti, 
		/// applica gli effetti e mi ritorna un Array di Byte.
		/// Comodo se si vuole salvare su file il risultato.
		/// </summary>
		public static byte [] RenderImageWithEffectsToBuffer( BitmapSource image, IEnumerable<ShaderEffect> effects ) {

			int ww = (int)image.PixelWidth;
			int hh = (int)image.PixelHeight;

			BitmapSource bs = RenderImageWithEffectsToBitmap( image, effects );

			byte [] buffer = new byte [ww * hh * 4];
			bs.CopyPixels( buffer, 4 * ww, 0 );
			return buffer;

		}

		/// <summary>
		/// Data una Bitmap e una collezione di effetti, 
		/// applica gli effetti e mi ritorna una altra BitmapSource.
		/// Comodo se si vuole continuare a lavorare in memoria.
		/// 
		/// Attenzione: visto che si può passare una collezione di effetti, cercare di chiamare questo 
		/// metodo poche volte con molti effetti, piuttosto che una volta per ogni effetto. In questo
		/// modo sarà più veloce il processo.
		/// </summary>
		public static BitmapSource RenderImageWithEffectsToBitmap( BitmapSource image, IEnumerable<ShaderEffect> effects ) {
			
			int ww = (int)image.PixelWidth;
			int hh = (int)image.PixelHeight;

			effects = effects.Reverse();
			Grid root = new Grid();
			Arrange( root, ww, hh );
			Grid current = root;
			foreach( var shaderEffect in effects ) {
				var effect = new Grid();
				Arrange( effect, ww, hh );
				effect.Effect = shaderEffect;
				current.Children.Add( effect );
				current = effect;
			}

			Image img = new Image();
			img.Source = image;
			Arrange( img, ww, hh );
			current.Children.Add( img );
			
			// Soluzione 1
			BitmapSource bs = RenderToBitmap( root );

			// Soluzione 2
			//RenderTargetBitmap rtb = new RenderTargetBitmap();
			//rtb.Render( root );

			return bs;
		}

		private static void Arrange( UIElement element, int width, int height ) {
			element.Measure( new Size( width, height ) );
			element.Arrange( new Rect( 0, 0, width, height ) );
			element.UpdateLayout();
		}

	}
}
