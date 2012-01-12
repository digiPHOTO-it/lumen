using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Core;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Scaricatore {

	public class ParamScarica {

		public ParamScarica() {
			// Per comodita istanzio anche l'oggetto contenuto.
			flashCardConfig = new FlashCardConfig();
		}

		public FlashCardConfig flashCardConfig { get; set; }
		public string cartellaSorgente  { get; set; }
		public bool eliminaFilesSorgenti { get; set; }
		public FaseDelGiorno faseDelGiorno  { get; set; }
	}

	public interface IScaricatoreFotoSrv : IServizio {

		ParamScarica ultimaChiavettaInserita {
			get;
		}

		/** Ricava l'elenco di tutti i fotografi attivi ordinati per INIZIALE (e non per cognome) */
		IEnumerable<Fotografo> fotografiAttivi {
			get;
		}

		bool battezzaFlashCard( ParamScarica param );

		/** Questo metodo non ritorna nulla, perchè la copia avviene in asincrono */
		void scarica( ParamScarica param );



		void addNuovoFotografo( string cognomeNome );
	}
}
