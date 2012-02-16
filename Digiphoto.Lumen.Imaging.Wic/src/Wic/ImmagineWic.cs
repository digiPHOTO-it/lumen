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

		public ImmagineWic( BitmapSource bitmapSource ) {
			this.bitmapSource = bitmapSource;
		}

		public ImmagineWic( string uriString ) {

			/*
			 * SOLUZIONE 1 : ok ma tiene loccato il file.
			BitmapImage bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.UriSource = new Uri( uriString );
			bitmapImage.EndInit();
			*/

			BitmapSource bitmapImage = BitmapFrame.Create( new Uri( uriString ), BitmapCreateOptions.None, BitmapCacheOption.OnLoad );

			/*
			MemoryStream data = new MemoryStream( File.ReadAllBytes( file ) );
			BitmapSource bitmap = BitmapFrame.Create( data );
			*/

			this.bitmapSource = bitmapImage;
		}

		#region Proprietà

		public override long ww {
			get {
				return (int)bitmapSource.Width;  // VERIFICARE SE CI POSSONO ESSERE PROBLEMI DI PERDITA DI VALORI
			}
		}

		public override long hh {
			get {
				return (int)bitmapSource.Height;  // VERIFICARE SE CI POSSONO ESSERE PROBLEMI DI PERDITA DI VALORI
			}
		}

		#endregion

		#region Metodi

		public override void Dispose() {
			// Questo dovrebbe servire a rilasciare il file su disco dove punta l'immagine
			bitmapSource = null;
		}

		#endregion
	}
}
