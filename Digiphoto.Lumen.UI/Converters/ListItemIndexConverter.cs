using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Imaging.Wic;
using System.Windows.Media;
using System.Windows.Controls;

namespace Digiphoto.Lumen.UI.Converters {

	/// <summary>
	/// Questo convertitore mi serve per convertire una immagine intesa come interfaccia IImmagine
	/// in un componente renderizzabile in un controllo Image.
	/// </summary>
	public class ListItemIndexConverter : IValueConverter
	{

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			ListBoxItem item = value as ListBoxItem;
            ListBox view = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
			int index = view.ItemContainerGenerator.IndexFromContainer(item);
			return index+1;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
