using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.UI.Main {

	public class CambioPaginaMsg : Messaggio {
		

		public CambioPaginaMsg( object sender ) : base( sender ) {
		}
		
		public string nuovaPag {
			get;
			set;
		}

	}
}
