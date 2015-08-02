using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Digiphoto.Lumen.Imaging.Correzioni;

namespace Digiphoto.Lumen.Imaging.Correzioni {

	public class AreaRispetto : Correzione {

		public AreaRispetto() {
		}

		public AreaRispetto( float ratio ) {
			this.ratio = ratio;
		}

		// 
		public float ratio {
			get;
			set;
		}

	}
}
