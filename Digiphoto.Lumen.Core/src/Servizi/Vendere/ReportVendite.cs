using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Servizi.Reports {

	public class ReportVendite {

		public ReportVendite() {
			mappaRighe = new Dictionary<DateTime, RigaReportVendite>();
		}


		public Dictionary<DateTime, RigaReportVendite> mappaRighe;

		/// <summary>
		/// L'ordine di questa lista corrisponde alle colonne dei formati nelle varie righe
		/// </summary>
		public List<String> formatiCartaVenduti;

	}
}
