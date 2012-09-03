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
				
				// Se non frizzo, non riesco a passare queste bitmap da un thread all'altro.
				this.bitmapSource.Freeze();

			} catch( Exception ee ) {
				// Che posso fare ? Tiriamo avanti
				this.bitmapSource = null;
			}
		}

		#region Proprietà


		/// <summary>
		/// Rappresenta la larghezza della immagine in pixel.
		/// </summary>
		public override int ww {
			get {
				return bitmapSource.PixelWidth;  // VERIFICARE SE CI POSSONO ESSERE PROBLEMI DI PERDITA DI VALORI
			}
		}

		public override int hh {
			get {
				return bitmapSource.PixelHeight;  // VERIFICARE SE CI POSSONO ESSERE PROBLEMI DI PERDITA DI VALORI
			}
		}

		#endregion

		#region Metodi

		public override void Dispose() {
			bitmapSource = null;
		}

		public override object Clone() {

			if( this.bitmapSource == null )
				throw new ObjectDisposedException( "immagineWic" );



			// Per motivi di thread multipli non posso chiamare il clone della mia sorgente, ma ne devo creare una nuova
// TODO purtroppo queste operazioni mi fanno sciupare un sacco di memoria RAM inutile.
//      Devo assolutamente risolvere in altro modo.
var wb = new WriteableBitmap( this.bitmapSource );




			return new ImmagineWic( wb );  // Se la bitmap è nulla mi sta bene che si spacchi. In tal caso correggere il programma chiamante.
		}

		#endregion
	}
}
