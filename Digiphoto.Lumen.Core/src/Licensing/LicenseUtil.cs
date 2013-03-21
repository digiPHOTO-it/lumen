using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using Digiphoto.Lumen.Config;
using log4net;
using Microsoft.Win32;

namespace Digiphoto.Lumen.Licensing {

	public static class LicenseUtil {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( LicenseUtil ) );

		/// <summary>
		/// Calcola un numero da 5 cifre che rappresenta un hash
		/// di tutti i dati hardware del computer su cui sta girando.
		/// </summary>
		/// Questo metodo l'ho preso da qui:
		/// http://blog.clizware.net/all/977
		/// 
		/// <returns>
		/// Una stringa da 5 caratteri che per ora contiene solo numeri
		/// (in pratica sarebbe un long)
		/// </returns>

	
		public static string getMachineCode() {
			SKGL.SerialKeyConfiguration skc = new SKGL.SerialKeyConfiguration();
			return skc.MachineCode.ToString();
		}

		public static string getMachineCodeDACANC() {
			/* 
			 * Copyright (C) 2012 Artem Los, All rights reserved.
			 * 
			 * This code will generate a 5 digits long key, finger print, of the system
			 * where this method is being executed. However, that might be changed in the
			 * hash function "GetStableHash", by changing the amount of zeroes in
			 * MUST_BE_LESS_OR_EQUAL_TO to the one you want to have. Ex 1000 will return 
			 * 3 digits long hash.
			 * 
			 * Please note, that you might also adjust the order of these, but remember to
			 * keep them there because as it is stated at 
			 * (http://www.codeproject.com/Articles/17973/How-To-Get-Hardware-Information-CPU-ID-MainBoard-I)
			 * the processorID might be the same at some machines, which will generate same
			 * hashes for several machines.
			 * 
			 * The function will probably be implemented into SKGL Project at http://skgl.codeplex.com/
			 * and Software Protector at http://softwareprotector.codeplex.com/, so I 
			 * release this code under the same terms and conditions as stated here:
			 * http://skgl.codeplex.com/license
			 * 
			 * Any questions, please contact me at
			 *  * artem@artemlos.net
			 */
			_giornale.Debug( "gmc step1" );
			ManagementObjectSearcher searcher = new ManagementObjectSearcher( "select * from Win32_Processor" );
			string collectedInfo = ""; // here we will put the informa

			_giornale.Debug( "gmc step2" );
			foreach( ManagementObject share in searcher.Get() ) {
				// first of all, the processorid
				collectedInfo += share.GetPropertyValue( "ProcessorId" ).ToString();
			}

			_giornale.Debug( "gmc step3" );
			searcher.Query = new ObjectQuery( "select * from Win32_BIOS" );
			foreach( ManagementObject share in searcher.Get() ) {
				//then, the serial number of BIOS
				collectedInfo += share.GetPropertyValue( "SerialNumber" ).ToString();
			}

			_giornale.Debug( "gmc step4" );
			searcher.Query = new ObjectQuery( "select * from Win32_BaseBoard" );
			foreach( ManagementObject share in searcher.Get() ) {
				//finally, the serial number of motherboard
				collectedInfo += share.GetPropertyValue( "SerialNumber" ).ToString();
			}

			_giornale.Debug( "gmc step5" );
			return GetStableHash( collectedInfo ).ToString();
		}

		private static int GetStableHash( string s ) {
			/*
			 * modification of code from:
			 * http://stackoverflow.com/questions/548158/fixed-length-numeric-hash-code-from-variable-length-string-in-c-sharp
			 *
			 * modified by Artem Los
			 *
			 */
			const int MUST_BE_LESS_OR_EQUAL_TO = 100000;
			uint hash = 0;

			foreach( byte b in System.Text.Encoding.Unicode.GetBytes( s ) ) {
				hash += b;
				hash += (hash << 10);
				hash ^= (hash >> 6);
			}

			hash += (hash << 3);
			hash ^= (hash >> 11);
			hash += (hash << 15);

			int result = (int)(hash % MUST_BE_LESS_OR_EQUAL_TO);
			int check = MUST_BE_LESS_OR_EQUAL_TO / result;

			if( check > 1 ) {
				result *= check;
			}

			return result;
		}


