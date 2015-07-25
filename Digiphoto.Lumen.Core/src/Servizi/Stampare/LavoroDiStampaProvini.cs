using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Stampare
{
	public class LavoroDiStampaProvini : LavoroDiStampa
	{
		public IList<Fotografia> fotografie
		{
			get;
			private set;
		}

		public new ParamStampaProvini param
		{
			get;
			private set;
		}

		public LavoroDiStampaProvini(IList<Fotografia> fotografie, ParamStampaProvini param) : base(param) 
		{
			this.fotografie = fotografie.OrderBy( f => f.giornata ).ThenBy( f => f.numero ).ToList();
			this.param = param;
		}

		public override string ToString() {
			return string.Format( "Job Stampa Provini " + param.numeroColonne + "x" + param.numeroRighe + ". (" + fotografie.Count + ")" );
		}

	}
}
