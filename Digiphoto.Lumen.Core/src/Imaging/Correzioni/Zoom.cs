using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging.Correzioni {
	
	public class Zoom : Correzione {

		public Zoom() : base() {
			fattore = 1;  // Fattore moltiplicativo
		}

		public double fattore {
			get;
			set;
		}

		public override bool isInutile {
			get {
				return fattore == 1;
			}
		}

		public override bool isSommabile( Correzione altra ) {
			return (altra is Zoom);
		}

		public override Correzione somma( Correzione altra ) {
			return new Zoom {
				fattore = this.fattore + ((Zoom)altra).fattore
			};
		}
	}
}
