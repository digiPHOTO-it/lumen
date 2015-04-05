using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.SessionState;

namespace Digiphoto.Lumen.SelfService.WebUI {
	
	internal static class Util {
		/// <summary>
		/// Calcola l'indirizzo base del servizio con barra finale
		/// </summary>
		/// <example>
		/// http://localhost:9000/
		/// </example>
		public static string baseAddress {
			get {
				String hostName;
				if( Properties.Settings.Default.NomeHost == "(this)" )
					hostName = System.Net.Dns.GetHostName();
				else
					hostName = Properties.Settings.Default.NomeHost;

				int numPorta = Properties.Settings.Default.NumPorta;
				if( numPorta == 0 )
					numPorta = 9000;

				return "http://" + hostName + ":" + numPorta + "/";
			}
		}


		/// <summary>
		/// Se la lingua è indicata, la setto,
		/// altrimenti la prendo dalla sessione.
		/// </summary>
		/// <param name="lingua"></param>
		internal static string ImpostaLingua( HttpSessionState session, string lingua ) {

			string linguaSelezionata = "it-IT";

			if( lingua != null ) {
				session["linguaSelezionata"] = lingua;
			}

			if( session["linguaSelezionata"] != null ) {
				linguaSelezionata = (string)session["linguaSelezionata"];
				try {
					Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture( linguaSelezionata );
					Thread.CurrentThread.CurrentUICulture = new CultureInfo( linguaSelezionata );
				} catch( Exception ) {
					linguaSelezionata = "it-IT";
				}
			}

			return linguaSelezionata;
		}


		internal static string ImpostaLingua( HttpSessionState session ) {
			return ImpostaLingua( session, null );
		}
	}
}