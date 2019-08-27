using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Core.Util {

	public struct Spazio {

		public long libero {
			get;
			set;
		}

		public long liberoGB {
			get {
				return libero / (long)(Math.Pow( 2, 30 ));
			}
		}

		public short liberoPerc {
			get {
				return (short)(libero * 100L / totale);
			}
		}

		public short occupatoPerc {
			get {
				return (short)(100 - liberoPerc);
			}
		}


		public long totale {
			get;
			set;
		}

		public long totaleGB {
			get {
				return totale / (long)(Math.Pow( 2, 30 ));
			}
		}

	}

	public static class FileSystemUtil {
		
		/// <summary>
		/// Returns free space for drive containing the specified folder, or returns -1 on failure.
		/// </summary>
		/// <param name="folderName">A folder on the drive in question.</param>
		/// <returns>Space free on the volume containing 'folder_name' or -1 on error.</returns>

		public static Spazio FreeSpace( string folderName ) {

			if( string.IsNullOrEmpty( folderName ) ) {
				throw new ArgumentNullException( "folderName" );
			}

			if( !folderName.EndsWith( "\\" ) ) {
				folderName += '\\';
			}

			long free = 0, total = 0, dummy2 = 0;

			if( GetDiskFreeSpaceEx( folderName, ref free, ref total, ref dummy2 ) ) {
				return new Spazio {
					libero = free,
					totale = total
				};
			} else {
				throw new InvalidOperationException( "Impossibile determinare spazio disco" );
			}
		}

		[SuppressMessage( "Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage" ), SuppressUnmanagedCodeSecurity]
		[DllImport( "Kernel32", SetLastError = true, CharSet = CharSet.Auto )]
		[return: MarshalAs( UnmanagedType.Bool )]

		private static extern bool GetDiskFreeSpaceEx (
			string lpszPath,                    // Must name a folder, must end with '\'.
			ref long lpFreeBytesAvailable,
			ref long lpTotalNumberOfBytes,
			ref long lpTotalNumberOfFreeBytes
		);
	}

}
