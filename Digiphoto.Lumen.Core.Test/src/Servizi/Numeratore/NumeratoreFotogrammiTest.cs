using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.Core.Test.Servizi.Numeratore {


	/// <summary>
	///This is a test class for NumeratoreFotogrammiTest and is intended
	///to contain all NumeratoreFotogrammiTest Unit Tests
	///</summary>
	[TestClass()]
	public class NumeratoreFotogrammiTest {


		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext {
			get {
				return testContextInstance;
			}
			set {
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion
		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void inizializzaClasse( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}
		
		[TestInitialize]
		public void Init() {
		}


		/// <summary>
		///A test for incrementaNumeratoreFoto
		///</summary>
		[TestMethod()]
		public void incrementaNumeratoreFotoTest() {

			DateTime dataUltimoScarico;
			const int gg = 123;
			int newNum;
			
			// ---
			LumenApplication.Instance.stato.giornataLavorativa = new DateTime( 2011, 10, 31 );
			dataUltimoScarico = new DateTime( 2011, 10, 20 );
			newNum = NumeratoreFotogrammi.eventualeAzzeramento( "G", gg, dataUltimoScarico );
			Assert.IsTrue( newNum == 0 );

			// ---
			dataUltimoScarico = new DateTime( 2011, 10, 31 );
			newNum = NumeratoreFotogrammi.eventualeAzzeramento( "G", gg, dataUltimoScarico );
			Assert.IsTrue( newNum == gg );

			// ---
			dataUltimoScarico = new DateTime( 2011, 10, 30 );
			newNum = NumeratoreFotogrammi.eventualeAzzeramento( "G", gg, dataUltimoScarico );
			Assert.IsTrue( newNum == 0 );

			// questa è una anomalia. non so ancora come gestirla.
			dataUltimoScarico = new DateTime( 2011, 11, 02 );
			newNum = NumeratoreFotogrammi.eventualeAzzeramento( "G", gg, dataUltimoScarico );
			Assert.IsTrue( newNum == gg );


			// ---
			dataUltimoScarico = new DateTime( 2000, 10, 31 );
			newNum = NumeratoreFotogrammi.eventualeAzzeramento( "M", gg, dataUltimoScarico );
			Assert.IsTrue( newNum == 0 );

			dataUltimoScarico = DateTime.Today.AddDays( -1 ); // ieri
			newNum = NumeratoreFotogrammi.eventualeAzzeramento( "M", gg, dataUltimoScarico );
			Assert.IsTrue( newNum == gg );

			// ---
			try {
				newNum = NumeratoreFotogrammi.eventualeAzzeramento( "x", gg, dataUltimoScarico );
				Assert.Fail();
			} catch( Exception ) {
				// ok se ha dato eccezione va bene.
			}

		}



	}
}
