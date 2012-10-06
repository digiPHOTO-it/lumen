using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Imaging.Wic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
			} else if( value is String ) {

				BitmapImage image = new BitmapImage();

				try {
					image.BeginInit();
					image.UriSource = new Uri( value as string, UriKind.Absolute );
					image.EndInit();
					imageSource = image;
				} catch {
					// Qui si potrebbe emettere una warning
				}

			} else
				return value;

			return imageSource;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
