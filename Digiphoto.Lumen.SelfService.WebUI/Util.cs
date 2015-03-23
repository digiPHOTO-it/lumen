using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Digiphoto.Lumen.SelfService.WebUI {
	
	public static class Util {
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
	}
}