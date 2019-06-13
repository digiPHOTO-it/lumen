using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Services.Fingerprint;
using log4net;
using System;
using System.ServiceModel;
using System.Threading;

namespace Digiphoto.Lumen.FingerprintService.Host {

	class Program {

		protected new static readonly ILog _giornale = LogManager.GetLogger( typeof( Program ) );

		private static Mutex mutex = null;
		private static ServiceHost myServiceHost = null;

		static void Main( string[] args ) {

			Console.Out.WriteLine( "Step=1 : Avvio Host di : Fingerprint-Service. Machine Name = [" + System.Environment.MachineName + "]" );

			// Se il nome è diverso da LUMEN, segnalo un avviso
			string testsrv = "SERVER-LUMEN";
			if( System.Environment.MachineName.ToUpper() != testsrv )
				Console.Out.WriteLine( "Attenzione: per default questo servizio deve girare su di un server di nome: " + testsrv );


			// Per prima cosa, evito esecuzioni doppie
			if( controllaSeGiaInEsecuzione() ) {
				Console.Error.WriteLine( "ERRORE : L'Host di Fingerprint-Service è già in esecuzione. Uscita forzata!" );
				Thread.Sleep( 2000 );
				Environment.Exit( 1 );
			}

			try {

				// Avvio Lumen
				LumenApplication.Instance.avvia();

				//
				Console.Out.WriteLine( "Step=2 : Inizializzo l'host del servizio Fingerprint-Service" );
				myServiceHost = new ServiceHost( typeof( FingerprintServiceImpl ) );

				//
				Console.Out.WriteLine( "Step=3 : Avvio servizio Fingerprint-Service" );
				myServiceHost.Open();

				_giornale.Info( "Fingerprint Service avviato" );

				//
				string line;
				do {
					Console.Out.WriteLine( "Step=4 : Servizio in esecuzione. Digitare 'stop' + INVIO per terminare" );
					line = Console.In.ReadLine();
				} while( line.ToUpper() != "STOP" );

				Console.Out.WriteLine( "Richiesta l'uscita" );

			} catch( Exception ee ) {

				Console.Error.Write( ee.StackTrace );
				_giornale.Error( "Avvio", ee );

			} finally {

				// Fermo l'host
				if( myServiceHost != null ) {
					Console.Out.WriteLine( "Step=5 : Arresto Host di : Fingerprint-Service" );
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

			mutex = new Mutex( true, "Digiphoto.Lumen.FingerprintService.Host" );
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
