using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Model {

	// Questi attributi sono transienti e non li gestisco sul database.
	// Ci penserò io a riempirli a mano
	public partial class Fotografia {

		public Immagine imgOrig {
			get;
			private set;
		}

		public Immagine imgProvino {
			get;
			private set;
		}

		public Immagine imgRisultante {
		}

	}
}
