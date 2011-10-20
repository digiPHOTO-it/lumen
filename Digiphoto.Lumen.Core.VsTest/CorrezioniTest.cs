using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Imaging.Ritocco;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Collections;
using Digiphoto.Lumen.Util;


namespace Digiphoto.Lumen.Core.VsTest {


	[TestClass]
	public class CorrezioniTest {

		[TestMethod]
		public void TestSerializzaArray() {

			Luminosita c1 = new Luminosita( 12.345f );
			Ruota c2 = new Ruota( 45 );
			Crop c3 = new Crop( 2000, 3000, 10, 20, 1100, 2200 );
			Specchio c4 = new Specchio();
			c4.direzione = 'Q';

			Correzione [] correzioni = new Correzione [] { c1, c2, c3, c4 };
			string xml2 = SerializzaUtil.objectToString( correzioni );

			Correzione [] correz3 = SerializzaUtil.stringToObject<Correzione []>( xml2 );
			Assert.IsTrue( correzioni.Length == correz3.Length );
		}
	}
}
