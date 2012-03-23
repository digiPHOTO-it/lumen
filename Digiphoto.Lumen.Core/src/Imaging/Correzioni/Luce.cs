using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging.Correzioni {

	public class Luce : Correzione {

		/// <summary>
		/// Valori ammessi da -1 a +1
		/// </summary>
		public double luminosita;

		/// <summary>
		/// Valori ammessi da 0 a 2
		/// </summary>
		public double contrasto;

		public override bool isSommabile( Correzione altra ) {
			return (altra is Luce);
		}

		public override Correzione somma( Correzione altra ) {
			
			Luce lcAltra = altra as Luce;

			Luce ris = new Luce {
				contrasto = this.contrasto + lcAltra.contrasto,
				luminosita = this.luminosita + lcAltra.luminosita
			};

			return ris;
		}

	}
}
