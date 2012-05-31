using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Servizi.Reports {

	public class RigaReportVendite  {

		public RigaReportVendite() {
			this.dettaglioFormatiCarta = new List<ReportVenditeDettaglio>();
		}

		public DateTime giornata {get; set;}
		public Decimal totIncassoCalcolato {get; set;}
		public Decimal totIncassoDichiarato {get; set;}
		public int totFotoStampate {get; set;}
		public int totFotoScattate {get; set;}
		public int totDischettiMasterizzati {get; set;}

		public IList<ReportVenditeDettaglio> dettaglioFormatiCarta;
	}

	public class ReportVenditeDettaglio {

		public string desFormatoCarta { get; set; }
		public int totFoto { get; set; }
	}


}
