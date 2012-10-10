using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Eventi {

	/// <summary>
	/// Questa classe serve per dare una breve e coincisa informazione all'utente
	/// di qualcosa che è avvenuto nel programma.
	/// Questi messaggi vengono visualizzati nella status bar.
	/// </summary>
	public class InformazioneUtente {

		public InformazioneUtente( string testo ) {
			this.testo = testo;
		}

		public InformazioneUtente( string testo, Esito esito ) : this( testo ) {
			this.esito = esito;
		}

		public string testo {
			get;
			set;
		}

		public Esito? esito {
			get;
			set;
		}

	}
}
