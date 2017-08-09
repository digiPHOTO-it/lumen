using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.SelfService.MobileUI.Converters
{

	/// <summary>
	/// Questo convertitore mi serve per convertire una immagine intesa come interfaccia IImmagine
	/// in un componente renderizzabile in un controllo Image.
	/// </summary>
	public class ImageSourceConverter : IValueConverter  {

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            ImageSource imageSource = null;

            if (value == null)
            {
                return imageSource;
            }

            if (value.GetType() == typeof(FotografiaDto))
            {
                SelfServiceClient ssClient = new SelfServiceClient();
                ssClient.Open();

                FotografiaDto fotografia = (FotografiaDto)value;

                imageSource = FotoSrv.Instance.loadPhoto(ssClient, "Provino", fotografia.id);
            }

            if(value.GetType() == typeof(byte[]))
            {
                imageSource = LoadImage((byte[])value);
            }
           
			return imageSource;
		}

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
