using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.SelfService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.SelfService.HostConsole {


	class Program {

		static void Main( string[] args ) {

			// Avvio Lumen
			Console.Out.WriteLine( "Step=1 : Avvio Lumen" );
			LumenApplication.Instance.avvia();

			//
			Console.Out.WriteLine( "Step=2 : Inizializzo l'host del servizio self service" );
			var myServiceHost = new ServiceHost( typeof( SelfService ) );

			//
			Console.Out.WriteLine( "Step=3 : Avvio servizio self service" );
			myServiceHost.Open();

			//
			string line;
			do {
				Console.Out.WriteLine( "Step=4 : Servizio in esecuzione. Digitare 'stop' + INVIO per terminare" );
				line = Console.In.ReadLine();
			} while( line.ToUpper() != "STOP" );

			Console.Out.WriteLine( "Richiesta l'uscita" );

			// Fermo Lumen
			Console.Out.WriteLine( "Step=5 : Arresto Lumen" );
			LumenApplication.Instance.avvia();

			//
			Console.Out.WriteLine( "Step=6 : Arresto host del servizio self service" );
			myServiceHost.Close();
			
			Console.Out.WriteLine( "Step=6 : Servizio concluso e terminato. Fine!" );
		}
	}
}
