using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.ComponentModel;

namespace Digiphoto.Lumen.UI.Converters {

	public class CollectionViewVisibilityMultiConverter : IMultiValueConverter
	{
		public object Convert( object[] value, Type targetType, object parameter, CultureInfo culture ) {
			if (value == null)
			{
				return Visibility.Collapsed;
			}

			if (value[1] is bool)
			{
				if ((bool)value[1])
				{
					return Visibility.Collapsed;
				}
			}

			if(value[0] is ICollectionView)
			{
				if ((value[0] as ICollectionView).IsEmpty)
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

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}