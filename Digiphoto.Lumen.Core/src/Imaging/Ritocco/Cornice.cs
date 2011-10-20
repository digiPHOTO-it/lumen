using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging.Ritocco {

	public class Cornice : Correzione {

		/** Nome senza path */
		public string nomeCornice { get; set; }

		/** La foto originale, viene ingrandita/ridotta per farla entrare nella cornice */
		public float fattoreZoomFoto;

		/** La foto originale viene ruotata */
		public short gradiRotazione;

		public override Cardinalita getCardinalita() {
			return Cardinalita.DISTINTI;
		}
	}
}
