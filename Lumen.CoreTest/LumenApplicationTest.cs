using Digiphoto.Lumen.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using log4net.Appender;
using System.Linq;
using System.Collections;
using log4net;
using log4net.Config;
using System.Collections.Generic;
using Digiphoto.Lumen.Applicazione;

namespace Lumen.CoreTest
{
    
    
    /// <summary>
    ///This is a test class for LumenApplicationTest and is intended
    ///to contain all LumenApplicationTest Unit Tests
    ///</summary>
	[TestClass()]
	public class LumenApplicationTest {


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
		// Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext) {
			XmlConfigurator.Configure();
		}
		
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


		[TestMethod()]
		public void AppConfigForTestIsUsedTest() {
			Assert.AreEqual( "unit.test", ConfigurationManager.AppSettings ["config.type"] );
		}

		[TestMethod()]
		public void Log4NetConfigurationLoaded() {
			IAppender [] appenders = LogManager.GetRepository().GetAppenders();
			ICollection<String> appenderNames = appenders.Select( appender => appender.Name ).ToArray();
			Assert.IsTrue( appenderNames.Contains( "FileAppender" ) );
		}

		/// <summary>
		///A test for Instance
		///</summary>
		[TestMethod()]
		public void InstanceTest() {
			
			LumenApplication actual1 = LumenApplication.Instance;
			LumenApplication actual2 = LumenApplication.Instance;
			Assert.AreSame( actual1, actual2 );

		}

		/// <summary>
		///A test for avvia
		///</summary>
		[TestMethod()]
		public void avviaTest() {
			LumenApplication_Accessor target = new LumenApplication_Accessor(); // TODO: Initialize to an appropriate value
			Assert.IsFalse( target.avviata );
			target.avvia();
			Assert.IsTrue( target.avviata );
		}
	}
}
