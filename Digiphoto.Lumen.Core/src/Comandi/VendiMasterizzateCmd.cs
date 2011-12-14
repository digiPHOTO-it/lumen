using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Comandi {

	/**
	 * Le stampe indicate vengono aggiunte al masterizzatore e contemporaneamente anche al carrello
	 */
	public class VendiMasterizzateCmd : Comando {

		public VendiMasterizzateCmd( Target target ) {
		}


		internal override Esito esegui( Fotografia foto ) {
			// TODO
			throw new NotImplementedException();
		}
	}
}
