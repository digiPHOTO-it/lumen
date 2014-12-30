using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Core.VsTest.Util {

	public class Costanti {

		/// <summary>
		/// Nomi delle immagini jpg usate nei test.
		/// Durante i test mi servono delle immagini per "scaricarle", "stamparle", ecc.ecc.
		/// Le tengo in una cartella apposita e con questo metodo me le ricavo tutte.
		/// </summary>
		/// <returns></returns>
		public static string[] NomiFileImmagini {
			get {
				String doveSono = Assembly.GetExecutingAssembly().Location;
				string appPath = Path.GetDirectoryName( doveSono );
				string cartella = Path.Combine( appPath, "images" );
				string[] nomiFiles = Directory.GetFiles( cartella, "*.jpg" );
				return nomiFiles;
			}
		}

		private static Random rnd = new Random();

		/// <summary>
		/// Ricavo una delle 10 immaigni a caso
		/// </summary>
		public static string getNomeImmagineRandom() {
			return NomiFileImmagini[rnd.Next( 1, 10 )];
		}


		/// <summary>
		/// Nome della stampante utilizzata nei test
		/// </summary>
		public static string NomeStampante {
			get {
				return "Shinko CHC-S2145";
			}
		}

		public static string NomeStampantePdf {
			get {
				return "doPDF v7";
			}
		}

		public static Fotografia findUnaFotografiaRandom( LumenEntities context ) {

			int max = context.Fotografie.Max( f => f.numero );
			int min = context.Fotografie.Min( f => f.numero );
			Fotografia ret = null;
			do {
				int numProva = rnd.Next( min, max );
				ret = context.Fotografie.FirstOrDefault( f => f.numero == numProva );
			} while( ret == null );

			return ret;
		}


	}
}
