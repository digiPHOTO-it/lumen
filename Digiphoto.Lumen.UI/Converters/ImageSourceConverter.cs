using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Imaging.Wic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.UI.Converters {

	/// <summary>
	/// Questo convertitore mi serve per convertire una immagine intesa come interfaccia IImmagine
	/// in un componente renderizzabile in un controllo Image.
	/// </summary>
	public class ImageSourceConverter : IValueConverter  {

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {

			ImageSource imageSource = null;

			if( value is IImmagine ) {
				imageSource = ((ImmagineWic)value).bitmapSource as ImageSource;
			} else if( value is Digiphoto.Lumen.Eventi.Esito ) {
				// Carico una icona dal file delle risorse
				Esito esito = (Esito)value;
				System.IO.Stream stream = null;
				if( esito == Esito.Ok )
					stream = this.GetType().Assembly.GetManifestResourceStream( "Digiphoto.Lumen.UI.Resources.information.ico" );
				else if( esito == Esito.Errore )
					stream = this.GetType().Assembly.GetManifestResourceStream( "Digiphoto.Lumen.UI.Resources.error.ico" );
				if( stream != null ) {
					//Decode the icon from the stream and set the first frame to the BitmapSource
					BitmapDecoder decoder = IconBitmapDecoder.Create( stream, BitmapCreateOptions.None, BitmapCacheOption.None );
					imageSource = decoder.Frames [0];
				}
			} else if( value is String ) {
				imageSource = caricaImmagine( value as string );
			} else
				return value;

			return imageSource;
		}

		private static ImageSource caricaImmagine( string uriString ) {

			BitmapImage image = new BitmapImage();

			try {
				image.BeginInit();
				image.UriSource = new Uri( uriString as string, UriKind.Absolute );
				image.EndInit();
			} catch {
				// Qui si potrebbe emettere una warning
			}

			return image;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
