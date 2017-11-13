using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.ComponentModel;

namespace Digiphoto.Lumen.UI.Converters {

	public class CollectionViewVisibilityConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {

			Visibility ret = Visibility.Hidden;

			if( value is ICollectionView ) {
				if( (value as ICollectionView).IsEmpty ) {
					ret = Visibility.Hidden;
				} else {
					ret = Visibility.Visible;
				}
			}

			// Se richiesto dal parametro, inverto il risultato
			if( "Not".Equals( parameter ) ) {
				if( ret == Visibility.Hidden )
					ret = Visibility.Visible;
				else
					ret = Visibility.Hidden;
			}

			return ret;
		}

		public object ConvertBack( object value, Type targetType, object parameter,	CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}