using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Config;
using Microsoft.Win32;

namespace Digiphoto.Lumen.Licensing {

	public class RegistryLicenseProvider : LicenseProvider {

		public RegistryLicenseProvider() {
		}

		public override License GetLicense( LicenseContext context, Type type, object instance, bool allowExceptions ) {

			// We'll test for the usage mode...run time v. design time. Note
			// we only check if run time...
			if (context.UsageMode == LicenseUsageMode.Runtime) {
				
				string strLic = LicenseUtil.getCurrentLicenseKey();

				if( strLic != null ) {
					// Trovato il codice di licenza. Ora provo a creare/generare una License vera e propria
					RegistryLicense license = new RegistryLicense( strLic );
					return license;
				}

				// If we got this far, we failed the license test. We then
				// check to see if exceptions are allowed, and if so, throw
				// a new license exception...
				if ( allowExceptions == true )
					throw new LicenseException(type, instance, "Your license is invalid");

				// Exceptions are not desired, so we'll simply return null.
				return null;
			} else {
				// return new DesigntimeRegistryLicense( type );
				return null;
			}
		}
	}

}
