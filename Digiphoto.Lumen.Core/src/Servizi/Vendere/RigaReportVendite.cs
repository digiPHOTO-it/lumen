using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Servizi.Reports {

	public class RigaReportVendite  {

		static int MAX_FORMATI = 4;

		public RigaReportVendite() {
			this.dettaglioFormatiCarta = new ReportVenditeDettaglio[MAX_FORMATI];
		}

		public DateTime giornata {get; set;}
		
		public int totFotoStampate {get; set;}
		public int totFotoMasterizzate {get; set;}
		public int totFotoScattate {get; set;}

		// Questo è preso dai carrelli (qui non si scappa)
		public Decimal totIncassoCalcolato {get; set;}
		public Decimal totIncassoDichiarato {get; set;}

		// Chiusura di cassa (queste uno potrebbe dimenticarsi di farle)
		public Nullable<Decimal> ccTotIncassoPrevisto {get; set;}
		public Nullable<Decimal> ccTotIncassoDichiarato {get; set;}

		/// <summary>
		/// Ritorna la quantità venduta di quel formato carta
		/// </summary>
		/// <param name="formato"></param>
		/// <returns></returns>
		public int getQta( String formato ) {
			var elem = dettaglioFormatiCarta.SingleOrDefault( r => r.desFormatoCarta == formato );
			return elem == null ? 0 : elem.totFoto;
		}


		// Qui sono obbligato a mettere in colonna le varie righe di prodotto
		public string formato1 { get; set; }
		public string formato2 { get; set; }
		public string formato3 { get; set; }
		public string formato4 { get; set; }
		public string formato5 { get; set; }

		public int? qtaFormato1 { get { return dettaglioFormatiCarta[0] == null ? (int?)null : dettaglioFormatiCarta[0].totFoto; } }
		public int? qtaFormato2 { get { return dettaglioFormatiCarta[1] == null ? (int?)null : dettaglioFormatiCarta[1].totFoto; } }
		public int? qtaFormato3 { get { return dettaglioFormatiCarta[2] == null ? (int?)null : dettaglioFormatiCarta[2].totFoto; } }
		public int? qtaFormato4 { get { return dettaglioFormatiCarta[3] == null ? (int?)null : dettaglioFormatiCarta[3].totFoto; } }

		public ReportVenditeDettaglio [] dettaglioFormatiCarta;
	}

	public class ReportVenditeDettaglio {
		public string desFormatoCarta { get; set; }
		public int totFoto { get; set; }
	}
}
