using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;
using System.IO;

namespace Digiphoto.Lumen.Servizi.Scaricatore {
		
	public enum Fase {
			FineScarico,
			FineLavora
	};

	public class ScaricoFotoMsg : Messaggio {

		public ScaricoFotoMsg( object sender, string descrizione ) : base( sender, descrizione ) {
		}

		/// <summary>
		/// Occhio può contenere il nome della cartella, oppure il nome del file singolo scaricato.
		/// </summary>
		public string sorgente {
			get;
			set;
		}

		public EsitoScarico esitoScarico {
			get;
			set;
		}
		
		public Fase fase {get;	set;}


		public bool _puoiTogliereLaFlashCard {
			get;
			set;
		}
	}
}
