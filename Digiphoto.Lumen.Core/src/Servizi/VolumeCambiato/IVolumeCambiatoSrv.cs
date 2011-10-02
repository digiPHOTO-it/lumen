using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Servizi.VolumeCambiato {
	
	interface IVolumeCambiatoSrv : IServizio {

		/** Se true rimango bloccato chiamando la waitForNextEvent */
		bool attesaBloccante {
			get;
			set;
		}

		/** Rimane in attesa bloccante o non bloccante (a seconda della property omonima)
		 *  che venga inserita o rimossa un disco rimovibile.
		 */
		void attesaEventi();

	}

}
