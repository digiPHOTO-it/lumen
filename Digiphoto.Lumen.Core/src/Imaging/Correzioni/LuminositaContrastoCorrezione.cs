using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging.Correzioni {

	public class LuminositaContrastoCorrezione : Correzione {

		/// <summary>
		/// Valori ammessi da -1 a +1
		/// </summary>
		public double luminosita;

		/// <summary>
		/// Valori ammessi da 0 a 2
		/// </summary>
		public double contrasto;

		public override bool isSommabile( Correzione altra ) {
			return (altra is LuminositaContrastoCorrezione);
		}

		public override Correzione somma( Correzione altra ) {
			
			LuminositaContrastoCorrezione lcAltra = altra as LuminositaContrastoCorrezione;

			LuminositaContrastoCorrezione ris = new LuminositaContrastoCorrezione {
				contrasto = this.contrasto + lcAltra.contrasto,
				luminosita = this.luminosita + lcAltra.luminosita
			};

			return ris;
		}

	}
}
