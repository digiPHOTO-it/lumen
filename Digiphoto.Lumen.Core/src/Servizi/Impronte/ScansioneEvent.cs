using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Core.Servizi.Impronte {

	public class ScansioneEvent : EventArgs {

		public bool isValid {
			get; set;
		}

		public string strBase64Template {
			get;
			set;
		}

		public string bmpFileName {
			get;
			internal set;
		}

		/// <summary>
		/// Timestamp del momemto della scansione.
		/// </summary>
		public DateTime tempo {
			get; set;
		}
	}
}
