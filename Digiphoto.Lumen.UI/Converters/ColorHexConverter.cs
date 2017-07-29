using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Digiphoto.Lumen.UI.Converters {

	public class ColorHexConverter : IValueConverter {

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			var hexCode = System.Convert.ToString( value );
			if( string.IsNullOrEmpty( hexCode ) )
				return null;
			try {
				var color = (Color)ColorConverter.ConvertFromString( hexCode );
				return color;
			} catch {
				return null;
			}
		}
		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {

			
			try {
				var color = (Color)value;
				return color;
			} catch {
			}

			return null;
		}

#if false

		public static System.Windows.Media.Color fromHex( string hex ) {
			string colorcode = hex;
			int argb = Int32.Parse( colorcode.Replace( "#", "" ), System.Globalization.NumberStyles.HexNumber );
			return System.Windows.Media.Color.FromArgb( (byte)((argb & -16777216) >> 0x18),
								  (byte)((argb & 0xff0000) >> 0x10),
								  (byte)((argb & 0xff00) >> 8),
								  (byte)(argb & 0xff) );
		}

 public System.Windows.Media.Color ConvertStringToColor(String hex)
    {
        //remove the # at the front
        hex = hex.Replace("#", "");

        byte a = 255;
        byte r = 255;
        byte g = 255;
        byte b = 255;

        int start = 0;

        //handle ARGB strings (8 characters long)
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            start = 2;
        }

        //convert RGB characters to bytes
        r = byte.Parse(hex.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
        g = byte.Parse(hex.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
        b = byte.Parse(hex.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

        return System.Windows.Media.Color.FromArgb(a, r, g, b);
    }

#endif
	}
}
