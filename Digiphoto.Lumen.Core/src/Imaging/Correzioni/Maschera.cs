using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging.Correzioni {

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

	}
}
