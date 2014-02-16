using System;

namespace Digiphoto.Lumen.Servizi.Reports {
	
	public class RigaReportProvvigioni {

		public string nomeFotografo { get; set; }
		public decimal incasso { get; set; }
		public decimal incassoStampe { get; set; }
		public decimal incassoMasterizzate { get; set; }
		public int contaStampe { get; set; }
		public int contaMasterizzate { get; set; }
	}
}
