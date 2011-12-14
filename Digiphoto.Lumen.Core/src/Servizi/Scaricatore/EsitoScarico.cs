using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Digiphoto.Lumen.Servizi.Scaricatore {

	public class EsitoScarico {

		public EsitoScarico() {
			fotoDaLavorare = new List<FileInfo>();
		}

		public IList<FileInfo> fotoDaLavorare {
			get;
			set;
		}

		public bool riscontratiErrori {
			get;
			set;
		}

		public int totFotoCopiateOk {
			get;
			set;
		}

		public int totFotoNonCopiate {
			get;
			set;
		}

		public int totFotoNonEliminate {
			get;
			set;
		}


	}
}
