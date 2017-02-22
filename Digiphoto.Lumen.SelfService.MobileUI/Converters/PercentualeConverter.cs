using System;
using System.Windows.Data;
using System.Globalization;

namespace Digiphoto.Lumen.SelfService.MobileUI.Converters {

	public class PercentualeConverter : IValueConverter {

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			return System.Convert.ToDouble(value, culture) * (System.Convert.ToDouble(parameter, culture) / 100);
		}

		public object ConvertBack( object value, Type targetType, object parameter,	CultureInfo culture ) {
			return System.Convert.ToDouble(value, culture) / (System.Convert.ToDouble(parameter, culture) / 100);
		}
	}
}