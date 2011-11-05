using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using System.Drawing;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Imaging.Nativa.Correzioni {

	internal class CorrettoreFactory {

		public Correttore creaCorrettore( Type tipoCorrezione ) {
			
			Correttore correttore = null;

			// In base al tipo di correzione, istanzio il giusto correttore
			if( tipoCorrezione.Equals( typeof( BiancoNeroCorrezione ) ) )
				correttore = new BiancoNeroCorrettore();

			return correttore;
		}
		
	}
}
