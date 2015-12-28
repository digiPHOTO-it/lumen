using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digiphoto.Lumen.Core.Test.Licenza {

	[TestClass]


	public class LicenzaTest {


		[TestMethod]
		public void TestLicenza1() {

			SKGL.Validate ValidateAKey = new SKGL.Validate();// create an object
			ValidateAKey.secretPhase = "My$ecretPa$$W0rd"; // the passsword

			// Questa licenza dovrebbe essere valida fino al 20/11/2015
			ValidateAKey.Key = "LBWRB-QOITK-SLCCU-MCGKS"; // enter a valid key
			Assert.IsFalse( ValidateAKey.IsExpired );
			Assert.IsTrue( ValidateAKey.IsValid );

			// Questa chiave non è valida (cioè è sbagliata)
			ValidateAKey.Key = "yFBFR-SZNVH-DGVXL-RTCLT";
			Assert.IsFalse( ValidateAKey.IsValid );

			// Questa chiave è corretta ma è scaduta
			ValidateAKey.Key = "LREYZ-AAIDV-RLGRK-EVPFQ";
			Assert.IsTrue( ValidateAKey.IsExpired );
			Assert.IsTrue( ValidateAKey.IsExpired );

			
		}

	}
}
