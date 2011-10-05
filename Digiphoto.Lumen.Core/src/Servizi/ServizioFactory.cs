using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using System.Runtime.Remoting;

namespace Digiphoto.Lumen.Servizi {

	class ServizioFactory {

		public IServizio creaServizio( string nome ) {
			return this.creaServizio( Type.GetType( nome ) );
		}

		public IServizio creaServizio( Type tipo ) {

			// Istanzio
			LumenApplication app = LumenApplication.Instance;
			
			/** Puo essere separato da virgola */
			string [] pezzi = app.configurazione.nomiServizi [tipo.FullName].Split( ',' );
			string nomeImpl = pezzi[0];
			string assemblyName = (pezzi.Length > 1 ? pezzi[1] : null);

			Object oo;
			if( assemblyName != null ) {
				ObjectHandle oh = (ObjectHandle) Activator.CreateInstance( AppDomain.CurrentDomain, assemblyName, nomeImpl );
				oo = oh.Unwrap();
			} else {
				oo = Activator.CreateInstance( Type.GetType( nomeImpl ) );
			}

			
			IServizio servizio = (IServizio)oo;
			// IServizio servizio = (IServizio)Activator.CreateInstance( assemblyName, nomeImpl );

			// Sottoscrivo questo servizio come asoltatore del bus di eventi
			LumenApplication.Instance.aggiungiAscoltatoreServizioBus( servizio );

			return servizio;
		}


	}


}
