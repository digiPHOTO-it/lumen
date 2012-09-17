using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.UI.PanAndZoom {

	public class PanAndZoomViewModel : ClosableWiewModel {

		public PanAndZoomViewModel( string nomeFileImmagine ) {
			loadImage( nomeFileImmagine );
		}

		public ImageSource imageSource {
			get;
			private set;
		}

		private void loadImage( string nomeFile ) {

			BitmapImage msk = new BitmapImage();
			msk.BeginInit();
			msk.UriSource = new Uri( nomeFile );
			msk.EndInit();

			imageSource = msk;
		}
	}
}
