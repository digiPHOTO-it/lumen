using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Eventi {

    public class Messaggio : EventArgs
    {

		public Messaggio() {
			timeStamp = DateTime.Now;
		}

		public Messaggio( string descrizione ) : this() {
			this.descrizione = descrizione;
		}

		public Messaggio( Object sender, EventArgs eventArgs ) : this() {
			this.sender = sender;
			this.eventArgs = eventArgs;
		}

		public string descrizione {
			get;
			set;
		}

		public object sender {
			get;
			set;
		}

		public EventArgs eventArgs {
			get;
			set;
		}

		/** Indica quando è avvenuto questo episodio (il momento della creazione) */
		public DateTime timeStamp {
			get;
			set;
		}
	}
}
