﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;

namespace Digiphoto.Lumen.UI.Converters {

	public class BoolInverterConverter : IValueConverter {

		#region IValueConverter Members

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			if( value is bool ) {
				return !(bool)value;
			}
			return value;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			if( value is bool ) {
				return !(bool)value;
			}
			return value;
		}

		#endregion
	}
}
