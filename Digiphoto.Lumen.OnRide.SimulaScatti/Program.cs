using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.OnRide.SimulaScatti {
	class Program {


		static void Main( string[] args ) {

			// Simulo degli scatti delle fotocamera, copiando delle immagini da una cartella ad un'altra a tempo

			string dirSorgente = args[0];
			string dirDestinaz = args[1];
			int pausa = Int32.Parse( args[2] ) * 1000;

			Console.WriteLine( "Dir sorgente = " + dirSorgente );
			Console.WriteLine( "Dir destinaz = " + dirDestinaz );
			Console.WriteLine( "Pausa = " + pausa );

			Console.WriteLine( "--- Press ESC to stop ---" );

			string [] files = Directory.GetFiles( dirSorgente, "*.jpg" );
			
			do {
			
				for( int ii = 0; (ii < files.Length) && (! Console.KeyAvailable); ii++ ) {

					FileInfo f = new FileInfo( files[ii] );
					string nomeFileDest = Path.Combine( dirDestinaz, DateTime.Now.ToString( "yyyyMMdd_HHmmss" ) + ".jpg" );
					File.Copy( files[ii], nomeFileDest );

					Console.Out.WriteLine( "copio il file: " + nomeFileDest );

					Thread.Sleep( pausa );
				}

			} while( Console.ReadKey( true ).Key != ConsoleKey.Escape );


		}
	}
}
