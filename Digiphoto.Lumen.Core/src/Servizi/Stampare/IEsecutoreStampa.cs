using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Servizi.Stampare {

	public enum EsitoStampa {
		Ok,
		Errore
	}


	public interface IEsecutoreStampa {

		bool asincrono {
			get;
			set;
		}

		EsitoStampa esegui( LavoroDiStampa lavoroDiStampa );

		Type tipoParamGestito {
			get;
		}
	}
}
