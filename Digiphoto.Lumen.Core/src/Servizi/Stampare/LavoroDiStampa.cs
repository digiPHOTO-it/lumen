﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Collections.Specialized;

namespace Digiphoto.Lumen.Servizi.Stampare {



	public class LavoroDiStampa {

		public Fotografia fotografia {
			get;
			private set;
		}

		public ParamStampaFoto param {
			get;
			private set;
		}

		public enum Stato {
			Nullo,
			Inserito,
			Accodato,
			InEsecuzione,
			Annullato,
			TerinatoOk
		}

		public Stato stato {
			get;
			internal set;
		}

		public LavoroDiStampa( Fotografia fotografia, ParamStampaFoto param ) {
			this.fotografia = fotografia;
			this.param = param;
			this.stato = Stato.Nullo;  // Ancora non ho fatto niente.
		}

		public override string ToString() {
			return string.Format( "Foto={0} ; param={1}", fotografia.ToString(), param.ToString() );
		}

	}
}
