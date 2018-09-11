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
		
		public int totFotoStampate {get; set;}
		public int totFotoMasterizzate {get; set;}
		public int totFotoScattate {get; set;}
		public int totDischettiMasterizzati {get; set;}

		// Questo è preso dai carrelli (qui non si scappa)
		public Decimal totIncassoCalcolato {get; set;}
		public Decimal totIncassoDichiarato {get; set;}

		// Chiusura di cassa (queste uno potrebbe dimenticarsi di farle)
		public Nullable<Decimal> ccTotIncassoPrevisto {get; set;}
		public Nullable<Decimal> ccTotIncassoDichiarato {get; set;}

		public IList<ReportVenditeDettaglio> dettaglioFormatiCarta;
	}

	public class ReportVenditeDettaglio {
		public string desFormatoCarta { get; set; }
		public int totFoto { get; set; }
	}
}
