using System;
using System.Windows.Data;

namespace Digiphoto.Lumen.SelfService.MobileUI.Converters
{
	/// <summary>
	/// Questo convertitore mi serve per convertire un booleano 
	/// </summary>
	public class MiPiaceBoolConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{

			// Carico una icona dal file delle risorse
			bool? miPiace = (bool?)value;
			return miPiace == true ? true : false;
        }

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool? miPiace = (bool?)value;
			return miPiace == true ? true : false;
		}
	}
}
