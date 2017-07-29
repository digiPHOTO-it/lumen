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

	public class BoolIsNullConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {

			bool esito = false;


			if (value == null)
			{
				esito = true;
			}
			else if (value.Equals(""))
			{
				esito = true;
			}

			if( "Not".Equals( parameter ) )
				esito = !esito;

			return esito;
		}

		public object ConvertBack( object value, Type targetType, object parameter,	CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}