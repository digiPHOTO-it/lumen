using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Digiphoto.Lumen.Servizi.Ritoccare.Clona {

	public class EsitoClone {

		public EsitoClone() {
			fotoDaClonare = new List<FileInfo>();
		}

		public IList<FileInfo> fotoDaClonare {
			get;
			set;
		}

		public bool riscontratiErrori {
			get;
			set;
		}

		public int totFotoClonateOk {
			get;
			set;
		}

		public int totFotoNonClonate {
			get;
			set;
		}

		public int totFotoNonEliminate {
			get;
			set;
		}


	}
}
