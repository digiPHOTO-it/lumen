using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using log4net;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Applicazione;
using System.Security.AccessControl;

namespace Digiphoto.Lumen.Util {

	/**
	 * Questa classe statica, contiene dei metodi di utilita per poter calcolare i vari
	 * percorsi (path) delle foto e dei provini.
	 */
	public class PathUtil {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( PathUtil) );
		private static readonly string THUMB = ".Thumb";

		/** Nel parametro si può passare sia il nome del file della foto, oppure il nome 
		 * della cartella che contiene le foto
		 */
		public static string decidiCartellaProvini( FileInfo fileInfo ) {
			if( Directory.Exists( fileInfo.FullName ) )
				return Path.Combine( fileInfo.FullName, THUMB );   // è una cartella
			else
				return Path.Combine( fileInfo.DirectoryName, THUMB );   // è un file probabilmente quello della foto.
		}

		public static string decidiCartellaProvini( Fotografia foto ) {
			return decidiCartellaProvini( fileInfoFoto(foto) );
		}

		public static FileInfo fileInfoFoto( Fotografia foto ) {
			return new FileInfo( nomeCompletoFoto( foto ) );
		}

		/** Dato in input una foto che contiene al suo interno un nome file relativo,
		 * appiccico anche la cartella di base del repository delle foto per tornare il nome completo.
		 */
		public static string nomeCompletoFoto( Fotografia foto ) {
			return Path.Combine( LumenApplication.Instance.configurazione.getCartellaRepositoryFoto(), foto.nomeFile ); 
		}

		/** 
		 * Data una foto (e quindi il suo percorso relativo), decido il nome completo da dare al provino.
		 * Mi servirà per poter salvare su disco l'immagine rimpicciolita
		 */
		public static string nomeCompletoProvino( Fotografia foto ) {
			FileInfo fotoInfo = fileInfoFoto( foto );
			return Path.Combine( decidiCartellaProvini( fotoInfo ), fotoInfo.Name );
		}

		public static string nomeRelativoFoto( FileInfo pathAssoluto ) {
			int iniz = LumenApplication.Instance.configurazione.getCartellaRepositoryFoto().Length;

			// scarto la parte iniziale di tutto il path togliendo il nome della cartella di base delle foto.
			return pathAssoluto.FullName.Substring( iniz + 1 );
		}

		public static string creaCartellaProvini( FileInfo cartellaFoto ) {

			string nomeCartellaProvini = decidiCartellaProvini( cartellaFoto );

			if( Directory.Exists( nomeCartellaProvini ) == false )
				Directory.CreateDirectory( nomeCartellaProvini );

			return nomeCartellaProvini;
		}

        public static string fotografoIDFromPath(String path)
        {
            String fotografoID = null;
            if (Directory.Exists(path) || true)
            {
                String[] array = path.Split(Path.DirectorySeparatorChar);
                foreach (String ar in array)
                {
                    if (ar.Contains(".Fot"))
                    {
                        fotografoID = ar.Split('.')[0];
                        break;
                    }
                }
            }
            return fotografoID;
        }

        public static string giornoFromPath(String path)
        {
            String giorno = null;
            if (Directory.Exists(path) || true)
            {
                String[] array = path.Split(Path.DirectorySeparatorChar);
                foreach (String ar in array)
                {
                    if(ar.Contains(".Gio")){
                        giorno = ar.Split('.')[0];
                        break;
                    }
                }
            }
            return giorno;
        }

		/// <summary>
		/// Mi dice se posso scrivere nella cartella indicata
		/// </summary>
		/// <param name="directoryPath"></param>
		/// <returns>true se posso scrivere.</returns>
		public static bool isCartellaScrivibile( string directoryPath ) {

			if( Directory.Exists( directoryPath ) == false )
				throw new ArgumentException( "cartella " + directoryPath + " inesistente" );

           bool isWriteAccess = false;

			try {
              AuthorizationRuleCollection collection = Directory.GetAccessControl(directoryPath).GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
              foreach (FileSystemAccessRule rule in collection) {
                 if (rule.AccessControlType == AccessControlType.Allow) {
                    isWriteAccess = true;
                    break;
                 }
              }
           } catch (Exception ex) {
              isWriteAccess = false;
           }

			return isWriteAccess;
		}
	}
}
