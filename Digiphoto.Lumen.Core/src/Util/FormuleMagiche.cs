using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Digiphoto.Lumen.Util {
	
	public static class FormuleMagiche {

		/// <summary>
		/// Ci sono dei problemi di rilascio memoria con le Bitmap.
		/// Con questa formula magica, pare che il Garbage Collector si convinca a 
		/// fare il suo dovere.
		/// </summary>
		public static void rilasciaMemoria() {
			Dispatcher.CurrentDispatcher.Invoke( DispatcherPriority.SystemIdle, new DispatcherOperationCallback( delegate {
				return null;
			} ), null );
		}


		public static bool sonoNellaUI = true; 

		/// <summary>
		/// Questo metodo funziona bene se usato nella applicazione WPF grafica.
		/// Se invece viene chiamato dal test-case (senza ui) allora rimane in wait e non esce.
		/// </summary>
		public static void attendiGcFinalizers() {

			if( sonoNellaUI )
				GC.WaitForPendingFinalizers();

			GC.Collect();
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
	}

}
