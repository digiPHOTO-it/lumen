using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.src.Util;

namespace Digiphoto.Lumen.UI.Converters {

	public class CompNumFotoConverter : IValueConverter {

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			if (!string.IsNullOrEmpty("" + value) && Configurazione.UserConfigLumen.compNumFoto)
			{
				return CompNumFoto.getStringValue(Int64.Parse("" + value));
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!string.IsNullOrEmpty("" + value) && Configurazione.UserConfigLumen.compNumFoto)
			{
				return CompNumFoto.getLongValue("" + value);
			}
			return value;
		}
	}
}
