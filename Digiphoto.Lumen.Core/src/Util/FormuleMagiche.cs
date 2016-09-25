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



	}

}
