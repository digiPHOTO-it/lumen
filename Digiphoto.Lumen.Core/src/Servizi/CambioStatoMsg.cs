using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Servizi {

	public class CambioStatoMsg : Messaggio {

		public CambioStatoMsg( object sender ) : base( sender ) {
		}

		// lo metto intero cosi lo posso confrontare con qualsiasi enumeration
		public int nuovoStato {
			get;
			set;
		}
	}
}
