using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Digiphoto.Lumen.UI.Converters {

	public class PercentualeConverter : IValueConverter {

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {

            Double inputValue = System.Convert.ToDouble(value, culture);

            if (parameter != null)
            {
                string[] parameters = ((String)parameter).Split(new char[] { ';' });

                Double result = inputValue * (System.Convert.ToDouble(parameters[0], culture) / 100);

                if (parameters.Count() > 1)
                {
                    Double minValue = System.Convert.ToDouble(parameters[1], culture);

                    if (result < minValue)
                        return minValue;
                    else
                        return result;
                }
            }

            return inputValue * (System.Convert.ToDouble(parameter, culture) / 100); ;

		}

		public object ConvertBack( object value, Type targetType, object parameter,	CultureInfo culture ) {
            Double inputValue = System.Convert.ToDouble(value, culture);

            if (parameter != null)
            {
                string[] parameters = ((String)parameter).Split(new char[] { ';' });

                Double result = inputValue / (System.Convert.ToDouble(parameters[0], culture) / 100);

                if (parameters.Count() > 1)
                {
                    Double minValue = System.Convert.ToDouble(parameters[1], culture);

                    if (result < minValue)
                        return minValue;
                    else
                        return result;
                }
            }

            return inputValue / (System.Convert.ToDouble(parameter, culture) / 100);
		}
	}
}