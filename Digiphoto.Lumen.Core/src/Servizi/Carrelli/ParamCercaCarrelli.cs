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
	}
}
