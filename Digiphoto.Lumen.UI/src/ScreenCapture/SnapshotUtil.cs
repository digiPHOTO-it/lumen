using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Digiphoto.Lumen.UI.ScreenCapture {
	
	public static class SnapshotUtil {

		public static BitmapSource CreateBitmap( FrameworkElement frameworkElement, bool isInUiTree ) {
			
			if( !isInUiTree ) {
				frameworkElement.Measure( new Size( double.PositiveInfinity, double.PositiveInfinity ) );
				frameworkElement.Arrange( new Rect( new Point( 0, 0 ), frameworkElement.DesiredSize ) );
			}

			int width = (int)Math.Ceiling( frameworkElement.ActualWidth );
			int height = (int)Math.Ceiling( frameworkElement.ActualHeight );

			width = width == 0 ? 1 : width;
			height = height == 0 ? 1 : height;

			RenderTargetBitmap rtbmp = new RenderTargetBitmap(
				width, height, 96, 96, PixelFormats.Default );
			rtbmp.Render( frameworkElement );
			return rtbmp;
		}
	}
}
