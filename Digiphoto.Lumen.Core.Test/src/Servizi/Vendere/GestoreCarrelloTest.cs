using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digiphoto.Lumen.Core.VsTest.src.Servizi.Vendere {

	[TestClass]
	public class GestoreCarrelloTest {

		GestoreCarrello gestoreCarrello;

		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void classInitialize( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}

		[ClassCleanup()]
		public static void classCleanup() {
			LumenApplication.Instance.ferma();
		}

		[TestInitialize()]
		public void MyTestInitialize() {

			this.gestoreCarrello = new GestoreCarrello();
		}
		[TestCleanup()]
		public void MyTestCleanup() {
			this.gestoreCarrello.Dispose();
			LumenApplication.Instance.ferma();
		}

		[TestMethod]
		public void TestMethod1() {

			this.gestoreCarrello.creaNuovo();
		}
	}
}
