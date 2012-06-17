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
			if (value == null)
			{
				return Visibility.Collapsed;
			}

			else if (value is ICollectionView)
			{
				if ((value as ICollectionView).IsEmpty)
				{
					return Visibility.Collapsed;
				}
				else
				{
					return Visibility.Visible;
				}
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack( object value, Type targetType, object parameter,	CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}