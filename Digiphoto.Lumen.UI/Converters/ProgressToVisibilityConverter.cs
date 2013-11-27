using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Digiphoto.Lumen.UI.Converters {

	public class ProgressToVisibilityConverter : IValueConverter {

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {

			bool isNumerico = false;
			short progresso = -99;
			if( value != null )
				isNumerico = Int16.TryParse( value.ToString(), out progresso );

			if( parameter == null ) {
				// Lavoro con la visibilita
				if( isNumerico ) {
					return (progresso <= 0 || progresso >= 100) ? Visibility.Hidden : Visibility.Visible;
				} else
					return Visibility.Hidden;
			} else {
				// Gestisco una stringa in formato nn:mm
				// Se la progressione è 0,100 allora ritorno nn alrimenti mm
				string [] vetPar = parameter.ToString().Split( ':' );
				if( isNumerico )
					return (progresso <= 0 || progresso >= 100) ? Double.Parse(vetPar[0]) : Double.Parse(vetPar[1]);
				else
					return Double.Parse( vetPar[0] );
			}
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
