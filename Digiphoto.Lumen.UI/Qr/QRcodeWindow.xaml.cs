using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace Digiphoto.Lumen.UI.Qr {


	public partial class QRcodeWindow : Window {

		[System.Runtime.InteropServices.DllImport( "gdi32.dll" )]
		public static extern bool DeleteObject( IntPtr hObject );


		public QRcodeWindow() {

			InitializeComponent();

			DataContextChanged += QRcodeWindow_DataContextChanged;
		}

		private void QRcodeWindow_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {
			creaImmagineQR( (string) e.NewValue );
		}

		private void creaImmagineQR( string qrValue ) {

			// creo il writer
			BarcodeWriter barcodeWriter = new BarcodeWriter {
				Format = BarcodeFormat.QR_CODE,
				Options = new EncodingOptions {
					Width = 440,
					Height = 440,
					Margin = 4
				}
			};

			// scrivo la bitmap
			using( Bitmap bitmap = barcodeWriter.Write( qrValue ) ) {
				IntPtr hbmp = bitmap.GetHbitmap();
				try {
					BitmapSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
						hbmp,
						IntPtr.Zero,
						Int32Rect.Empty,
						BitmapSizeOptions.FromEmptyOptions() );
					imgQRcode.Source = source; // set WPF image source
				} finally {
					DeleteObject( hbmp );
				}
			}

		}

		static void writeToFile( MemoryStream stream ) {

			using( FileStream file = new FileStream( @"c:\tmp\qrCode.png", FileMode.Create, System.IO.FileAccess.Write ) ) {
				byte[] bytes = new byte[stream.Length];
				stream.Read( bytes, 0, (int)stream.Length );
				file.Write( bytes, 0, bytes.Length );
				//stream.Close();
			}
		}


	}
}
