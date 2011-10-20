using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Digiphoto.Lumen.Imaging.Ritocco {

	public class Specchio : Correzione {
		
		/** Può essere V=verticale ; O=Orizzontale */
		public char direzione {	get; set; }

		public Specchio() {
		}

		public Specchio( char direzione ) {
			this.direzione = direzione;
		}

		public override Cardinalita getCardinalita() {
			return Cardinalita.UNOSOLO;
		}
	}
}
