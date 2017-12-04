using Digiphoto.Lumen.Eventi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Eventi {
	
	public class RilevataInconsistenzaDatabaseMsg : Messaggio {

		public RilevataInconsistenzaDatabaseMsg( object sender ) : base( sender ) {
		}

		public Nullable<DateTime> giornataDaVerificare { get; set; }
	}
}
