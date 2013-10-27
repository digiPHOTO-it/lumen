using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {

	/// <summary>
	/// E' un correttore finto.
	/// </summary>
	public class GimpCorrettore : Correttore {

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {
			return immagineSorgente;
		}

	}
}
