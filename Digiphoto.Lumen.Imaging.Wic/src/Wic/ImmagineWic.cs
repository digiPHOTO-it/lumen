using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using log4net;

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

			try {
				_giornale.Debug( "carico immagine da disco : " + uriString );

				const char metodo = 'B';

				if( metodo == 'A' ) {
					// Soluzione A  : NON FUNZIONA!!! Lascia il file aperto su disco.
					BitmapImage bitmapImage = new BitmapImage();
					bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
					bitmapImage.BeginInit();
					bitmapImage.UriSource = new Uri( uriString );
					bitmapImage.EndInit();

					this.bitmapSource = bitmapImage;

				} else {
					// Soluzione B (carico diretto da stream di byte). In questo caso, però chi è che fa il dispose ?
					MemoryStream data = new MemoryStream( File.ReadAllBytes( uriString ) );
					this.bitmapSource = BitmapFrame.Create( data );
				}

				_giornale.Debug( "ok caricata. Ora freezzo" );

				// Se non frizzo, non riesco a passare queste bitmap da un thread all'altro.
				this.bitmapSource.Freeze();

				_giornale.Debug( "ok freezata" );

			} catch( Exception ee ) {
				_giornale.Error( "fallita creazione immagine " + uriString, ee );
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
var wb = new WriteableBitmap( this.bitmapSource );




			return new ImmagineWic( wb );  // Se la bitmap è nulla mi sta bene che si spacchi. In tal caso correggere il programma chiamante.
		}

		#endregion
	}
}
