using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Licensing {

	public class LicenseNotFoundException : LumenException {

		public LicenseNotFoundException()
			: base() {
		}

		public LicenseNotFoundException( String msg )
			: base( msg ) {
		}

	}
}
