using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Util {

	public class LumenException : Exception {

		public LumenException() : base() {
		}

		public LumenException( string message ) : base( message ) {
		}

		public LumenException( string message, Exception innerException ): base( message, innerException ) {
		}

	}
}
