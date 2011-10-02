using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.VolumeCambiato;

namespace Digiphoto.Lumen.Servizi {

	class ServizioFactory {

		public IServizio creaServizio( string nome ) {
			return this.creaServizio( Type.GetType( nome ) );
		}

		public IServizio creaServizio( Type tipo ) {

			// Istanzio
			LumenApplication app = LumenApplication.Instance;
			string nomeImpl = app.configurazione.nomiServizi [tipo.FullName];
			IServizio servizio = (IServizio)Activator.CreateInstance( Type.GetType(nomeImpl) );

			// Sottoscrivo questo servizio come asoltatore del bus di eventi
			LumenApplication.Instance.aggiungiAscoltatoreServizioBus( servizio );

			return servizio;
		}


	}


}
