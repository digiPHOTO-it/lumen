using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Eventi {

	public enum Esito {
		Ok,
		Errore
	}



	public class Messaggio {

		public Messaggio( object sender ) {
			this.sender = sender;
			timeStamp = DateTime.Now;
			this.descrizione = "Messaggio";
			this.esito = Esito.Ok;
		}

		public Messaggio( object sender, string descrizione ) : this( sender ) {
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

		public object sender {
			get;
			private set;
		}

		public bool showInStatusBar {
			get;
			set;
		}

		/** Questa informazione è a disposizione del programma chiamante.
		 *  Viene ritornata per poter eseguire operazioni postume di chiusura, o convalida
		 */
		public Object senderTag;

		public override string ToString() {
			return this.descrizione + esito != null ? "Esito = " + ((Esito)esito).ToString() : "nullo";
		}

	}
}
