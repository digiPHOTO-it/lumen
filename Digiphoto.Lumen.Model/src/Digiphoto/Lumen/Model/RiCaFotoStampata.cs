using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Model {
	
	public partial class RiCaFotoStampata {

		public string nomeStampante {
			get;
			set;
		}

		/// <summary>
		/// Stampa con i bordi bianchi.
		/// </summary>
		public bool bordiBianchi {
			get;
			set;
		}
	}
}
