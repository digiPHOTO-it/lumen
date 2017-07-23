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
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Util;
using System.IO;

namespace Digiphoto.Lumen.UI.Converters {

	/// <summary>
	/// Questo convertitore mi serve per convertire una immagine intesa come interfaccia IImmagine
	/// in un componente renderizzabile in un controllo Image.
	/// </summary>
	public class CorrezioniXmlImageSourceConverter : IValueConverter  {

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			
			ImageSource imageSource = null;

			if (value is String)
			{
				imageSource = caricaMascheraDaCorrezioneXml((String)value);
			}

			return imageSource;
		}

		private static ImageSource caricaMascheraDaCorrezioneXml( string correzioneXml ) 
		{
			ImageSource imageSource = caricaImmagineDefault();
			CorrezioniList correzioni = SerializzaUtil.stringToObject<CorrezioniList>((String)correzioneXml);
			if (correzioni != null && correzioni.Contains(typeof(Maschera)))
			{
				ImmagineWic immagineMaschera = null;
				Maschera maschera = (Maschera)correzioni.FirstOrDefault(c => c is Maschera);
				if (maschera != null)
				{
					immagineMaschera = new ImmagineWic(Path.Combine(PathUtil.getCartellaMaschera(FiltroMask.MskSingole), maschera.nome));
				}

				if (immagineMaschera != null)
				{
					imageSource = ((ImmagineWic)immagineMaschera).bitmapSource as ImageSource;
				}
			}
			
			return imageSource;
		}

		private static ImageSource caricaImmagineDefault()
		{
			BitmapImage image = new BitmapImage();

			try
			{
				image.BeginInit();
				image.UriSource =  new Uri(@"pack://application:,,,/Digiphoto.Lumen.UI;component/Resources/cornice_empty-48x48.png");
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
