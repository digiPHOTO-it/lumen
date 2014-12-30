using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using log4net;
using System.Windows;
using System.Windows.Media;

namespace Digiphoto.Lumen.Imaging.Wic {

	public class ImmagineWic : Immagine {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( ImmagineWic ) );

		public BitmapSource bitmapSource {
			get;
			private set;
		}

		public ImmagineWic( BitmapSource bitmapSource ) {
			this.bitmapSource = bitmapSource;
		}

		public ImmagineWic( string uriString ) {

			caricaImmagineDaDisco( uriString );
		}

		private void caricaImmagineDaDisco( String uriString ) {

			_giornale.Debug( "carico immagine da disco : " + uriString );

			const char metodo = 'C';

			if( metodo == 'A' ) {
				// Soluzione A  : NON FUNZIONA!!! Lascia il file aperto su disco.
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.BeginInit();
				bitmapImage.UriSource = new Uri( uriString );
				bitmapImage.EndInit();

				this.bitmapSource = bitmapImage;

			} else if( metodo == 'B' ) {

				// Soluzione B (carico diretto da stream di byte). In questo caso, però chi è che fa il dispose ?
				// Questa soluzione mi da problemi quando "torno originale".
				// E' incredibile ma con questa modalità, questo testcase non funziona: outOfMemoryTest

				MemoryStream data = new MemoryStream( File.ReadAllBytes( uriString ) );
				this.bitmapSource = BitmapFrame.Create( data );

			} else if( metodo == 'C' ) {
				
				// Soluzione C (puttanazza eva vaffanculo alla microsoft. Ci fosse qualcosa che funziona.
				memoryStream = new MemoryStream();
				BitmapImage bi = new BitmapImage();
				byte[] bytArray = File.ReadAllBytes( uriString );
				memoryStream.Write( bytArray, 0, bytArray.Length );
				memoryStream.Position = 0;
				bi.BeginInit();
				bi.StreamSource = memoryStream;
				bi.EndInit();
				this.bitmapSource = bi;
					
			} else if( metodo == 'D' ) {

				using( FileStream fs = new FileStream( uriString, FileMode.Open ) ) {
					this.bitmapSource = CreateImageSource( fs );
					fs.Close();
				}	
			}

			_giornale.Debug( "ok caricata. Ora freezzo" );

			// Se non frizzo, non riesco a passare queste bitmap da un thread all'altro.
			this.bitmapSource.Freeze();

			_giornale.Debug( "ok freezata" );
		}

		#region Proprietà

		private MemoryStream memoryStream { get; set; }

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

			if( memoryStream != null ) {
				memoryStream.Dispose();
				memoryStream = null;
			}

			if( bitmapSource != null ) {
				bitmapSource = null;				
			}

		}

		public override object Clone() {

			if( this.bitmapSource == null )
				throw new ObjectDisposedException( "immagineWic" );

			// Per motivi di thread multipli non posso chiamare il clone della mia sorgente, ma ne devo creare una nuova
			// TODO purtroppo queste operazioni mi fanno sciupare un sacco di memoria RAM inutile.
			//      Devo assolutamente risolvere in altro modo.
			WriteableBitmap wb = new WriteableBitmap( this.bitmapSource );

			ImmagineWic immagine = new ImmagineWic( wb );
			return immagine;  // Se la bitmap è nulla mi sta bene che si spacchi. In tal caso correggere il programma chiamante.
		}


		private static BitmapSource CreateImageSource( Stream stream ) {
			BitmapImage bi = new BitmapImage();
			bi.BeginInit();
			bi.CacheOption = BitmapCacheOption.OnLoad;
			bi.StreamSource = stream;
			bi.EndInit();
			bi.Freeze();
			BitmapSource prgbaSource = new FormatConvertedBitmap( bi, PixelFormats.Pbgra32, null, 0 );
			WriteableBitmap bmp = new WriteableBitmap( prgbaSource );
			int w = bmp.PixelWidth;
			int h = bmp.PixelHeight;
			int[] pixelData = new int[w * h];
			//int widthInBytes = 4 * w;
			int widthInBytes = bmp.PixelWidth * (bmp.Format.BitsPerPixel / 8); //equals 4*w
			bmp.CopyPixels( pixelData, widthInBytes, 0 );
			bmp.WritePixels( new Int32Rect( 0, 0, w, h ), pixelData, widthInBytes, 0 );
			bi = null;
			return bmp;
		}

		public override byte[] getBytes() {

			MemoryStream memStream = new MemoryStream();
			JpegBitmapEncoder encoder = new JpegBitmapEncoder();
			BitmapFrame bf = BitmapFrame.Create( bitmapSource );
			bf.Freeze();
			encoder.Frames.Add( bf );
			encoder.Save( memStream );
			return memStream.GetBuffer();

		}

		#endregion
	}
}
