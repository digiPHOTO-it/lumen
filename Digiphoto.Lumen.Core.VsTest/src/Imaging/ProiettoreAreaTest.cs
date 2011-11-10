using Digiphoto.Lumen.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;

namespace Digiphoto.Lumen.Core.VsTest
{
    
    
    /// <summary>
    ///This is a test class for ProiettoreAreaTest and is intended
    ///to contain all ProiettoreAreaTest Unit Tests
    ///</summary>
	[TestClass()]
	public class ProiettoreAreaTest {


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


		/**
		 *      Foto  = Verticale 
		 *      Carta = Verticale
		 *      :: misure identiche
		 */
		[TestMethod()]
		public void calcolaOriOriEqVertTest() {

			Rectangle stampante = new Rectangle( 0, 0, 117, 139 );
			Rectangle foto = new Rectangle( 0, 0, 117, 139 );  // identico

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = false;

			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 *      Foto  = Orizzontale
		 *      Carta = Orizzontale
		 *      :: misure identiche
		 */
		[TestMethod()]
		public void calcolaOriOriEqOrizTest() {

			Rectangle stampante = new Rectangle( 0, 0, 139, 117 );
			Rectangle foto = new Rectangle( 0, 0, 139, 117  );  // identico

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = false;

			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 *   Foto  = Orizzontale
		 *   Carta = Verticale
		 *   :: misure identiche
		 *   :: senza rotazione (deve tagliare)
		 */
		[TestMethod()]
		public void calcolaOrizVertIdentiche() {

			Rectangle stampante = new Rectangle( 0, 0, 150, 200 );
			Rectangle foto = new Rectangle( 0, 0, 200, 150 );  // identico ma nell'altro verso

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = false;

			Proiezione esito = proiettore.calcola( foto );

			Rectangle risultatoAtteso = new Rectangle( 0, 0, 150, 112 );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, risultatoAtteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 *   Foto  = Orizzontale
		 *   Carta = Verticale
		 *   :: misure identiche
		 *   :: Con rotazione non deve tagliare
		 */
		[TestMethod()]
		public void calcolaOrizVertIdenticheRuota() {

			Rectangle stampante = new Rectangle( 0, 0, 150, 200 );
			Rectangle foto = new Rectangle( 0, 0, 200, 150 );  // identico ma nell'altro verso

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;
			proiettore.autoZoomToFit = false;

			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsTrue( esito.effettuataRotazione );
		}

		/**
		 *   Foto  = Orizzontale
		 *   Carta = Verticale
		 *   :: misure identiche
		 *   :: Con rotazione non deve tagliare
		 */
		[TestMethod()]
		public void calcolaOrizVertIdenticheRuota2() {

			Rectangle stampante = new Rectangle( 0, 0, 200, 150 );
			Rectangle foto = new Rectangle( 0, 0, 150, 200 );  // identico ma nell'altro verso

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;
		
			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsTrue( esito.effettuataRotazione );
		}


	}
}
