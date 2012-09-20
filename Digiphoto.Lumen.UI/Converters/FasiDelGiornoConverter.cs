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

	public class FasiDelGiornoConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			if (value != null)
			{
				return FaseDelGiornoUtil.getFaseDelGiorno((short)value);
			}

			return null;
		}

		public object ConvertBack( object value, Type targetType, object parameter,	CultureInfo culture ) {
			if (value != null)
			{
				return (short)value;
			}

			return null;
		}
	}
}