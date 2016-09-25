using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Servizi.EliminaFotoVecchie {
	
public class FotoEliminateMsg : Messaggio {

		public FotoEliminateMsg( object sender ) : base( sender ) {
		}

		public IEnumerable<Fotografia> listaFotoEliminate {
			get;
			set;
		}
	}
}
