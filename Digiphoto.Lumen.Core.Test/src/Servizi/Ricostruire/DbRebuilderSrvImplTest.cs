using Digiphoto.Lumen.Servizi.Ricostruzione;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Database;

namespace Digiphoto.Lumen.Core.Test.Servizi.Ricostruire {
    
    
    /// <summary>
    ///This is a test class for DbRebuilderSrvImplTest and is intended
    ///to contain all DbRebuilderSrvImplTest Unit Tests
    ///</summary>
	[TestClass()]
	public class DbRebuilderSrvImplTest {


		private TestContext testContextInstance;

		UnitOfWorkScope _unitOfWorkScope;

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
		[ClassInitialize()]
		public static void MyClassInitialize( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}
		
		// Use ClassCleanup to run code after all tests in a class have run
		[ClassCleanup()]
		public static void MyClassCleanup() {
			LumenApplication.Instance.ferma();
		}
		
		//Use TestInitialize to run code before running each test
		[TestInitialize()]
		public void MyTestInitialize() {
			_unitOfWorkScope = new UnitOfWorkScope();
		}

		//Use TestCleanup to run code after each test has run
		[TestCleanup()]
		public void MyTestCleanup() {
			_unitOfWorkScope.Dispose();
		}
		
		#endregion


		/// <summary>
		///A test for analizzare
		///</summary>
		[TestMethod()]
		public void analizzareTest() {

			DbRebuilderSrvImpl target = new DbRebuilderSrvImpl(); // TODO: Initialize to an appropriate value
			target.analizzare();
	
			

		}

		/// <summary>
		///A test for ricostruire
		///</summary>
		[TestMethod()]
		public void ricostruireTest() {
			
			DbRebuilderSrvImpl target = new DbRebuilderSrvImpl(); // TODO: Initialize to an appropriate value
			
			target.analizzare();
			
			target.ricostruire();
			
			System.Diagnostics.Trace.WriteLine( "Fotografi creati " + target.contaFotografiAggiunti );
			System.Diagnostics.Trace.WriteLine( "Fotografie create " + target.contaFotoAggiunte );

			Assert.IsTrue( target.contaFotoAggiunte == target.contaFotoMancanti );

		}
	}
}
