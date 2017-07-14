using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.ComponentModel;
using Digiphoto.Lumen.Model;
using System.Windows.Controls;

namespace Digiphoto.Lumen.UI.Converters {

	public class BooleanToSelectionModeConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			bool multipla = (bool)value;
			return multipla ? SelectionMode.Multiple : SelectionMode.Single;
		}

		public object ConvertBack( object value, Type targetType, object parameter,	CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}