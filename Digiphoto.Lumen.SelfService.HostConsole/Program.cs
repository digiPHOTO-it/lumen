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

			//
			Console.Out.WriteLine( "Step=1 : Inizializzo l'host del servizio" );
			var myServiceHost = new ServiceHost( typeof( SelfService ) );

			//
			Console.Out.WriteLine( "Step=2 : Avvio servizio" );
			myServiceHost.Open();

			//
			Console.Out.WriteLine( "Step=3 : Servizio in esecuzione. Premere INVIO per terminare" );
			Console.In.ReadLine();

			//
			Console.Out.WriteLine( "Step=4 : Richiesta l'uscita" );
			myServiceHost.Close();
			
			Console.Out.WriteLine( "Step=5 : Servizio concluso e terminato. Uscita" );
		}
	}
}
