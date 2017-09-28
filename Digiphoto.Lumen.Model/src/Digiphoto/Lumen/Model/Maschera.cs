using Digiphoto.Lumen.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Model {

	public enum FiltroMask {
		MskSingole,
		MskMultiple
	}

	/// <summary>
	/// Questa entità fa parte del modello anche se non è persistente
	/// </summary>
	public class Maschera {

		public IImmagine imgProvino { get; set; }
		
		public IImmagine imgOriginale { get; set; }

		public string nomeFile { get; set; }

		public FiltroMask tipo { get; set; }
	}
}
