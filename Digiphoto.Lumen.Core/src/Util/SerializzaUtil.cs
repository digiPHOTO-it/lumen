using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections;
using System.IO;
using System.Xml;
using Digiphoto.Lumen.Imaging.Ritocco;
using System.Xml.Linq;

namespace Digiphoto.Lumen.Util {

	public static class SerializzaUtil {

		public static string objectToString( Object obj ) {
			return objectToString( obj, obj.GetType() );
		}

		public static string objectToString( Object obj, System.Type objType ) {

			XmlSerializer ser;

			if( obj is IEnumerable ) {

				List<Type> extraTipi = new List<Type>();

				// Carico una lista
				IEnumerator itera = ((IEnumerable)obj).GetEnumerator();
				while( itera.MoveNext() ) {
					Object oo = itera.Current;
					if( extraTipi.Contains( oo.GetType() ) == false )
						extraTipi.Add( oo.GetType() );
				}

				ser = new XmlSerializer( objType, extraTipi.ToArray() );

			} else {
				ser = new XmlSerializer( objType );
			}

			StringWriter sw = new StringWriter();
			ser.Serialize( sw, obj );
			string ret = sw.ToString();
			sw.Close();
			return ret;
		}

		/** Creo un oggetto deserializzando la stringa xml passata */
		public static object stringToObject( string xml, System.Type objType ) {

			XmlSerializer ser = new XmlSerializer( objType );
			return ser.Deserialize( new StringReader( xml ) );
		}

		/** Questo metodo è uguale a quello sopra ma faccio già il cast in uscita */
		public static T stringToObject<T>( string xml ) {
			return (T)stringToObject( xml, typeof(T) );
		}

	}
}
