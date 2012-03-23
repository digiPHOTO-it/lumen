using Digiphoto.Lumen.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;

namespace Digiphoto.Lumen.Core.VsTest
{


	/*
	 * Per definire tutti i casi strani di proiezione,
	 * ho creato un foglio di calcolo che è allegato a questo progetto 
	 * nell cartella doc.
	 */
	[TestClass()]
	public class ProiettoreAreaTest {

		// Per le prove uso due misure di larghezza ed altezza che sono fissi
		// Uso due numeri primi perché dividendoli per calcolare il ratio,
		// si possono innescare problemi di arrotondamento/troncamento.
		// Nei test occorre ricordare che L2 è più grande di L1.
		private static readonly int L1 = 3613;
		private static readonly int L2 = 4271;
		private static readonly int DELTA = 733;

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
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: verticale
		 * Ratio:  f<s
		 * autoZoom: false
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest01() {

			Int32Rect foto = new Int32Rect( 0, 0, 5, 12);
			Int32Rect stampante = new Int32Rect( 0, 0, 50, 100 );

			Assert.IsTrue( ratio( foto ) < ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );
			int neww = (int)(100 * (5f / 12f));
			Assert.IsTrue( neww == 41 );
			Int32Rect atteso = new Int32Rect( 4,0, neww, 100 );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: verticale
		 * Ratio:  f=s
		 * autoZoom: false
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest02() {

			Int32Rect foto = creaVert();
			Int32Rect stampante = creaVert();

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}



		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: verticale
		 * Ratio:  f>s
		 * autoZoom: false
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest03() {

			Int32Rect foto = new Int32Rect( 0, 0, 50, 100 );
			Int32Rect stampante = new Int32Rect( 0, 0, 5, 12 );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );
			Int32Rect atteso = new Int32Rect( 0, 1, 5, 10 );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );

