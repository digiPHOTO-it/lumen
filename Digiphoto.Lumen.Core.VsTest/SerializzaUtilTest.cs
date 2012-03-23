using Digiphoto.Lumen.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Digiphoto.Lumen.Imaging.Correzioni;

namespace Digiphoto.Lumen.Core.VsTest
{
    
    
    /// <summary>
    ///This is a test class for SerializzaUtilTest and is intended
    ///to contain all SerializzaUtilTest Unit Tests
    ///</summary>
	[TestClass()]
	public class SerializzaUtilTest {


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
		///A test for objectToString
		///</summary>
		[TestMethod()]
		public void objectToStringTest() {

			CorrezioniList lista = new CorrezioniList();
			lista.Add( new Ruota() { gradi = 35 } );
			lista.Add( new BiancoNero() );
			lista.Add( new Ruota() { gradi = 50 } );

			string ris = SerializzaUtil.objectToString( lista, typeof(CorrezioniList) );



			object oo = SerializzaUtil.stringToObject( ris, typeof( CorrezioniList ) );

		}

		/// <summary>
		///A test for stringToObject
		///</summary>
		[TestMethod()]
		public void stringToObjectTest() {
			string xml = string.Empty; // TODO: Initialize to an appropriate value
			Type objType = null; // TODO: Initialize to an appropriate value
			object expected = null; // TODO: Initialize to an appropriate value
			object actual;
			actual = SerializzaUtil.stringToObject( xml, objType );
			Assert.AreEqual( expected, actual );
			Assert.Inconclusive( "Verify the correctness of this test method." );
		}
	}
}
