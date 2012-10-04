using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.UI.Converters {

	public class PercentualeConverterSlideX : IValueConverter {

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			return (System.Convert.ToDouble(value, culture) - Configurazione.UserConfigLumen.slideBoundsX) * (System.Convert.ToDouble(parameter, culture)/100);
		}

		public object ConvertBack( object value, Type targetType, object parameter,	CultureInfo culture ) {
			return System.Convert.ToDouble(value, culture) / (System.Convert.ToDouble(parameter, culture)/100) + Configurazione.UserConfigLumen.slideBoundsX;
		}
	}
}