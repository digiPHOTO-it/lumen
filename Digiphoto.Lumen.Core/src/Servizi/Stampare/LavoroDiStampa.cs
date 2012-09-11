using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Collections.Specialized;

namespace Digiphoto.Lumen.Servizi.Stampare {

	public class LavoroDiStampa {

		public enum Stato {
			Accodato,
			InEsecuzione,
			Completato
		}

		public EsitoStampa esitostampa {
			get;
			internal set;
		}

		public Stato stato {
			get;
			internal set;
		}

		public ParamStampa param
		{
			get;
			protected set;
		}

		public LavoroDiStampa(ParamStampa param)
		{
			this.param = param;
		}

	}
}
