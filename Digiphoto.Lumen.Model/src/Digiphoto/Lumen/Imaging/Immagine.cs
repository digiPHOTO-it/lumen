using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging {

	public enum Orientamento {
		Verticale, Orizzontale
	}

	public interface Immagine {

		int ww { get;	}
		int hh { get; }
		Orientamento orientamento { get; }
		float rapporto { get; }
	}

}
