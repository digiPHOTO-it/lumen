using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System;
using System.Collections.Generic;
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

namespace Digiphoto.Lumen.UI.Carrelli.SelfService {
	/// <summary>
	/// Interaction logic for QRcodeSelfService.xaml
	/// </summary>
	public partial class QRcodeSelfService : Window {
		public QRcodeSelfService() {
			InitializeComponent();

			creaImmagineQR();
		}

		private void creaImmagineQR() {

			QrEncoder encoder = new QrEncoder( ErrorCorrectionLevel.M );
			QrCode qrCode;
			encoder.TryEncode( "http://192.168.1.4:5462/Carrello/Details/369845ba-995a-4dc9-a19d-eb4c77eddc24", out qrCode );
			var fms = new FixedModuleSize( 8, QuietZoneModules.Two );
			WriteableBitmapRenderer wRenderer = new WriteableBitmapRenderer( fms, Colors.Black, Colors.White );
			WriteableBitmap wBitmap = new WriteableBitmap( 400, 400, 96, 96, PixelFormats.Gray8, null );
			wRenderer.Draw( wBitmap, qrCode.Matrix );

			imgQRcode.Source = wBitmap;
		}
	}
}
