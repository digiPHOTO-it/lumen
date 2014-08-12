using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;

namespace Digiphoto.Lumen.UI.ScreenCapture {

	public static class SnapshotUtil {

		#region modo1
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
		#endregion

		#region modo2
		public static BitmapImage WindowSnapshotToImage( Visual sourceVisual, Visual targetVisual ) {
			Matrix m = PresentationSource.FromVisual( sourceVisual ).CompositionTarget.TransformToDevice;
			double myDeviceDpiX = m.M11 * 96.0;
			double myDeviceDpiY = m.M22 * 96.0;

			var imgStream = GrabSnapshotStream( targetVisual, myDeviceDpiX, myDeviceDpiY, ImageFormats.JPG );

			BitmapImage imageSource = new BitmapImage();

			using( imgStream ) {
				imgStream.Position = 0;
				imageSource.BeginInit();
				imageSource.CacheOption = BitmapCacheOption.OnLoad;
				imageSource.StreamSource = imgStream;
				imageSource.EndInit();
			}

			return imageSource;
		}

		private static MemoryStream GrabSnapshotStream( Visual targetVisual, double dpiX, double dpiY, ImageFormats imageFormats ) {
			Rect bounds = VisualTreeHelper.GetDescendantBounds( targetVisual );

			BitmapSource renderTargetBitmap = captureVisualBitmap(
				targetVisual,
				dpiX,
				dpiY
				);

			BitmapEncoder bitmapEncoder;

			switch( imageFormats ) {
				case ImageFormats.PNG: {
						bitmapEncoder = new PngBitmapEncoder();
						break;
					}
				case ImageFormats.BMP: {
						bitmapEncoder = new BmpBitmapEncoder();
						break;
					}
				case ImageFormats.JPG: {
						bitmapEncoder = new JpegBitmapEncoder();
						break;
					}
				default:
					throw new NotSupportedException( "The Incorrect Logic" );
			}

			bitmapEncoder.Frames.Add( BitmapFrame.Create( renderTargetBitmap ) );

			// Create a MemoryStream with the image.
			// Returning this as a MemoryStream makes it easier to save the image to a file or simply display it anywhere.
			var memoryStream = new MemoryStream();
			bitmapEncoder.Save( memoryStream );

			return memoryStream;
		}

		private static BitmapSource captureVisualBitmap( Visual targetVisual, double dpiX, double dpiY ) {
			Rect bounds = VisualTreeHelper.GetDescendantBounds( targetVisual );
			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
				(int)(bounds.Width * dpiX / 96.0),
				(int)(bounds.Height * dpiY / 96.0),
				dpiX,
				dpiY,

				//PixelFormats.Default
				PixelFormats.Pbgra32
				);

			DrawingVisual drawingVisual = new DrawingVisual();
			using( DrawingContext drawingContext = drawingVisual.RenderOpen() ) {
				VisualBrush visualBrush = new VisualBrush( targetVisual );
				drawingContext.DrawRectangle( visualBrush, null, new Rect( new Point(), bounds.Size ) );
			}
			renderTargetBitmap.Render( drawingVisual );

			return renderTargetBitmap;
		}

		public enum ImageFormats {
			PNG,
			BMP,
			JPG
		}
		#endregion

	}
}
