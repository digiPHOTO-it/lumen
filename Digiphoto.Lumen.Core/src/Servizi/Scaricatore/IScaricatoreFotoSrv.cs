using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Core;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Scaricatore {

	public enum StatoScarica {
		Idle=401,        // in attesa che qualcuno inserisca una memory-card da scaricare
		Scaricamento=402,  // Sto scaricando le foto dalla memory-card all'HardDisk
		Provinatura=403    // Sto creando i provini
	}

	public class ParamScarica {

		public ParamScarica() {
			// Per comodita istanzio anche l'oggetto contenuto.
			flashCardConfig = new FlashCardConfig();
		}

		public FlashCardConfig flashCardConfig { get; set; }
		public string cartellaSorgente  { get; set; }
		public string nomeFileSingolo { get; set; }
		public bool eliminaFilesSorgenti { get; set; }
		public FaseDelGiorno? faseDelGiorno  { get; set; }
		public bool ricercaBarCode { get; set; }

		public override string ToString() {
			
			StringBuilder sb = new StringBuilder( "Cartella = " ).Append( cartellaSorgente );
			sb.Append( "\n" );
			sb.Append( flashCardConfig.ToString() );
			sb.Append( "\nElimina files = " ).Append( eliminaFilesSorgenti );
			if( faseDelGiorno != null )
				sb.Append( "\nFase del giorno = " ).Append( faseDelGiorno );
			return sb.ToString();
		}
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

		StatoScarica statoScarica {
			get;
		}

	}
}
