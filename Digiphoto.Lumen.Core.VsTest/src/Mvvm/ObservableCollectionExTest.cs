using Digiphoto.Lumen.UI.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using System.Threading;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.Threading;

namespace Digiphoto.Lumen.UI.Mvvm.VsTest
{
    /// <summary>
    ///This is a test class for ObservableCollectionExTest and is intended
    ///to contain all ObservableCollectionExTest Unit Tests
    ///</summary>
	[TestClass()]
	public class ObservableCollectionExTest {

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


		ObservableCollection<int> _collezione;

		[TestMethod()]
		public void multiThreadCollectionEx() {

			_collezione = new ObservableCollectionEx<int>( new int [] { 1 } );
			Thread thread = new Thread( new ThreadStart( threadAggiungiNumeri ) );
			thread.IsBackground = true;
			thread.Start();

			do {
				Thread.Sleep( 200 );
			} while( _collezione.Count < 10 );

			thread.Join();

			// Verifico che ci siano 10 elementi
			Assert.IsTrue( _collezione.Count == 10 );

			// -----------------------
		}

		[TestMethod()]
		public void multiThreadCollection() {

			_collezione = new ObservableCollection<int>( new int [] { 1 }  );
			Thread thread = new Thread( new ThreadStart( threadAggiungiNumeri ) );
			thread.IsBackground = true;
			thread.Start();

			do {
				Thread.Sleep( 200 );
			} while( _collezione.Count < 10 );

			thread.Join();

			// Verifico che ci siano 10 elementi
			Assert.IsTrue( _collezione.Count == 10 );

			// -----------------------
		}


		void threadAggiungiNumeri() {

			for( int ii = 2; ii <= 10; ii++ ) {
				_collezione.Insert( 0, ii );  // inserisco sembre in testa
				Thread.Sleep( 300 );
			}
		}

	}
}
