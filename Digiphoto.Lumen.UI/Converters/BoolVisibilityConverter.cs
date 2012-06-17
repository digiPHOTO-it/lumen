using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.ComponentModel;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.UI.Converters {

	public class BoolVisibilityConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			if (value == null)
			{
				return Visibility.Collapsed;
			}
			if(value is bool){
				if ((bool)value)
				{
					return Visibility.Visible;
				}
				else
				{
					return Visibility.Collapsed;
				}
			}
			
			return Visibility.Collapsed;
		}

		public object ConvertBack( object value, Type targetType, object parameter,	CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}