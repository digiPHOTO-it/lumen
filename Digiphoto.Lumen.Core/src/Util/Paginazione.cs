using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Util {
	
	public class Paginazione {

		public int skip {
			get;
			set;
		}
		public int take {
			get;
			set;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder( "--Paginazione--" );
			sb.Append( "\r\nSkip: " + skip );
			sb.Append( "\r\nTake: " + take );
			return sb.ToString();
		}
	}
}
