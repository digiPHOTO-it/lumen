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

			// calcolo il nome completo della interfaccia che mi fa da chiave per la mappa.
			string key = calcFullName( tipo );

			if( app.configurazione.nomiServizi.Keys.Contains( key ) == false )
				throw new NotSupportedException( "servizio " + key + " non definito nella configurazione" );

			/** Puo essere separato da virgola */
			string [] pezzi = app.configurazione.nomiServizi [key].Split( ',' );
			string nomeImpl = pezzi[0];
			string assemblyName = (pezzi.Length > 1 ? pezzi[1] : null);

			Object oo;
			try {
				if( assemblyName != null ) {
					ObjectHandle oh = (ObjectHandle)Activator.CreateInstance( AppDomain.CurrentDomain, assemblyName, nomeImpl );
					oo = oh.Unwrap();
				} else {
					oo = Activator.CreateInstance( Type.GetType( nomeImpl ) );
				}
			} catch( Exception ee ) {
				System.Console.Out.WriteLine( "Impossibile creare impl del servizio " + nomeImpl );			
				throw ee;
			}
			
			IServizio servizio = (IServizio)oo;
			// IServizio servizio = (IServizio)Activator.CreateInstance( assemblyName, nomeImpl );

			// Sottoscrivo questo servizio come asoltatore del bus di eventi
			LumenApplication.Instance.aggiungiAscoltatoreServizioBus( servizio );

			return servizio;
		}


		internal static string calcFullName( Type tipo ) {

			// Se il tipo indicato prevede un generic, converto il nome con le parentesi angolari
			Type [] generici = tipo.GetGenericArguments();

			string mioFullName;
			if( generici == null || generici.Length == 0 )
				mioFullName = tipo.FullName;
			else {
				string nomeSenzaNumero = tipo.Name.Substring( 0, tipo.Name.IndexOf( "`" ) );
				mioFullName = tipo.Namespace + "." + nomeSenzaNumero + "<" + generici [0].FullName + ">";
			}

			return mioFullName;
		}
	}


}
