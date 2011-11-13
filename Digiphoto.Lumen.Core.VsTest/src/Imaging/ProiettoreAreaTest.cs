using Digiphoto.Lumen.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;

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

			Rectangle foto = new Rectangle( 0, 0, 5, 12);
			Rectangle stampante = new Rectangle( 0, 0, 50, 100 );

			Assert.IsTrue( ratio( foto ) < ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );
			int neww = (int)(100 * (5f / 12f));
			Assert.IsTrue( neww == 41 );
			Rectangle atteso = new Rectangle( 4,0, neww, 100 );

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

			Rectangle foto = creaVert();
			Rectangle stampante = creaVert();

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

			Rectangle foto = new Rectangle( 0, 0, 50, 100 );
			Rectangle stampante = new Rectangle( 0, 0, 5, 12 );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );
			Rectangle atteso = new Rectangle( 0, 1, 5, 10 );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );

			// Altra prova identica deve dare lo stesso risultato
			Rectangle foto2 = new Rectangle( 0, 0, 2, 4 );
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

			Rectangle foto = new Rectangle( 0, 0, 47, 67 );
			Rectangle stampante = new Rectangle( 0, 0, 311, 266 );
			
			Assert.IsTrue( ratio(foto) < (1/ratio(stampante)));

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );

			Rectangle atteso = new Rectangle( 62, 0, 186, 266 );
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

			Rectangle foto = creaVert();
			Rectangle stampante = creaOriz();

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );

			Rectangle atteso = new Rectangle( 607, 0, 3056, L1 );
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

			Rectangle foto = new Rectangle( 0, 0, 47, 67 );
			Rectangle stampante = new Rectangle( 0, 0, 311, 187 );

			Assert.IsTrue( ratio( foto ) > (1 / ratio( stampante )) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );

			Rectangle atteso = new Rectangle( 90, 0, 131, 187 );
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

			Rectangle foto = new Rectangle( 0, 0, 13, 5 );
			Rectangle stampante = new Rectangle( 0, 0, 50, 149 );

			Assert.IsTrue( ratio( foto ) < 1/ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );
			int newh = (int)(50 / (13f / 5f));
			Assert.IsTrue( newh == 19 );
			Rectangle atteso = new Rectangle( 0, 65, 50, newh );

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

			Rectangle stampante = creaVert();
			Rectangle foto = creaOriz();

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = false;
			proiettore.autoRotate = false;

			Proiezione esito = proiettore.calcola( foto );

			Rectangle risultatoAtteso = new Rectangle( 0, 607, L1, 3056 );

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

			Rectangle foto = new Rectangle( 0, 0, 177, 111 );
			Rectangle stampante = new Rectangle( 0, 0, 189, 200 );

			Assert.IsTrue( ratio( foto ) > 1 / ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );
			int newh = (int)(189f / (177f / 111f));
			Assert.IsTrue( newh == 118 );
			Rectangle atteso = new Rectangle( 0, 41, 189, newh );

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

			Rectangle stampante = new Rectangle( 0, 0, 901, 607 );
			Rectangle foto = new Rectangle( 0,0,407,399);

			Assert.IsTrue( ratio( foto ) < ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );

			Rectangle atteso = new Rectangle( 141, 0, 619, 607 );  // Rimane uguale l'altezza. Cambia la larghezza

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

			Rectangle stampante = creaOriz();
			Rectangle foto = creaOriz();

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

			Rectangle stampante = new Rectangle( 0, 0, 193, 155 );
			Rectangle foto = new Rectangle( 0, 0, 12355, 8997 );

			Assert.IsTrue( ratio( foto ) > ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );

			Proiezione esito = proiettore.calcola( foto );

			Rectangle atteso = new Rectangle( 0, 7, 193, 140 );  // Rimane uguale la larghezza. Cambia l'altezza

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

			Rectangle foto = new Rectangle( 0, 0, 5, 12 );
			Rectangle stampante = new Rectangle( 0, 0, 50, 100 );

			Assert.IsTrue( ratio( foto ) < ratio( stampante ) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );
			
			int neww = (int)(100 * (5f / 12f));
			Assert.IsTrue( neww == 41 );
			Rectangle atteso = new Rectangle( 4, 0, neww, 100 );

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

			Rectangle foto = creaVert();
			Rectangle stampante = creaVert();

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

			Rectangle foto = new Rectangle( 0, 0, 50, 100 );
			Rectangle stampante = new Rectangle( 0, 0, 5, 12 );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );
			Rectangle atteso = new Rectangle( 0, 1, 5, 10 );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsFalse( esito.effettuataRotazione );

			// Altra prova identica deve dare lo stesso risultato
			Rectangle foto2 = new Rectangle( 0, 0, 2, 4 );
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

			Rectangle foto = new Rectangle( 0, 0, 47, 67 );
			Rectangle stampante = new Rectangle( 0, 0, 311, 266 );

			Assert.IsTrue( ratio( foto ) < (1 / ratio( stampante )) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );

			Rectangle atteso = ProiettoreArea.ruota( new Rectangle( 0, 24, 311, 218 ) );
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

			Rectangle stampante = creaOriz();
			Rectangle foto = creaVert();

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;

			Proiezione esito1 = proiettore.calcola( foto );

			Assert.AreEqual( esito1.sorg, foto );
			Assert.AreEqual( esito1.dest, ProiettoreArea.ruota(stampante) );
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

			Rectangle foto = new Rectangle( 0, 0, 47, 67 );
			Rectangle stampante = new Rectangle( 0, 0, 311, 187 );

			Assert.IsTrue( ratio( foto ) > (1 / ratio( stampante )) );

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;
			Proiezione esito = proiettore.calcola( foto );

			Rectangle atteso = ProiettoreArea.ruota( new Rectangle( 22, 0, 266, 187 ) );
			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, atteso );
			Assert.IsTrue( esito.effettuataRotazione );
			Assert.IsTrue( Math.Abs( ratio( foto ) - ratio( atteso ) ) < 0.09 );
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

			Rectangle foto = creaOriz();
			Rectangle stampante = creaVert();

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, ProiettoreArea.ruota(stampante) );
			Assert.IsTrue( esito.effettuataRotazione );
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

			Rectangle stampante = creaOriz();
			Rectangle foto = creaOriz();

			ProiettoreArea proiettore = new ProiettoreArea( stampante );
			proiettore.autoZoomToFit = false;
			proiettore.autoRotate = true;

			Proiezione esito = proiettore.calcola( foto );

			Assert.AreEqual( esito.sorg, foto );
			Assert.AreEqual( esito.dest, stampante );
			Assert.IsFalse( esito.effettuataRotazione );
		}


		private Rectangle creaOriz() {
			return creaOriz( null );
		}

		private Rectangle creaVert() {
			return creaVert( null );
		}

		/**
		 * maggiora = true  allora  incremento il ratio
		 * maggiora = false allora  decremento il ratio
		 */
		private Rectangle creaVert( bool? maggiora ) {

			int deltaW = (maggiora == true) ? DELTA : 0;
			int deltaH = (maggiora == false)  ? DELTA : 0;

			return new Rectangle( 0, 0, L1+deltaW, L2+deltaH );
		}

		/**
		 * maggiora = true  allora  incremento il ratio
		 * maggiora = false allora  decremento il ratio
		 */
		private Rectangle creaOriz( bool? maggiora ) {

			int deltaW = (maggiora == false) ? DELTA : 0;
			int deltaH = (maggiora == true) ? DELTA : 0;

			return new Rectangle( 0, 0, L2+deltaW, L1+deltaH );
		}

		private float ratio( Size size ) {
			return (float)size.Width / (float)size.Height;
		}
		private float ratio( Rectangle r ) {
			return ratio( r.Size );
		}

	}
}
