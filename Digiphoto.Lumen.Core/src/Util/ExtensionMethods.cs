using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Util {

	public static class ExtensionMethods {

		public static T[] ConvertToArray<T>( this IEnumerable<T> enumerable ) {
			if( enumerable == null )
				throw new ArgumentNullException( "enumerable" );

			return enumerable as T[] ?? enumerable.ToArray();
		}

		/// <summary>
		/// Metodo di estensione statica.
		/// Si può usare su di un qualunque oggetto per farne una copia.
		/// In pratica si serializza in memorie e si ri-istanzia un altro oggetto 
		/// che diventa un clone del primo
		/// 
		/// In pratica realizza una Deep Copy che è il contrario della ShallowCopy
		/// </summary>
		/// <typeparam name="T">La classe dell'oggtto da copiare</typeparam>
		/// <param name="source">L'oggetto da copiare</param>
		/// <returns></returns>
		public static T deepCopy<T>( this T source ) {
			var isNotSerializable = !typeof( T ).IsSerializable;
			if( isNotSerializable )
				throw new ArgumentException( "The type must be serializable.", "source" );

			var sourceIsNull = ReferenceEquals( source, null );
			if( sourceIsNull )
				return default( T );

			var formatter = new BinaryFormatter();
			using( var stream = new MemoryStream() ) {
				formatter.Serialize( stream, source );
				stream.Seek( 0, SeekOrigin.Begin );
				return (T)formatter.Deserialize( stream );
			}
		}

		public static IEnumerable<T> ToIEnumerable<T>( this IEnumerator<T> enumerator ) {
			while( enumerator.MoveNext() ) {
				yield return enumerator.Current;
			}
		}
	}
}
