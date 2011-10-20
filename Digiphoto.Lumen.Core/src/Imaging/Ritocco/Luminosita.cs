using System;

namespace Digiphoto.Lumen.Imaging.Ritocco {
	
	public class Luminosita : Correzione {

		#region Proprietà

		public float fattore { get; set; }
			
		#endregion

		public Luminosita() {
		}

		public Luminosita( float fattore ) {
			this.fattore = fattore;
		}

		public override Cardinalita getCardinalita() {
			return Cardinalita.SOMMABILE;
		}

	}
}