			// Altra prova identica deve dare lo stesso risultato
			Int32Rect foto2 = new Int32Rect( 0, 0, 2, 4 );
			Proiezione esito2 = proiettore.calcola( foto2 );
			Assert.AreEqual( esito.dest, atteso );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: orizzontale
		 * Ratio:  f<s   (ovviamente in senso assoluto [ABS] perchè una è maggiore di 1 l'altra no)
		 * autoZoom: false
		 * autoRotate: false 
		 */
		[TestMethod()]
		public void proiettaTest04() {

			Int32Rect foto = new Int32Rect( 0, 0, 47, 67 );
			Int32Rect stampante = new Int32Rect( 0, 0, 311, 266 );
			
			Assert.IsTrue( ratio(foto) < (1/ratio(stampante)));

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 62, 0, 186, 266 );
			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}


		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: orizzontale
		 * Ratio:  f=s
		 * autoZoom: false
		 * autoRotate: false 
		 */
		[TestMethod()]
		public void proiettaTest05() {

			Int32Rect foto = creaVert();
			Int32Rect stampante = creaOriz();

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 607, 0, 3056, L1 );
			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: orizzontale
		 * Ratio:  f>s   (ovviamente in senso relativo perchè una è maggiore di 1 l'altra no)
		 * autoZoom: false
		 * autoRotate: false 
		 */
		[TestMethod()]
		public void proiettaTest06() {

			Int32Rect foto = new Int32Rect( 0, 0, 47, 67 );
			Int32Rect stampante = new Int32Rect( 0, 0, 311, 187 );

			Assert.IsTrue( ratio( foto ) > (1 / ratio( stampante )) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 90, 0, 131, 187 );
			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Orizzontale
		 * Stampante: verticale
		 * Ratio:  f<s
		 * autoZoom: false
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest07() {

			Int32Rect foto = new Int32Rect( 0, 0, 13, 5 );
			Int32Rect stampante = new Int32Rect( 0, 0, 50, 149 );

			Assert.IsTrue( ratio( foto ) < 1/ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );
			int newh = (int)(50 / (13f / 5f));
			Assert.IsTrue( newh == 19 );
			Int32Rect atteso = new Int32Rect( 0, 65, 50, newh );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: verticale
		 * Ratio:  f=s
		 * autoZoom: false
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest08() {

			Int32Rect stampante = creaVert();
			Int32Rect foto = creaOriz();

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = false;
			proiettore.autoRotate = false;

			Proiezione esito = proiettore.calcola( foto );

			Int32Rect risultatoAtteso = new Int32Rect( 0, 607, L1, 3056 );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, risultatoAtteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Orizzontale
		 * Stampante: verticale
		 * Ratio:  f>s
		 * autoZoom: false
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest09() {

			Int32Rect foto = new Int32Rect( 0, 0, 177, 111 );
			Int32Rect stampante = new Int32Rect( 0, 0, 189, 200 );

			Assert.IsTrue( ratio( foto ) > 1 / ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );
			int newh = (int)(189f / (177f / 111f));
			Assert.IsTrue( newh == 118 );
			Int32Rect atteso = new Int32Rect( 0, 41, 189, newh );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: orizzontale
		 * Ratio:  f<s
		 * autoZoom: false
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest10() {

			Int32Rect stampante = new Int32Rect( 0, 0, 901, 607 );
			Int32Rect foto = new Int32Rect( 0,0,407,399);

			Assert.IsTrue( ratio( foto ) < ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 141, 0, 619, 607 );  // Rimane uguale l'altezza. Cambia la larghezza

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: orizzontale
		 * Ratio:  f=s
		 * autoZoom: false
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest11() {

			Int32Rect stampante = creaOriz();
			Int32Rect foto = creaOriz();

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = false;

			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: orizzontale
		 * Ratio:  f>s
		 * autoZoom: false
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest12() {

			Int32Rect stampante = new Int32Rect( 0, 0, 193, 155 );
			Int32Rect foto = new Int32Rect( 0, 0, 12355, 8997 );

			Assert.IsTrue( ratio( foto ) > ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 0, 7, 193, 140 );  // Rimane uguale la larghezza. Cambia l'altezza

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: verticale
		 * Ratio:  f<s
		 * autoZoom: false
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest13() {

			Int32Rect foto = new Int32Rect( 0, 0, 5, 12 );
			Int32Rect stampante = new Int32Rect( 0, 0, 50, 100 );

			Assert.IsTrue( ratio( foto ) < ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );
			
			int neww = (int)(100 * (5f / 12f));
			Assert.IsTrue( neww == 41 );
			Int32Rect atteso = new Int32Rect( 4, 0, neww, 100 );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}


		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: verticale
		 * Ratio:  f=s
		 * autoZoom: false
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest14() {

			Int32Rect foto = creaVert();
			Int32Rect stampante = creaVert();

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = false;
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: verticale
		 * Ratio:  f>s
		 * autoZoom: false
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest15() {

			Int32Rect foto = new Int32Rect( 0, 0, 50, 100 );
			Int32Rect stampante = new Int32Rect( 0, 0, 5, 12 );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );
			Int32Rect atteso = new Int32Rect( 0, 1, 5, 10 );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );

			// Altra prova identica deve dare lo stesso risultato
			Int32Rect foto2 = new Int32Rect( 0, 0, 2, 4 );
			Proiezione esito2 = proiettore.calcola( foto2 );
			Assert.AreEqual( esito.dest, atteso );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: orizzontale
		 * Ratio:  f<s   (ovviamente in senso assoluto [ABS] perchè una è maggiore di 1 l'altra no)
		 * autoZoom: false
		 * autoRotate: true 
		 */
		[TestMethod()]
		public void proiettaTest16() {

			Int32Rect foto = new Int32Rect( 0, 0, 47, 67 );
			Int32Rect stampante = new Int32Rect( 0, 0, 311, 266 );

			Assert.IsTrue( ratio( foto ) < (1 / ratio( stampante )) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 0, 24, 311, 218 );
			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsTrue( esito.effettuataRotazione );

			Assert.IsTrue( ratio( atteso ) - (1/ratio( foto )) < 0.09 );

		}


		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: verticale
		 * Ratio:  f=s
		 * autoZoom: false
		 * autoRotate: true
		 * 
		 * Con rotazione non deve tagliare
		 */
		[TestMethod()]
		public void proiettaTest17() {

			Int32Rect stampante = creaOriz();
			Int32Rect foto = creaVert();

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;

			Proiezione esito1 = proiettore.calcola( foto );

			Assert.AreEqual( esito1.sorg, foto );
			Assert.AreEqual( esito1.dest, stampante );
			Assert.IsTrue( esito1.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: orizzontale
		 * Ratio:  f>s   (ovviamente in senso relativo perchè una è maggiore di 1 l'altra no)
		 * autoZoom: false
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest18() {

			Int32Rect foto = new Int32Rect( 0, 0, 47, 67 );
			Int32Rect stampante = new Int32Rect( 0, 0, 311, 187 );

			Assert.IsTrue( ratio( foto ) > (1 / ratio( stampante )) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 22, 0, 266, 187 );
			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsTrue( esito.effettuataRotazione );
			Assert.IsTrue( Math.Abs( ratio( foto ) - (1/ratio( atteso )) ) < 0.09 );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Orizzontale
		 * Stampante: verticale
		 * Ratio:  f>s
		 * autoZoom: false
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest19() {

			Int32Rect foto = new Int32Rect( 0, 0, 177, 111 );
			Int32Rect stampante = new Int32Rect( 0, 0, 189, 200 );

			Assert.IsTrue( ratio( foto ) > 1 / ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );
			Int32Rect atteso = new Int32Rect( 32, 0, 125, 200 );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsTrue( esito.effettuataRotazione );
		}



		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: verticale
		 * Ratio:  f=s
		 * autoZoom: false
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest20() {

			Int32Rect foto = creaOriz();
			Int32Rect stampante = creaVert();

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsTrue( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Orizzontale
		 * Stampante: verticale
		 * Ratio:  f>s
		 * autoZoom: false
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest21() {

			Int32Rect foto = new Int32Rect( 0, 0, 177, 111 );
			Int32Rect stampante = new Int32Rect( 0, 0, 189, 200 );

			Assert.IsTrue( ratio( foto ) > 1 / ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );
			int newh = (int)(200f / (177f / 111f));
			Assert.IsTrue( newh == 125 );
			Int32Rect atteso = new Int32Rect( 32, 0, newh, 200 );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsTrue( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: orizzontale
		 * Ratio:  f<s
		 * autoZoom: false
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest22() {

			Int32Rect stampante = new Int32Rect( 0, 0, 901, 607 );
			Int32Rect foto = new Int32Rect( 0, 0, 407, 399 );

			Assert.IsTrue( ratio( foto ) < ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 141, 0, 619, 607 );  // Rimane uguale l'altezza. Cambia la larghezza

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}


		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: orizzontale
		 * Ratio:  f=s
		 * autoZoom: false
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest23() {

			Int32Rect stampante = creaOriz();
			Int32Rect foto = creaOriz();

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = false;
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: orizzontale
		 * Ratio:  f>s
		 * autoZoom: false
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest24() {

			Int32Rect stampante = new Int32Rect( 0, 0, 193, 155 );
			Int32Rect foto = new Int32Rect( 0, 0, 12355, 8997 );

			Assert.IsTrue( ratio( foto ) > ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 0, 7, 193, 140 );  // Rimane uguale la larghezza. Cambia l'altezza

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Orizzontale
		 * Stampante: Orizzontale
		 * Ratio:  f<s
		 * autoZoom: true
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest25() {

			Int32Rect foto = new Int32Rect( 0, 0, 100, 80 );
			Int32Rect stampante = new Int32Rect( 0, 0, 10, 6 );

			Assert.IsTrue( ratio( foto ) < ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			proiettore.autoRotate = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 0, 10, 100, 60 );

			Assert.AreEqual( esito.sorg, atteso );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}


		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Verticale
		 * Stampante: Verticale
		 * Ratio:  f=s
		 * autoZoom: true
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest26() {

			Int32Rect foto = new Int32Rect( 0, 0, 80, 100 );
			Int32Rect stampante = new Int32Rect( 0, 0, 16, 20 );

			Assert.IsTrue( ratio( foto ) == ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			proiettore.autoRotate = true;
			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Verticale
		 * Stampante: verticale
		 * Ratio:  f>s
		 * autoZoom: true
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest27() {

			Int32Rect foto = new Int32Rect( 0, 0, 80, 100 );
			Int32Rect stampante = new Int32Rect( 0, 0, 6, 10 );

			Assert.IsTrue( ratio( foto ) > ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			proiettore.autoRotate = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 10, 0, 60, 100 );

			Assert.AreEqual( esito.sorg, atteso );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: orizzontale
		 * Ratio:  f<s   (ovviamente in senso assoluto [ABS] perchè una è maggiore di 1 l'altra no)
		 * autoZoom: true
		 * autoRotate: true 
		 */
		[TestMethod()]
		public void proiettaTest28() {

			Int32Rect foto1 = new Int32Rect( 0, 0, 80, 100 );
			Int32Rect stampante1 = new Int32Rect( 0, 0, 22, 20 );

			Assert.IsTrue( ratio( foto1 ) < (1 / ratio( stampante1 )) );

			ProiettoreArea proiettore1 = new ProiettoreArea( stampante1 );
			proiettore1.autoRotate = true;
			proiettore1.autoZoomToFit = true;

			Proiezione esito1 = proiettore1.calcola( foto1 );

			Int32Rect atteso1 = new Int32Rect( 0, 6, 80, 88 );
			Assert.AreEqual( esito1.sorg, atteso1 );
			Assert.AreEqual( esito1.dest, stampante1 );
			Assert.IsTrue( esito1.effettuataRotazione );


			// Controllo che il ratio finale sia giusto (arrotondando a 3 decimali)
			float vr1 = (float) Math.Round( ratio( atteso1 ), 3 );
			float vr2 = (float)Math.Round( 1/ratio( stampante1 ), 3 );
			Assert.IsTrue( vr1 == vr2 );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: verticale
		 * Ratio:  f=s
		 * autoZoom: true
		 * autoRotate: true
		 * 
		 * Con rotazione non deve tagliare
		 */
		[TestMethod()]
		public void proiettaTest29() {

			Int32Rect stampante = new Int32Rect( 0, 0, 50, 45 );
			Int32Rect foto = new Int32Rect( 0, 0, 90, 100 );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;
			proiettore.autoZoomToFit = true;

			Proiezione esito1 = proiettore.calcola( foto );

			Assert.AreEqual( esito1.dest, stampante );
			Assert.AreEqual( esito1.sorg, foto );
			Assert.IsTrue( esito1.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: orizzontale
		 * Ratio:  f>s
		 * autoZoom: true
		 * autoRotate: true 
		 */
		[TestMethod()]
		public void proiettaTest30() {

			Int32Rect foto1 = new Int32Rect( 0, 0, 90, 100 );
			Int32Rect stampante1 = new Int32Rect( 0, 0, 20, 15 );

			float rf = (float)Math.Round( ratio( foto1 ), 3 );
			float rs = (float)Math.Round( 1/ratio( stampante1 ), 3 );
			Assert.IsTrue( rf > rs );

			ProiettoreArea proiettore1 = new ProiettoreArea( stampante1 );
			proiettore1.autoRotate = true;
			proiettore1.autoZoomToFit = true;

			Proiezione esito1 = proiettore1.calcola( foto1 );

			Int32Rect atteso1 = new Int32Rect( 7, 0, 75, 100 );
			Assert.AreEqual( esito1.dest, stampante1 );
			Assert.AreEqual( esito1.sorg, atteso1 );
			Assert.IsTrue( esito1.effettuataRotazione );


			// Controllo che il ratio finale sia giusto (arrotondando a 3 decimali)
			float vr1 = (float)Math.Round( ratio( atteso1 ), 3 );
			float vr2 = (float)Math.Round( 1 / ratio( stampante1 ), 3 );
			Assert.IsTrue( vr1 == vr2 );
		}


		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Orizzontale
		 * Stampante: Verticale
		 * Ratio:  f<s
		 * autoZoom: true
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest31() {

			Int32Rect foto = new Int32Rect( 0, 0, 100, 80 );
			Int32Rect stampante = new Int32Rect( 0, 0, 6, 10 );

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( 1 / ratio( stampante ), 3 );
			Assert.IsTrue( rf < rs );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			proiettore.autoRotate = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect(0,10,100,60);

			Assert.AreEqual( esito.sorg, atteso );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsTrue( esito.effettuataRotazione );

			// Controllo che il ratio finale sia giusto (arrotondando a 3 decimali)
			float vr1 = (float)Math.Round( ratio( esito.sorg ), 3 );
			float vr2 = (float)Math.Round( 1/ratio( stampante ), 3 );
			Assert.IsTrue( vr1 == vr2 );

		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: verticale
		 * Ratio:  f=s
		 * autoZoom: false
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest32() {

			Int32Rect foto = new Int32Rect( 0, 0, 100, 80 );
			Int32Rect stampante = new Int32Rect( 0, 0, 40, 50 );

			float r1 = (float) Math.Round( ratio(foto), 3 );
			float r2 = (float) Math.Round( 1/ratio(stampante) , 3 );
			Assert.IsTrue( r1 == r2 );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;
			proiettore.autoZoomToFit = true;

			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsTrue( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Orizzontale
		 * Stampante: Verticale
		 * Ratio:  f>s
		 * autoZoom: true
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest33() {

			Int32Rect foto = new Int32Rect( 0, 0, 100, 54 );
			Int32Rect stampante = new Int32Rect( 0, 0, 6, 10 );

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( 1/ratio( stampante ), 3 );
			Assert.IsTrue( rf > rs );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			proiettore.autoRotate = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 5, 0, 89, 54 );

			Assert.AreEqual( esito.sorg, atteso );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsTrue( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: orizzontale
		 * Ratio:  f<s
		 * autoZoom: true
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest34() {

			Int32Rect stampante = new Int32Rect( 0, 0, 901, 607 );
			Int32Rect foto = new Int32Rect( 0, 0, 407, 399 );

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( ratio( stampante ), 3 );
			Assert.IsTrue( rf < rs );


			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;
			proiettore.autoZoomToFit = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 0, 62, 407, 274 );  // Rimane uguale l'altezza. Cambia la larghezza

			Assert.AreEqual( esito.sorg, atteso );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}


		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: orizzontale
		 * Ratio:  f=s
		 * autoZoom: false
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest35() {

			Int32Rect stampante = creaOriz();
			Int32Rect foto = creaOriz();

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( ratio( stampante ), 3 );
			Assert.IsTrue( rf == rs );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: orizzontale
		 * Ratio:  f>s
		 * autoZoom: true
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest36() {

			Int32Rect stampante = new Int32Rect( 0, 0, 193, 155 );
			Int32Rect foto = new Int32Rect( 0, 0, 12355, 8997 );

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( ratio( stampante ), 3 );
			Assert.IsTrue( rf > rs );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 0, 7, 193, 140 );  // Rimane uguale la larghezza. Cambia l'altezza

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Orizzontale
		 * Stampante: Orizzontale
		 * Ratio:  f<s
		 * autoZoom: true
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest37() {

			Int32Rect foto = new Int32Rect( 0, 0, 100, 80 );
			Int32Rect stampante = new Int32Rect( 0, 0, 10, 6 );

			Assert.IsTrue( ratio( foto ) < ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 0, 10, 100, 60 );

			Assert.AreEqual( esito.sorg, atteso );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Verticale
		 * Stampante: Verticale
		 * Ratio:  f=s
		 * autoZoom: true
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest38() {

			Int32Rect foto = new Int32Rect( 0, 0, 80, 100 );
			Int32Rect stampante = new Int32Rect( 0, 0, 16, 20 );

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( ratio( stampante ), 3 );
			Assert.IsTrue( rf == rs );


			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Verticale
		 * Stampante: verticale
		 * Ratio:  f>s
		 * autoZoom: true
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest39() {

			Int32Rect foto = new Int32Rect( 0, 0, 80, 100 );
			Int32Rect stampante = new Int32Rect( 0, 0, 6, 10 );

			Assert.IsTrue( ratio( foto ) > ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 10, 0, 60, 100 );

			Assert.AreEqual( esito.sorg, atteso );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: orizzontale
		 * Ratio:  f<s   
		 * autoZoom: true
		 * autoRotate: false 
		 */
		[TestMethod()]
		public void proiettaTest40() {

			Int32Rect foto = new Int32Rect( 0, 0, 800, 1000 );
			Int32Rect stampante = new Int32Rect( 0, 0, 22, 20 );

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( 1/ratio( stampante ), 3 );
			Assert.IsTrue( rf < rs );

			ProiettoreArea proiettore1 = new ProiettoreArea( stampante );
			proiettore1.autoZoomToFit = true;

			Proiezione esito1 = proiettore1.calcola( foto );

			Int32Rect atteso1 = new Int32Rect( 0, 136, 800, 727 );
			Assert.AreEqual( esito1.sorg, atteso1 );
			Assert.AreEqual( esito1.dest, stampante );
			Assert.IsFalse( esito1.effettuataRotazione );

			// Controllo che il ratio finale sia giusto (arrotondando a 3 decimali)
			float vr1 = (float)Math.Round( ratio( esito1.sorg ), 3 );
			float vr2 = (float)Math.Round( ratio( stampante ), 3 );
			Assert.IsTrue( vr1 == vr2 );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: orizzontale
		 * Ratio:  f=s
		 * autoZoom: true
		 * autoRotate: false 
		 */
		[TestMethod()]
		public void proiettaTest41() {

			Int32Rect foto = new Int32Rect( 0, 0, 800, 1000 );
			Int32Rect stampante = new Int32Rect( 0, 0, 100, 80 );

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( 1f / ratio( stampante ), 3 );
			Assert.IsTrue( rf == rs );

			ProiettoreArea proiettore1 = new ProiettoreArea( stampante );
			proiettore1.autoZoomToFit = true;

			Proiezione esito1 = proiettore1.calcola( foto );

			Int32Rect atteso1 = new Int32Rect( 0, 180, 800, 640 );
			Assert.AreEqual( esito1.sorg, atteso1 );
			Assert.AreEqual( esito1.dest, stampante );
			Assert.IsFalse( esito1.effettuataRotazione );

			// Controllo che il ratio finale sia giusto (arrotondando a 3 decimali)
			float vr1 = (float)Math.Round( ratio( esito1.sorg ), 3 );
			float vr2 = (float)Math.Round( ratio( stampante ), 3 );
			Assert.IsTrue( vr1 == vr2 );
		}


		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: verticale
		 * Stampante: orizzontale
		 * Ratio:  f>s
		 * autoZoom: true
		 */
		[TestMethod()]
		public void proiettaTest42() {

			Int32Rect foto1 = new Int32Rect( 0, 0, 9000, 10000 );
			Int32Rect stampante1 = new Int32Rect( 0, 0, 2000, 1500 );

			float rf = (float)Math.Round( ratio( foto1 ), 3 );
			float rs = (float)Math.Round( 1 / ratio( stampante1 ), 3 );
			Assert.IsTrue( rf > rs );

			ProiettoreArea proiettore1 = new ProiettoreArea( stampante1 );
			proiettore1.autoZoomToFit = true;

			Proiezione esito1 = proiettore1.calcola( foto1 );

			Int32Rect atteso1 = new Int32Rect( 0, 1625, 9000, 6749 );
			Assert.AreEqual( esito1.dest, stampante1 );
			Assert.AreEqual( esito1.sorg, atteso1 );
			Assert.IsFalse( esito1.effettuataRotazione );


			// Controllo che il ratio finale sia giusto (arrotondando a 3 decimali)
			float vr1 = (float)Math.Round( ratio( atteso1 ), 2 );
			float vr2 = (float)Math.Round( ratio( stampante1 ), 2 );
			Assert.IsTrue( vr1 == vr2 );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Orizzontale
		 * Stampante: Verticale
		 * Ratio:  f<s
		 * autoZoom: true
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest43() {

			Int32Rect foto = new Int32Rect( 0, 0, 100, 80 );
			Int32Rect stampante = new Int32Rect( 0, 0, 6, 10 );

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( 1 / ratio( stampante ), 3 );
			Assert.IsTrue( rf < rs );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 26, 0, 48, 80 );

			Assert.AreEqual( esito.sorg, atteso );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );

			// Controllo che il ratio finale sia giusto (arrotondando a 3 decimali)
			float vr1 = (float)Math.Round( ratio( atteso ), 3 );
			float vr2 = (float)Math.Round( ratio( stampante ), 3 );
			Assert.IsTrue( vr1 == vr2 );

		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Orizzontale
		 * Stampante: Verticale
		 * Ratio:  f=s
		 * autoZoom: true
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest44() {

			Int32Rect foto = new Int32Rect( 0, 0, 1000, 600 );
			Int32Rect stampante = new Int32Rect( 0, 0, 60, 100 );

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( 1 / ratio( stampante ), 3 );
			Assert.IsTrue( rf == rs );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 320, 0, 360, 600 );

			Assert.AreEqual( esito.sorg, atteso );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );

			// Controllo che il ratio finale sia giusto (arrotondando a 3 decimali)
			float vr1 = (float)Math.Round( ratio( atteso ), 3 );
			float vr2 = (float)Math.Round( ratio( stampante ), 3 );
			Assert.IsTrue( vr1 == vr2 );

		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: Orizzontale
		 * Stampante: Verticale
		 * Ratio:  f>s
		 * autoZoom: true
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest45() {

			Int32Rect foto = new Int32Rect( 0, 0, 1000, 540 );
			Int32Rect stampante = new Int32Rect( 0, 0, 6, 10 );

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( 1 / ratio( stampante ), 3 );
			Assert.IsTrue( rf > rs );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 338, 0, 324, 540 );

			Assert.AreEqual( esito.sorg, atteso );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );

			// Controllo che il ratio finale sia giusto (arrotondando a 3 decimali)
			float vr1 = (float)Math.Round( ratio( atteso ), 2 );
			float vr2 = (float)Math.Round( ratio( stampante ), 2 );
			Assert.IsTrue( vr1 == vr2 );

		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: orizzontale
		 * Ratio:  f<s
		 * autoZoom: true
		 * autoRotate: falae
		 */
		[TestMethod()]
		public void proiettaTest46() {

			Int32Rect stampante = new Int32Rect( 0, 0, 901, 607 );
			Int32Rect foto = new Int32Rect( 0, 0, 407, 399 );

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( ratio( stampante ), 3 );
			Assert.IsTrue( rf < rs );


			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 0, 62, 407, 274 );

			Assert.AreEqual( esito.sorg, atteso );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}

		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: orizzontale
		 * Ratio:  f=s
		 * autoZoom: true
		 * autoRotate: false
		 */
		[TestMethod()]
		public void proiettaTest47() {

			Int32Rect stampante = creaOriz();
			Int32Rect foto = creaOriz();

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( ratio( stampante ), 3 );
			Assert.IsTrue( rf == rs );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;

			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}


		/**
		 * Vedi battaglia navale!
		 * 
		 * Foto: orizzontale
		 * Stampante: orizzontale
		 * Ratio:  f>s
		 * autoZoom: true
		 * autoRotate: true
		 */
		[TestMethod()]
		public void proiettaTest48() {

			Int32Rect stampante = new Int32Rect( 0, 0, 193, 155 );
			Int32Rect foto = new Int32Rect( 0, 0, 12355, 8997 );

			float rf = (float)Math.Round( ratio( foto ), 3 );
			float rs = (float)Math.Round( ratio( stampante ), 3 );
			Assert.IsTrue( rf > rs );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = true;
			Proiezione esito = proiettore.calcola( foto );

			Int32Rect atteso = new Int32Rect( 576, 0, 11202, 8997 );

			Assert.AreEqual( esito.sorg, atteso );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}


		private Int32Rect creaOriz() {
			return creaOriz( null );
		}

		private Int32Rect creaVert() {
			return creaVert( null );
		}

		/**
		 * maggiora = true  allora  incremento il ratio
		 * maggiora = false allora  decremento il ratio
		 */
		private Int32Rect creaVert( bool? maggiora ) {

			int deltaW = (maggiora == true) ? DELTA : 0;
			int deltaH = (maggiora == false)  ? DELTA : 0;

			return new Int32Rect( 0, 0, L1+deltaW, L2+deltaH );
		}

		/**
		 * maggiora = true  allora  incremento il ratio
		 * maggiora = false allora  decremento il ratio
		 */
		private Int32Rect creaOriz( bool? maggiora ) {

			int deltaW = (maggiora == false) ? DELTA : 0;
			int deltaH = (maggiora == true) ? DELTA : 0;

			return new Int32Rect( 0, 0, L2+deltaW, L1+deltaH );
		}

		private float ratio( Size size ) {
			return ratio( size.Width, size.Height );
		}
		private float ratio( double w, double h ) {
			return (float) (w/h);
		}
		private float ratio( int w, int h ) {
			return (float)w / (float)h;
		}
		private float ratio( Int32Rect r ) {
			return ratio( r.Width, r.Height );
		}

	}
}
