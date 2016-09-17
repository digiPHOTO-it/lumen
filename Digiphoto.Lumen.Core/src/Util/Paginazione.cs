using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Digiphoto.Lumen.Util {

	[Serializable]
	public class Paginazione {
		
		public int skip {
			get;
			set;
		}
		public int take {
			get;
			set;
		}

		public bool isEmpty {
			get {
				return skip == 0 && take == 0;
			}
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder( "--Paginazione--" );
			sb.Append( "\r\nSkip: " + skip );
			sb.Append( "\r\nTake: " + take );
			return sb.ToString();
		}

	}
}
