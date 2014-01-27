using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Digiphoto.Lumen.Util {
	
	public class CoreUtil {

		// Questa formula magica, l'ho trovata qui: 
		// http://stackoverflow.com/questions/8742454/saving-a-fixeddocument-to-an-xps-file-causes-memory-leak/8827967#8827967
		// e senza di questa ti assicuro che il GC non riesce a pulire la memoria occupata da lla Bitmap
		public static void abraCadabra() {
			Dispatcher.CurrentDispatcher.Invoke( DispatcherPriority.SystemIdle, new DispatcherOperationCallback( delegate {
				return null;
			} ), null );
		}

	}
}
