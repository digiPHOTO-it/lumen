using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Comandi {

	/**
	 * Le stampe indicate devono essere vendute.
	 * Se lavoro con il carrello, verranno solo aggiunte al carrello.
	 * Se lavoro in stampa diretta, verrà creato un nuovo carrello e subito stampato.
	 */
	public class VendiStampeCmd : Comando {

		public VendiStampeCmd( Target target ) {
		}


		internal override Esito esegui( Fotografia foto ) {
			// TODO
			throw new NotImplementedException();
		}
	}
}
