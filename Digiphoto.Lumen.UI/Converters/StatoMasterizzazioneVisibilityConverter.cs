using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Digiphoto.Lumen.UI.Converters {

	public class StatoMasterizzazioneVisibilityConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			if (value == null)
			{
				return Visibility.Hidden;
			}
			if(value is Digiphoto.Lumen.Servizi.Masterizzare.Fase)
            {
				if (Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa == (Digiphoto.Lumen.Servizi.Masterizzare.Fase)value)
				{
					return Visibility.Hidden;
				}
				else if (Digiphoto.Lumen.Servizi.Masterizzare.Fase.CopiaCompletata == (Digiphoto.Lumen.Servizi.Masterizzare.Fase)value)
                {
					return Visibility.Hidden;
				}
                else
                {
                    return Visibility.Visible;
                }
            }
			
			return Visibility.Hidden;
		}

		public object ConvertBack( object value, Type targetType, object parameter,	CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}