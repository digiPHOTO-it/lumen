using System;
using System.Globalization;
using System.Windows.Data;

namespace Digiphoto.Lumen.SelfService.MobileUI.Converters {

	public class GiornataFiltroConverter : IValueConverter {

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {

			int numGiorni = Int32.Parse( (String)parameter );

			DateTime test1 = DateTime.Today.AddDays( -1 * numGiorni );
			DateTime test2 = (DateTime)value;

			return test1.Equals( test2 );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {

			bool test = (bool)value;
			if( test == false )
				return null;

			int numGiorni = Int32.Parse( (String)parameter );
			return DateTime.Today.AddDays( -1 * numGiorni );
		}
	}
}
