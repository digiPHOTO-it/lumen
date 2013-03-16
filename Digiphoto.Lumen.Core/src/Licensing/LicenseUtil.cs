using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using Digiphoto.Lumen.Config;
using Microsoft.Win32;

namespace Digiphoto.Lumen.Licensing {

	public static class LicenseUtil {


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
			ManagementObjectSearcher searcher = new ManagementObjectSearcher( "select * from Win32_Processor" );
			string collectedInfo = ""; // here we will put the informa

			foreach( ManagementObject share in searcher.Get() ) {
				// first of all, the processorid
				collectedInfo += share.GetPropertyValue( "ProcessorId" ).ToString();
			}

			searcher.Query = new ObjectQuery( "select * from Win32_BIOS" );
			foreach( ManagementObject share in searcher.Get() ) {
				//then, the serial number of BIOS
				collectedInfo += share.GetPropertyValue( "SerialNumber" ).ToString();
			}

			searcher.Query = new ObjectQuery( "select * from Win32_BaseBoard" );
			foreach( ManagementObject share in searcher.Get() ) {
				//finally, the serial number of motherboard
				collectedInfo += share.GetPropertyValue( "SerialNumber" ).ToString();
			}
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
					licenseKey.DeleteValue( VAL_KEY );
				else
					licenseKey.SetValue( VAL_KEY, key );
			}
		}

		public static bool isValida( RegistryLicense license ) {
			return (license != null && license.IsExpired == false && license.IsOnRightMachine);
		}

	}
}
