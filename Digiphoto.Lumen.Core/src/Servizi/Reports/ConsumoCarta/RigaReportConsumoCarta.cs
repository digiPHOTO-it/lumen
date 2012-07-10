using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using System.Windows.Forms;

namespace Digiphoto.Lumen.Servizi.Reports.ConsumoCarta
{
	public class RigaReportConsumoCarta
	{
		public DateTime giornata { get; set; }
		public int diCuiFoto { get; set; }
		public int diCuiProvini { get; set; }
		public string descFormatoCarta { get; set; }

		public static List<RigaReportConsumoCarta> righe(ParamRangeGiorni paramRangeGiorni)
		{
			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

			IEnumerable<ConsumoCartaGiornaliero> righeConsumoCarta = dbContext.ConsumiCartaGiornalieri.Where(cC => cC.giornata >= paramRangeGiorni.dataIniz && cC.giornata <= paramRangeGiorni.dataFine);
			
			List<RigaReportConsumoCarta> righe = new List<RigaReportConsumoCarta>();

			foreach(ConsumoCartaGiornaliero cC in righeConsumoCarta){
				RigaReportConsumoCarta rPCC = new RigaReportConsumoCarta();
				rPCC.diCuiFoto = cC.diCuiFoto;
				rPCC.diCuiProvini = cC.diCuiProvini;
				rPCC.descFormatoCarta = cC.formatoCarta.descrizione;
				rPCC.giornata = cC.giornata;
				righe.Add(rPCC);
			}
			return righe;
		}
	}
}
