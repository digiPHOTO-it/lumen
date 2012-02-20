using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;

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

			try {

				// Soluzione 1 (tiene bloccato il file)			
				//BitmapImage bitmapImage = new BitmapImage();
				//bitmapImage.BeginInit();
				//bitmapImage.UriSource = new Uri( uriString );
				//bitmapImage.EndInit();
				// this.bitmapSource = bitmapImage;

				// Soluzione 2 (forzo il caricamento in memoria della bitmap. Non tiene bloccato il file)
				// BitmapSource bitmapImage = BitmapFrame.Create( new Uri( uriString ), BitmapCreateOptions.None, BitmapCacheOption.OnLoad );
				// this.bitmapSource = bitmapImage;

				// Soluzione 3 (carico diretto da stream di byte)
				MemoryStream data = new MemoryStream( File.ReadAllBytes( uriString ) );
				this.bitmapSource = BitmapFrame.Create( data );

			} catch( Exception ee ) {
				// Che posso fare ? Tiriamo avanti
				this.bitmapSource = null;
			}
		}

		#region Proprietà


		/// <summary>
		/// Rappresenta la larghezza della immagine in pixel.
		/// </summary>
		public override long ww {
			get {
				return (int)bitmapSource.PixelWidth;  // VERIFICARE SE CI POSSONO ESSERE PROBLEMI DI PERDITA DI VALORI
			}
		}

		public override long hh {
			get {
				return (int)bitmapSource.PixelHeight;  // VERIFICARE SE CI POSSONO ESSERE PROBLEMI DI PERDITA DI VALORI
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
