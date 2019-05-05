using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.OnRide.SimulaScatti {

	class SimulaScatti {

		string dirSorgente;
		string dirDestinaz;
		int pausaMin;
		int pausaMax;
		const int QUADRETTI = 80;

		enum StatoRunning {
			Stop,
			Run,
			Quit
		}

		StatoRunning statoRunning {
			get;
			set;
		}

		void elaboraTastoPremuto() {

			if( Console.KeyAvailable ) {

				ConsoleKey tastoPremuto = Console.ReadKey( true ).Key;

				// Voglio uscire
				if( tastoPremuto == ConsoleKey.Q || tastoPremuto == ConsoleKey.Escape )
					statoRunning = StatoRunning.Quit;

				if( tastoPremuto == ConsoleKey.P )
					statoRunning = StatoRunning.Stop;

				if( tastoPremuto == ConsoleKey.Spacebar )
					statoRunning = StatoRunning.Run;

			}
		}

		public SimulaScatti() {
			statoRunning = StatoRunning.Stop;
		}

		void Run() {

			Console.WriteLine( "Dir sorgente = " + dirSorgente );
			Console.WriteLine( "Dir destinaz = " + dirDestinaz );
			Console.WriteLine( "Pausa = da : " + pausaMin / 1000 + " a " + pausaMax / 1000 + " secondi" );

			Console.WriteLine( "--- Press ESC to stop or P to Pause ---" );

			string[] files = Directory.GetFiles( dirSorgente, "*.jpg" );
			string rigaMsg;
			statoRunning = StatoRunning.Run;

			Random random = new Random();

			// Loop infinito principale
			do {
				elaboraTastoPremuto();

				if( statoRunning == StatoRunning.Run ) {

					// Loop per ogni file immagine jpeg presente nella cartella
					for( int ii = 0; (ii < files.Length) && (!Console.KeyAvailable); ii++ ) {


						// Per ogni file stabilisco una pausa random diversa, per simulare la situazione reale negli scivoli
						int pausa = random.Next( pausaMin, pausaMax );
						DateTime inizioAttesa = DateTime.Now;
						DateTime adesso;
						int secMancanti;
						string progressBar;


						// loop di 1 secondo per dare il movimento alla progress-bar
						do {
							adesso = DateTime.Now;
							secMancanti = inizioAttesa.AddMilliseconds( pausa ).Subtract( adesso ).Seconds;
							if( secMancanti >= 0 ) {

								// secMancanti : attesa = x : QUADRETTI
								// x = QUADRETTI * secMancanti / attesa
								int nCarMancanti = QUADRETTI * secMancanti / (pausa / 1000);
								int nCarPassati = (QUADRETTI - nCarMancanti);
								progressBar = (new string( '#', nCarMancanti )) + (new string( '_', nCarPassati ));
								rigaMsg = String.Format( "Attesa: {0:000}/{1:000} sec. {2}", secMancanti, pausa, progressBar );
								log( rigaMsg );

								Thread.Sleep( pausa / QUADRETTI );

								elaboraTastoPremuto();
							}
							
						} while( statoRunning == StatoRunning.Run && secMancanti > 0 );

						if( statoRunning == StatoRunning.Run ) {
							FileInfo f = new FileInfo( files[ii] );
							string nomeFileDest = Path.Combine( dirDestinaz, DateTime.Now.ToString( "yyyyMMdd_HHmmss" ) + ".jpg" );
							File.Copy( files[ii], nomeFileDest );

							log( "Copio il file: " + nomeFileDest );
						}

						elaboraTastoPremuto();
						
						if( statoRunning != StatoRunning.Run )
							break;
					}

				}

				if( statoRunning == StatoRunning.Stop ) {

					log( "<<< PAUSA >>> premere SPAZIO per riprendere" );
					Thread.Sleep( 2500 );
				}

			} while( statoRunning != StatoRunning.Quit );


		}

		string BIANCA = new String( ' ', 110 );
		void log( string msg ) {
			Console.Write( "\r" );
			Console.Write( BIANCA );
			Console.Write( "\r" );
			Console.Write( msg );
		}


		static void Main( string[] args ) {

			// Simulo degli scatti delle fotocamera, copiando delle immagini da una cartella ad un'altra a tempo
			SimulaScatti simula = new SimulaScatti();

			simula.dirSorgente = args[0];
			simula.dirDestinaz = args[1];
			
			simula.pausaMin = Int32.Parse( args[2] ) * 1000;
			simula.pausaMax = Int32.Parse( args[3] ) * 1000;

			simula.Run();

		}
	}
}
