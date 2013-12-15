using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Digiphoto.Lumen.UI.Converters {

	public class NumericToBooleanConverter : IValueConverter {

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {

			if( IsNumeric( value ) ) {
				double numero;
				if( Double.TryParse( value.ToString(), out numero ) ) {
					return true;
				}
			}

			return false;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}


		public static bool IsNumeric( object obj ) {
			return (obj == null) ? false : IsNumeric( obj.GetType() );
		}

		public static bool IsNumeric( Type type ) {
			
			if( type == null )
				return false;

			TypeCode typeCode = Type.GetTypeCode( type );

			switch( typeCode ) {
				case TypeCode.Byte:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return true;
			}
			return false;
		}

	}
}
