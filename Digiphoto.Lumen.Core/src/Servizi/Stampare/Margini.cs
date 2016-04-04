using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Core.Servizi.Stampare {

	public class Margini {

		public Margini() {
		}

		public Margini( double left, double right, double top, double bottom ) {
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}
		
		public Double left { get; set; }

		public Double right { get; set; }

		public Double top { get; set; }

		public Double bottom { get; set; }
	}
}
