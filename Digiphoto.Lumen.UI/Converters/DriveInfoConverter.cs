using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.UI.Converters {

	public class DriveInfoConverter : IValueConverter {

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {

			DriveInfo driveInfo = (DriveInfo)value;
						
			if( "DESCRIZ".Equals( parameter ) ) {
				string desc = null;
				try {
					if( !string.IsNullOrWhiteSpace( driveInfo.VolumeLabel ) )
						desc = driveInfo.VolumeLabel;
				} catch( Exception ) {
				} 

				if( desc == null )
					desc = driveInfo.DriveType.ToString();

				if( desc == null )
					desc = "disco senza nome";

				return desc;
			}

			if( "ICONA".Equals( parameter ) ) {
				// Devo tornare una image source
				try {
					string uriString = String.Format( @"pack://application:,,,/Digiphoto.Lumen.UI;component/Resources/DriveType_{0}-64x64.png", driveInfo.DriveType.ToString() );
					Uri iconaUri = new Uri( uriString );
					return new BitmapImage( iconaUri );
				} catch( Exception ) {
				}
			}

			if( "FREESPACEMB".Equals( parameter ) ) {
				// torno lo spazio libero in Mega 
				try {
					if( driveInfo.IsReady ) {
						return (long) (driveInfo.TotalFreeSpace / Math.Pow( 2, 20 ) );
					} 
				} catch( Exception ) {
				}
			}

			if( "TOTALSPACEMB".Equals( parameter ) ) {
				// torno lo spazio totale in Mega 
				try {
					if( driveInfo.IsReady ) {
						return (long)(driveInfo.TotalSize / Math.Pow( 2, 20 ));
					}
				} catch( Exception ) {
				}
			}

			return null;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