		/// <summary>
		/// Esegue la validazione del codice licenza.
		/// Ritorna true se è valido.
		/// </summary>
		/// <param name="codiceLicenza"></param>
		/// <returns></returns>
		public static bool validaCodiceLicenza( string codiceLicenza ) {

			bool valida = false;

			if( codiceLicenza != null ) {

				try {
					RegistryLicense license = new RegistryLicense( codiceLicenza );

					valida = isValida( license );
 
				} catch( Exception ) {
				}
			}
			return valida;
		}

		private static string SUBKEY_REG = "Software\\digiPHOTO.it\\Lumen\\Registration";
		private static string VAL_KEY = "LicenseKey";


		/// <summary>
		/// Legge nel registry la chiave corrente, e istanzia la relativa classe.
		/// </summary>
		/// <returns></returns>
		public static RegistryLicense createCurrentLicense() {

			RegistryLicense license = null;
			try {
				license = new RegistryLicense( readCurrentLicenseKey() );
			} catch( Exception ) {
			}
			return license;
		}

		public static String readCurrentLicenseKey() {

			string strLic = null;

			RegistryKey licenseKey = Registry.LocalMachine.OpenSubKey( SUBKEY_REG );

			if( licenseKey != null ) {
				strLic = (string)licenseKey.GetValue( VAL_KEY );
			}

			return strLic;
		}

		public static void writeCurrentLicenseKey( string key ) {

			RegistryKey licenseKey = Registry.LocalMachine.OpenSubKey( SUBKEY_REG, true );

			if( licenseKey != null ) {

				if( String.IsNullOrWhiteSpace( key ) )
					licenseKey.DeleteValue( VAL_KEY, false );
				else
					licenseKey.SetValue( VAL_KEY, key );
			}
		}

		public static bool isValida( RegistryLicense license ) {
			return (license != null && license.IsExpired == false && license.IsOnRightMachine);
		}


		/// <summary>
		/// Read the serial number from the hard disk that keep the bootable partition (boot disk)
		/// </summary>
		/// <returns>
		/// If succedes, returns the string rappresenting the Serial Number.
		/// null if it fails.
		/// </returns>
		static string getHddSerialNumber() {

			// --- Win32 Disk Partition
			ManagementScope scope = new ManagementScope( @"\root\cimv2" );
			ObjectQuery query = new ObjectQuery( "select * from Win32_DiskPartition WHERE BootPartition=True" );
			ManagementObjectSearcher searcher = new ManagementObjectSearcher( scope, query );
			ManagementObjectCollection partitions = searcher.Get();

			uint diskIndex = 999;
			foreach( ManagementObject partition in partitions ) {

#if QQDEBUG
				Console.WriteLine( "****************************" );
				foreach( var qq in partition.Properties ) {
					Console.WriteLine( qq.Name + " = " + qq.Value );
				}
#endif
				diskIndex = (uint)partition ["Index"];
				break;
			}

			// I haven't found the bootable partition. Fail.
			if( diskIndex == 999 )
				return null;


			// --- Win32 Disk Drive
			searcher = new ManagementObjectSearcher( "SELECT * FROM Win32_DiskDrive where Index = " + diskIndex );

			string deviceName = null;
			foreach( ManagementObject wmi_HD in searcher.Get() ) {
#if QQDEBUG
				Console.WriteLine( "---------------------------------" );
				foreach( var qq in wmi_HD.Properties ) {
					Console.WriteLine( qq.Name + " = " + qq.Value );
				}
#endif

				deviceName = (string)wmi_HD ["Name"];
				break;

			}

			// I haven't found the disk drive. Fail
			if( String.IsNullOrWhiteSpace( deviceName ) )
				return null;

			if( deviceName.StartsWith( @"\\.\" ) ) {
				deviceName = deviceName.Replace( @"\\.\", "%" );
			}

			// --- Physical Media
			searcher = new ManagementObjectSearcher( "SELECT * FROM Win32_PhysicalMedia WHERE Tag like '" + deviceName + "'" );

			string serial = null;
			foreach( ManagementObject wmi_HD in searcher.Get() ) {
#if QQDEBUG
				Console.WriteLine( "!!!!!!!!!!!!!!!!!!!!!!!!!!!" );
				foreach( var qq in wmi_HD.Properties ) {
					Console.WriteLine( qq.Name + " = " + qq.Value );
				}
#endif
				serial = (string)wmi_HD ["SerialNumber"];
			}

			return serial;
		}
	}
}
