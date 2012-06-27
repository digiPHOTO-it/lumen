using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Stampare
{
	public class LavoroDiStampaFoto : LavoroDiStampa
	{
		public Fotografia fotografia
		{
			get;
			private set;
		}

		public ParamStampaFoto param
		{
			get;
			private set;
		}

		
		public LavoroDiStampaFoto( Fotografia fotografia, ParamStampaFoto param ): base(param)
		{
			this.fotografia = fotografia;
			this.param = param;
		}

		public override string ToString() {
			return string.Format( "Foto={0} ; param={1}", fotografia.ToString(), param.ToString() );
		}

	}
}
