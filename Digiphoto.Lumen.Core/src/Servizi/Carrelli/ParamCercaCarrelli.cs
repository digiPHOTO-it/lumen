using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Servizi.Carrelli {
	


	public class ParamCercaCarrelli : ParamCerca {

		/** Filtro sul timestamp di registrazione */
		public DateTime? dateTimeRegistrazioneIniz { get; set; }
		public DateTime? dateTimeRegistrazioneFine { get; set; }

		/**   0=tutti  ;  1=solo temporanei  ; 2=solo definitivi */
		short stato;

		/** Eventuale like sulla descrizione (tutta maiuscola) */
		string intestazione;
	}
}
