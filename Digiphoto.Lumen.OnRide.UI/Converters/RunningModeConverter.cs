using Digiphoto.Lumen.OnRide.UI.Config;
using System;
using System.Globalization;

using System.Windows.Data;

namespace Digiphoto.Lumen.OnRide.UI.Converters {

	public class RunningModeConverter : IValueConverter {

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {

			RunningMode runningMode = (RunningMode)value;

			if( "icon".Equals( parameter ) ) {

				string png = null;
				switch( runningMode ) {

					case RunningMode.Automatico:
						png = "RunningMode_Automatico-32x32.png";
						break;

					case RunningMode.Presidiato:
						png = "RunningMode_Presidiato-32x32.png";
						break;
				}

				return new Uri( @"pack://application:,,,/Digiphoto.Lumen.OnRide.UI;component/Resources/" + png );
			}

			throw new NotImplementedException();
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
