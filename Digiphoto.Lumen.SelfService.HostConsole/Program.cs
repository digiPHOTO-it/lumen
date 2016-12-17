using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.SelfService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.SelfService.HostConsole {


	class Program {

		private static Mutex mutex = null;
		private static ServiceHost myServiceHost = null;

		static void Main( string[] args ) {

			Console.Out.WriteLine( "Step=1 : Avvio HOST self service" );

			// Per prima cosa, evito esecuzioni doppie
			if( controllaSeGiaInEsecuzione() ) {
				Console.Error.WriteLine( "ERRORE : L'host del servizio SelfService di Lumen è già in esecuzione. Uscita forzata!" );
				Thread.Sleep( 2000 );
				Environment.Exit( 1 );
			}

			try {

				// Avvio Lumen
				LumenApplication.Instance.avvia();

				//
				Console.Out.WriteLine( "Step=2 : Inizializzo l'host del servizio self service" );
				myServiceHost = new ServiceHost( typeof( SelfService ) );

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

			} catch( Exception ee ) {

				Console.Error.Write( ee.StackTrace );

			} finally {

				// Fermo l'host
				if( myServiceHost != null ) {
					Console.Out.WriteLine( "Step=5 : Arresto host del servizio self service" );
					myServiceHost.Close();
				}

				// Fermo Lumen
				Console.Out.WriteLine( "Step=6 : Arresto Lumen" );
				LumenApplication.Instance.ferma();

				Console.Out.WriteLine( "Step=7 : rilascio mutex" );
				rilascioMutex();
			}

			Console.Out.WriteLine( "Step=8 : Servizio concluso e terminato. Fine!" );
		}

		private static bool controllaSeGiaInEsecuzione() {

			bool giaInEsecuzione;

			mutex = new Mutex( true, "Digiphoto.Lumen.SelService.Host" );
			if( mutex.WaitOne( 0, false ) ) {
				giaInEsecuzione = false;
            } else {
				giaInEsecuzione = true;
            }

			return giaInEsecuzione;
        }

		private static void rilascioMutex() {

			try {
				if( mutex != null ) {
					mutex.ReleaseMutex();
					mutex.Dispose();
					mutex = null;
				}
			} catch( Exception ee ) {
				Console.Error.WriteLine( "Problema nel rilascio del mutex di lock applicazione: \r\n" + ee.Message );
			}

		}
	}
}
