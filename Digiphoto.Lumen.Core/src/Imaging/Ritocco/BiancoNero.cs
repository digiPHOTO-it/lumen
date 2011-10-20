using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Digiphoto.Lumen.Imaging.Ritocco {

	public class BiancoNero : Correzione {

		/** E' inutile applicare il bianco/nero più di una volta */
		public override Cardinalita getCardinalita() {
			return Cardinalita.UNOSOLO;
		}
	}
}
