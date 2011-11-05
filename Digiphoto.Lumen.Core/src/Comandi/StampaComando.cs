using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Comandi {


	public class StampaComando : Comando {

		public StampaComando() {
		}

		public StampaComando( Target target ) : base( target ) {
		}

		internal override Esito esegui( Fotografia foto ) {
			// TODO
			throw new NotImplementedException();
		}
	}
}
