using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Digiphoto.Lumen.SelfService.MobileUI.Converters {

	public class CompareConverter : IValueConverter {

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {

			return parameter.Equals( value );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {

			return ((bool)value) ? parameter : Binding.DoNothing;
		}
	}
}
