using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging.Ritocco {
	
	public class Contrasto : Correzione {
		
		public float fattore { get; set; }

		public Contrasto() {
		}

		public Contrasto( float fattore ) {
			this.fattore = fattore;
		}

		public override Cardinalita getCardinalita() {
			return Cardinalita.SOMMABILE;
		}

	}
}
