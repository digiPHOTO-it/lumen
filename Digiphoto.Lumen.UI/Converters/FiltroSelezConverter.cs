using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Digiphoto.Lumen.UI.Converters {

	public class FiltroSelezConverter : IValueConverter {

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {

			object ret = null;

			if( value is ModalitaFiltroSelez ) {
				if( ((ModalitaFiltroSelez)value) == ModalitaFiltroSelez.Tutte )
					ret = false;
				else
					ret = true;
			}

			return ret;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {

			if( value is bool ) {

				if( ((bool)value) == true )
					return ModalitaFiltroSelez.SoloSelezionate;
				else
					return ModalitaFiltroSelez.Tutte;
			}

			return null;
		}
	}
}
