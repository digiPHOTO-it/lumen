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

	public class BoolWindowStateConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			if (value == null)
			{
				return WindowState.Normal;
			}
			if(value is bool){
				if ((bool)value)
				{
					return WindowState.Maximized;
				}
				else
				{
					return WindowState.Normal;
				}
			}

			return WindowState.Normal;
		}

		public object ConvertBack( object value, Type targetType, object parameter,	CultureInfo culture ) {
			if(value==null){
				return false;
			}

			if (value is WindowState)
			{
				if ((WindowState)value == WindowState.Normal)
				{
					return false;
				}
				else
				{
					return true;
				}
			}

			return false;
		}
	}
}