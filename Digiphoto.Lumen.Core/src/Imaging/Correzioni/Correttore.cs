using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Imaging.Correzioni {

	public abstract class Correttore {

		public abstract Immagine applica( Immagine immagineSorgente, Correzione correzione );

	}
}
