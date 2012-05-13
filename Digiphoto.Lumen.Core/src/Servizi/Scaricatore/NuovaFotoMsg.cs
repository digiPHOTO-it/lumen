using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Scaricatore {

	public class NuovaFotoMsg : Messaggio {

		public Fotografia foto {
			get;
			private set;
		}

		public NuovaFotoMsg( object sender, Fotografia foto ) : base( sender ) {
			this.foto = foto;
		}
	}
}
