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
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.UI.Converters {

	/// <summary>
	/// Questo convertitore mi serve per convertire una immagine intesa come interfaccia IImmagine
	/// in un componente renderizzabile in un controllo Image.
	/// </summary>
	public class ModoVenditaImageSourceConverter : IValueConverter  {

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {

			ImageSource imageSource = null;

			if( value is IImmagine ) {
				imageSource = ((ImmagineWic)value).bitmapSource as ImageSource;
			}
			else if (value is ModoVendita)
			{
				string uriTemplate = @"pack://application:,,,/Digiphoto.Lumen.UI;component/Resources/##-48x48.png";
				Uri uri = null;
				
				// Carico una icona dal file delle risorse
				ModoVendita modoVendita = (ModoVendita)value;

				if (modoVendita == ModoVendita.Carrello)
					uri = new Uri(uriTemplate.Replace("##", "Carrello"));
				else if (modoVendita == ModoVendita.StampaDiretta)
					uri = new Uri(uriTemplate.Replace("##", "Printer"));

				if (uri != null)
				{
					imageSource = caricaImmagine(uri);
				}
			} else if( value is String ) {
				imageSource = caricaImmagine(new Uri(value as string, UriKind.Absolute));
			} else
				return value;

			return imageSource;
		}

		private static ImageSource caricaImmagine(Uri uri)
		{

			BitmapImage image = new BitmapImage();

			try
			{
				image.BeginInit();
				image.UriSource = uri;
				image.EndInit();
			}
			catch
			{
				// Qui si potrebbe emettere una warning
			}

			return image;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
