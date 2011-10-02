using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using log4net;
using System.Drawing.Imaging;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.Util {

	public class ProviniUtil {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( ProviniUtil) );

		/** Nel parametro si può passare sia il nome del file della foto, oppure il nome 
		 * della cartella che contiene le foto
		 */
		public static string decidiCartellaProvini( FileInfo nomeFile ) {
			return Path.Combine( nomeFile.DirectoryName, ".Thumb" );
		}

		public static string decidiCartellaProvini( Fotografia foto ) {
			return Path.Combine( LumenApplication.Instance.configurazione.getCartellaBaseFoto(), foto.nomeFile, ".Thumb" ); 
		}
		
		public static string nomeCompletoFoto( Fotografia foto ) {
			return Path.Combine( LumenApplication.Instance.configurazione.getCartellaBaseFoto(), foto.nomeFile ); 
		}

		public static string creaCartellaProvini( FileInfo cartellaFoto ) {

			string nomeCartellaProvini = decidiCartellaProvini( cartellaFoto );

			if( Directory.Exists( nomeCartellaProvini ) == false )
				Directory.CreateDirectory( nomeCartellaProvini );

			return nomeCartellaProvini;
		}



	}
}
