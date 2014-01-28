using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.ComponentModel;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.UI.Converters {

	public class SumQuantitaConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {

			if (value == null)
			{
				return 0;
			}
			else if (value is ICollectionView)
			{
				if (!(value as ICollectionView).IsEmpty)
				{
					short quantitaTotale = 0;

					ICollectionView cV = (ICollectionView)value;

					foreach (RigaCarrello riga in cV)
					{
						if( riga.discriminator.Equals( parameter ) )
							quantitaTotale += riga.quantita;
					};

					return quantitaTotale;
				}
			}
			else if(true)
			{
				ICollectionView cV = CollectionViewSource.GetDefaultView(value);

				if (cV!=null)
				{
					short quantitaTotale = 0;

					foreach (RigaCarrello riga in cV)
					{
						if( riga.discriminator.Equals( parameter ) )
							quantitaTotale += riga.quantita;
					};

					return quantitaTotale;
				}
			}
			return 0;
		}

		public object ConvertBack( object value, Type targetType, object parameter,	CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}