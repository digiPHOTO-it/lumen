using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Imaging.Wic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.UI.Converters {

	/// <summary>
	/// Questo convertitore mi serve per convertire un booleano 
	/// </summary>
	public class ModoVenditaBoolConverter : IValueConverter {

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {

			// Carico una icona dal file delle risorse
			ModoVendita modoVendita = (ModoVendita)value;
			return modoVendita == ModoVendita.Carrello ? true : false;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			bool usoCarrello = (bool)value;
			return usoCarrello ? ModoVendita.Carrello : ModoVendita.StampaDiretta;
		}
	}
}
