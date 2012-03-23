using System;


namespace Digiphoto.Lumen.Imaging.Correzioni {

	public class Ruota : Correzione {

		public float gradi;
		public bool scartoAutomatico;
		public string backgroudColor;

		public override bool isSommabile( Correzione altra ) {

			return( altra is Ruota );
		}
		
		// Sommo i gradi.
		public override Correzione somma( Correzione altra ) {

			Correzione ret = null;

			if( isSommabile( altra ) ) {
				Ruota ruotaAltra = (Ruota)altra;
				this.gradi = this.gradi + ruotaAltra.gradi;
				ret = this;
			}

			return ret;
		}

		public override bool isInutile {
			get {
				return (gradi == 0f || (gradi % 360) == 0);
			}
		}

		public bool isAngoloRetto {
			get {
				return (gradi % 90f) == 0;
			}
		}

	}
}
