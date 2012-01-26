using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.Imaging.Wic {

	public class ImmagineWic : Immagine {

		public BitmapSource bitmapSource {
			get;
			private set;
		}

		public ImmagineWic( BitmapFrame bitmapFrame ) {
			this.bitmapSource = bitmapFrame;
		}

		public ImmagineWic( string uriString ) {

			BitmapImage bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.UriSource = new Uri( uriString );
			bitmapImage.EndInit();
			this.bitmapSource = bitmapImage;
		}

		#region Proprietà

		public override int ww {
			get {
				return (int)bitmapSource.Width;  // VERIFICARE SE CI POSSONO ESSERE PROBLEMI DI PERDITA DI VALORI
			}
		}

		public override int hh {
			get {
				return (int)bitmapSource.Height;  // VERIFICARE SE CI POSSONO ESSERE PROBLEMI DI PERDITA DI VALORI
			}
		}

		#endregion

		#region Metodi

		public override void Dispose() {
			bitmapSource = null;
		}

		#endregion
	}
}
