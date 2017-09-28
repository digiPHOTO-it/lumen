using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging.Correzioni {

	// TODO appena possibile rinominare in Mascheratura
	public class Maschera : Correzione {

		/// <summary>
		/// Il nome della cornice non comprende il path.
		/// E' solo il nome del file PNG.
		/// </summary>
		public String nome {
			get;
			set;
		}

		public double width {
			get;
			set;
		}

		public double height {
			get;
			set;
		}

		/// <summary>
		/// Se indicato, significa che la cornice indicata, va applicata con un
		/// rapporto forzato (es: 4/3)
		/// In questo caso, prima di applicarla, devo fare uno stretch dell'immagine.
		/// </summary>
		public string expRatioForzato {
			get;
			set;
		}

	}
}
