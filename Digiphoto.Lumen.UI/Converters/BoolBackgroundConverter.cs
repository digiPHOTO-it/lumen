using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.ComponentModel;
using Digiphoto.Lumen.Model;
using System.Drawing;

namespace Digiphoto.Lumen.UI.Converters {

	public class BoolBackgroundConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			if (value == null)
			{
				return null;
			}
			if(value is bool){
				if ((bool)value)
				{
					return Color.Red.Name;
				}
				else
				{
					return null;
				}
			}

			return null;
		}

		public object ConvertBack( object value, Type targetType, object parameter,	CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}