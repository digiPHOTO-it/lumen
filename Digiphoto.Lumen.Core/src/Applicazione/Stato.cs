using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Applicazione {

	public sealed class Stato {

		
		internal Stato() {
		}

		/**
		 * La giornata lavorativa, rappresenta la data legale e non quella "solare" del calendario.
		 * Infatti è possibile che le foto acquisite alle 2 di notte vadano nel giorno precedente.
		 */
		public DateTime giornataLavorativa {
			get; 
			internal set;
		}


	}
}
