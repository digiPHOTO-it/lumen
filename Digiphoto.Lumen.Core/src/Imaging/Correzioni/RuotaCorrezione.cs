using System;


namespace Digiphoto.Lumen.Imaging.Correzioni {

	public class RuotaCorrezione : Correzione {

		public float gradi;
		public bool scartoAutomatico;
		public string backgroudColor;

		public override bool isSommabile( Correzione altra ) {

			return( altra is RuotaCorrezione );
		}
		
		// Sommo i gradi.
		public override Correzione somma( Correzione altra ) {

			Correzione ret = null;

			if( isSommabile( altra ) ) {
				RuotaCorrezione ruotaAltra = (RuotaCorrezione)altra;
				this.gradi = this.gradi + ruotaAltra.gradi;
				ret = this;
			}

			return ret;
		}
	}
}
