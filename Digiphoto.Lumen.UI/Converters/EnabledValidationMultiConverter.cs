using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;
using System.Globalization;

namespace Digiphoto.Lumen.UI.Converters {

	public class EnabledValidationMultiConverter : IMultiValueConverter
	{

		#region IValueConverter Members

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			bool enable = true;

			foreach (object vE in values)
			{
				enable = enable && vE as ValidationError == null;
			}

			return enable;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
