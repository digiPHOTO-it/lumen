using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core;

namespace Digiphoto.Lumen.Servizi.Ricerca {

	public class ParamRicerca {

		public Evento [] eventi { get; set; }
		public string Didascalia { get; set; }
		public Fotografo [] fotografo { get; set; }
		public FaseDelGiorno [] fasiDelGiorno { get; set; }
		public int [] numeriFotogrammi;

		DateTime? giornataIniz { get; set; }
		DateTime? giornataFine { get; set; }
	}

	interface IRicercatoreSrv {
	}
}
