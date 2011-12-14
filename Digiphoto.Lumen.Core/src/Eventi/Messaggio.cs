﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Eventi {

	public enum Esito {
		Ok,
		Errore
	}

	public class Messaggio : EventArgs {

		public Messaggio() {
			timeStamp = DateTime.Now;
		}

		public Messaggio( string descrizione ) : this() {
			this.descrizione = descrizione;
		}

		/** Descrizione dell'episodio */
		public string descrizione {
			get;
			set;
		}

		/** Indica quando è avvenuto questo episodio (il momento della creazione) */
		public DateTime timeStamp {
			get;
			set;
		}

		public Esito? esito {
			get;
			set;
		}

	}
}
