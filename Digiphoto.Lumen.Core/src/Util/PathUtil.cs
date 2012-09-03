using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Applicazione;
using System.Security.AccessControl;
using Digiphoto.Lumen.Config;
using System.Windows.Forms;

namespace Digiphoto.Lumen.Util {

	/**
	 * Questa classe statica, contiene dei metodi di utilita per poter calcolare i vari
	 * percorsi (path) delle foto e dei provini.
	 */
	public class PathUtil {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( PathUtil) );
		private static readonly string THUMB = ".Thumb";
		private static readonly string MODIF = ".Modif";

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

		/** Nel parametro si può passare sia il nome del file della foto, oppure il nome 
		 * della cartella che contiene le foto
		 */
		public static string decidiCartellaRisultanti( FileInfo fileInfo ) {
			if( Directory.Exists( fileInfo.FullName ) )
				return Path.Combine( fileInfo.FullName, MODIF );   // è una cartella
			else
				return Path.Combine( fileInfo.DirectoryName, MODIF );   // è un file probabilmente quello della foto.
		}


		public static FileInfo fileInfoFoto( Fotografia foto ) {
			return new FileInfo( nomeCompletoFoto( foto ) );
		}

		public static string nomeCompletoFile( Fotografia foto, IdrataTarget quale ) {
			if( quale == IdrataTarget.Originale )
				return nomeCompletoFoto( foto );
			if( quale == IdrataTarget.Provino )
				return nomeCompletoProvino( foto );
			if( quale == IdrataTarget.Risultante )
				return nomeCompletoRisultante( foto );
			throw new ArgumentException( quale.ToString() );
		}

		/// <summary>
		/// Questo metodo è un doppione. Usare nomeCompletoOrig
		/// </summary>
		public static string nomeCompletoFoto( Fotografia foto ) {
			return nomeCompletoOrig( foto );
		}

		/** 
		 * Data una foto (e quindi il suo percorso relativo), decido il nome completo da dare al provino.
		 * Mi servirà per poter salvare su disco l'immagine rimpicciolita
		 */
		public static string nomeCompletoProvino( Fotografia foto ) {
			FileInfo fotoInfo = fileInfoFoto( foto );
			return Path.Combine( decidiCartellaProvini( fotoInfo ), fotoInfo.Name );
		}

		/** 
		 * Data una foto (e quindi il suo percorso relativo), decido il nome completo da dare al provino.
		 * Mi servirà per poter salvare su disco l'immagine rimpicciolita
		 */
		public static string nomeCompletoRisultante( Fotografia foto ) {

			FileInfo fotoInfo = fileInfoFoto( foto );
			return Path.Combine( decidiCartellaRisultanti( fotoInfo ), fotoInfo.Name );
		}


		/// <summary>
		/// Calcolo il giusto percorso delle foto originali.
		/// Combino quindo il path ed il nome del file della foto ORIGINALE.
		/// </summary>
		/// <param name="foto">la fotografia in questione</param>
		/// <returns>il percorso completo del file che rappresenta la foto ORIGINALE</returns>
		public static string nomeCompletoOrig( Fotografia foto ) {
			return Path.Combine( Configurazione.cartellaRepositoryFoto, foto.nomeFile );
		}



		public static string nomeRelativoFoto( FileInfo pathAssoluto ) {
			int iniz = Configurazione.cartellaRepositoryFoto.Length;

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
           } catch (Exception) {
              isWriteAccess = false;
           }

			return isWriteAccess;
		}

		internal static void creaCartellaProvini( Fotografia foto ) {

			// Ricavo il nome della foto originale per determinare la cartella
			string nomeFile = PathUtil.nomeCompletoOrig( foto );
			string nomeCartella = PathUtil.decidiCartellaProvini( new FileInfo( nomeFile ) );
			if( !Directory.Exists( nomeCartella ) ) {
				Directory.CreateDirectory( nomeCartella );
			}
		}

		internal static void creaCartellaRisultanti( Fotografia foto ) {

			// Ricavo il nome della foto originale per determinare la cartella
			string nomeFile = PathUtil.nomeCompletoOrig( foto );
			string nomeCartella = PathUtil.decidiCartellaRisultanti( new FileInfo( nomeFile ) );
			if( !Directory.Exists( nomeCartella ) ) {
				Directory.CreateDirectory( nomeCartella );
			}
		}


		/// <summary>
		/// Mi torna il nome di un file temporaneo ma con l'estensione che voglio io.
		/// </summary>
		/// <param name="estensione">Può essere sia con il punto che senza.</param>
		/// <returns>nome completo di un file temporaneo che sicuramente non esiste</returns>
		public static string dammiTempFileConEstesione( string estensione ) {

			string nomeTemp;

			do {
				nomeTemp = Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString() );
				if( estensione.StartsWith( "." ) == false )
					nomeTemp += ".";
				nomeTemp += estensione;

			} while( File.Exists( nomeTemp ) );

			return nomeTemp.ToString();
		}

		public static string createTempDirectory() {
			string path;
			do {
				path = Path.Combine( Path.GetTempPath(), Path.GetRandomFileName() ); 
			} while( Directory.Exists(path) );

			Directory.CreateDirectory(path);
			return path; 
		}

		public static string scegliCartella() {
			string cartella = null;

			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.ShowNewFolderButton = true;
			DialogResult result = dlg.ShowDialog();
			if( result == System.Windows.Forms.DialogResult.OK )
				cartella = dlg.SelectedPath;

			return cartella;
		}


		// Adds an ACL entry on the specified file for the specified account.
		public static void AddFileSecurity( string fileName, string account, FileSystemRights rights, AccessControlType controlType ) {

			// Get a FileSecurity object that represents the
			// current security settings.
			FileSecurity fSecurity = File.GetAccessControl( fileName );

			// Add the FileSystemAccessRule to the security settings.
			fSecurity.AddAccessRule( new FileSystemAccessRule( account, rights, controlType ) );

			// Set the new access settings.
			File.SetAccessControl( fileName, fSecurity );
		}

		// Adds an ACL entry on the specified file for the specified account.
		public static void AddFileSecurity( string fileName, FileSystemRights rights, AccessControlType controlType ) {
			AddFileSecurity( fileName, Environment.UserName, rights, controlType );
		}
	}
}
