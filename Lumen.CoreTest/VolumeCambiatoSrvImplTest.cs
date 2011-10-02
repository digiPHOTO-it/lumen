using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Management;
using System.Threading;

namespace Lumen.CoreTest
{
    
    
    /// <summary>
    ///This is a test class for VolumeCambiatoSrvImplTest and is intended
    ///to contain all VolumeCambiatoSrvImplTest Unit Tests
    ///</summary>
	[TestClass()]
	public class VolumeCambiatoSrvImplTest {


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
		///A test for VolumeCambiatoSrvImpl Constructor
		///</summary>
		[TestMethod()]
		[DeploymentItem( "Digiphoto.Lumen.Core.dll" )]
		public void VolumeCambiatoSrvImplConstructorTest() {
			VolumeCambiatoSrvImpl_Accessor target = new VolumeCambiatoSrvImpl_Accessor();
			Assert.IsFalse( target.isRunning );
		}

		/// <summary>
		///A test for Dispose
		///</summary>
		[TestMethod()]
		[DeploymentItem( "Digiphoto.Lumen.Core.dll" )]
		public void DisposeTest() {
			VolumeCambiatoSrvImpl_Accessor target = new VolumeCambiatoSrvImpl_Accessor(); // TODO: Initialize to an appropriate value
			target.Dispose();
			Assert.IsFalse( target.isRunning );
		}

		/// <summary>
		///A test for attesa
		///</summary>
		[TestMethod()]
		[DeploymentItem( "Digiphoto.Lumen.Core.dll" )]
		public void attesaEventiTest() {

			VolumeCambiatoSrvImpl_Accessor target = new VolumeCambiatoSrvImpl_Accessor(); // TODO: Initialize to an appropriate value
			
			target.start();
			Assert.IsTrue( target.isRunning );
			target.attesaBloccante = true;

			Console.WriteLine( "Controllare che la chiavetta non sia inserita.\nPremere invio per continuare" );
			Console.ReadLine();

			Assert.IsNull( target.ultimoDriveMontato );

			Console.WriteLine( "Ora simulo attesa. Inserire la chiavetta entro 2 minuti" );

			target.attesaEventi();

			Assert.IsNotNull( target.ultimoDriveMontato );

			Console.WriteLine( "Ora togliere la chiavetta." );
			Console.ReadLine();

			target.attesaEventi();

			Assert.IsNull( target.ultimoDriveMontato );


			Assert.IsTrue( target.isRunning );
			target.stop();
			Assert.IsFalse( target.isRunning );

			Console.WriteLine( "main thread: Worker thread has terminated." );

		}

		/// <summary>
		///A test for onVolumeCambiatoEvent
		///</summary>
		[TestMethod()]
		[DeploymentItem( "Digiphoto.Lumen.Core.dll" )]
		public void onVolumeCambiatoEventTest() {

			/*
			 * TODO
			 * 
			VolumeCambiatoSrvImpl_Accessor target = new VolumeCambiatoSrvImpl_Accessor(); // TODO: Initialize to an appropriate value
			object sender = null; // TODO: Initialize to an appropriate value
			EventArrivedEventArgs e = null; // TODO: Initialize to an appropriate value
			target.onVolumeCambiatoEvent( sender, e );
			Assert.Inconclusive( "A method that does not return a value cannot be verified." );
		
			 */
		}

		/// <summary>
		///A test for start
		///</summary>
		[TestMethod()]
		[DeploymentItem( "Digiphoto.Lumen.Core.dll" )]
		public void startTest() {
			VolumeCambiatoSrvImpl_Accessor target = new VolumeCambiatoSrvImpl_Accessor(); // TODO: Initialize to an appropriate value
			target.start();
			Assert.IsTrue( target.isRunning );
		}

		/// <summary>
		///A test for stop
		///</summary>
		[TestMethod()]
		[DeploymentItem( "Digiphoto.Lumen.Core.dll" )]
		public void stopTest() {

			VolumeCambiatoSrvImpl_Accessor target = new VolumeCambiatoSrvImpl_Accessor(); // TODO: Initialize to an appropriate value

			target.stop();
			Assert.IsFalse( target.isRunning );
		}









		public bool _inserita {
			get;
			set;
		}
	}
}
