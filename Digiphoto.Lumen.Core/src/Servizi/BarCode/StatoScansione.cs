using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Servizi.BarCode {
	
	public class StatoScansione {
		
		public int percentuale { get; set;  }
		public int totale { get; set; }
		public int attuale { get; set; }
		public int barcodeTrovati { get; set; }
	}
}
