﻿using System;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging;


namespace Digiphoto.Lumen.Model {

	// Questi attributi sono transienti e non li gestisco sul database.
	// Ci penserò io a riempirli a mano
	public partial class Fotografia {

		public Immagine imgOrig {
			get;
			set;
		}

		public Immagine imgProvino {
			get;
			set;
		}

		public Immagine imgRisultante {
			get;
			set;
		}

	}
}
