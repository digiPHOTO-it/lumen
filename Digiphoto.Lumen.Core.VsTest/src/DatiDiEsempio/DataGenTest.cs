using Digiphoto.Lumen.Core.DatiDiEsempio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Core.VsTest
{
    
    
    /// <summary>
    ///This is a test class for DataGenTest and is intended
    ///to contain all DataGenTest Unit Tests
    ///</summary>
	[TestClass()]
	public class DataGenTest {


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


		/// <summary>
		///A test for generaMolti
		///</summary>
		public void generaMoltiTestHelper<TEntity>()
			where TEntity : class {
			DataGen<TEntity> target = new DataGen<TEntity>();
			int quanti = 2;
			IEnumerable<TEntity> actual;
			actual = target.generaMolti( quanti );

			IEnumerator<TEntity> enu = actual.GetEnumerator();
			int conta = 0;
			while( enu.MoveNext() ) {
				TEntity entita = enu.Current;
				++conta;
				Console.WriteLine( conta.ToString() );
			}
			Assert.IsTrue( conta == quanti );

		}

		[TestMethod()]
		public void generaMoltiTest() {
			generaMoltiTestHelper<GenericParameterHelper>();
		}

		[TestMethod()]
		public void generaMoltiFotografiTest() {
			generaMoltiTestHelper<Fotografo>();
		}

	}
}
