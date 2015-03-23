using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Digiphoto.Lumen.SelfService.WebUI {

	public class ParamRicerca {

		public DateTime? giorno {
			get;
			set;
		}

		public string idFotografo {
			get;
			set;
		}

		public int numPagina {
			get;
			set;
		}

	}
}