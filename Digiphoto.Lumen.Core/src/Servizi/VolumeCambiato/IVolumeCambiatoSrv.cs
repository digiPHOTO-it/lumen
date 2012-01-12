using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Digiphoto.Lumen.Servizi.VolumeCambiato {
	
	public interface IVolumeCambiatoSrv : IServizio {

		/** Se true rimango bloccato chiamando la waitForNextEvent */
		bool attesaBloccante {
			get;
			set;
		}

		/** Rimane in attesa bloccante o non bloccante (a seconda della property omonima)
		 *  che venga inserita o rimossa un disco rimovibile.
		 */
		void attesaEventi();

		/// <summary>
		/// Ritorna l'elenco dei dischi rimovibili USB che sono pronti ed attivi.
		/// </summary>
		/// <returns></returns>
		DriveInfo [] GetDrivesUsbAttivi();

	}

}
