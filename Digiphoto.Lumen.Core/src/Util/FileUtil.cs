using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Digiphoto.Lumen.Util {


	public static class FileUtil {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( FileUtil ) );

		/// <summary>
		/// E' incredibile, ma la classe FileStream se usata in un loop,
		/// quindi nello stesso processo,
		/// si auto-locca anche se chiudi e fai la dispose regolarmente.
		/// Il trucco è quello di catturare l'errore e aspettare un attimo e riprovare.
		/// Probabilmente Windows che fa cacare lui stesso, a velocità elevate fa qualche casino.
		/// 
		/// Se dopo 10 pause da mezzo secondo non ci sono ancora riuscito, allora sollevo l'eccezione
		/// 
		/// Blocks until the file is not locked any more.
		/// </summary>
		/// <param name="fullPath">Occhio il file viene creato !!! azzerato !!! per poi scriverci</param>
		public static FileStream waitForFile( string fullPath ) {

			int numTries = 0;
			while( true ) {
				++numTries;
				try {
					// Attempt to open the file exclusively.
					FileStream fs = new FileStream( fullPath, FileMode.Create );
					fs.ReadByte();
					// If we got this far the file is ready
					fs.Seek( 0, SeekOrigin.Begin );
					return fs;
				} catch( Exception ex ) {
					_giornale.Warn( "WaitForFile " + fullPath + " failed to get an exclusive lock: " + ex.ToString() );

					if( numTries > 10 ) {
						_giornale.Error( "WaitForFile " + fullPath + " giving up after 10 tries" );
						throw ex;
					}

					// Wait for the lock to be released
					System.Threading.Thread.Sleep( 500 );
				}
			}
		}

	}
}
