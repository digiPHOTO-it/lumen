using Digiphoto.Lumen.Servizi.Stampare;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Database;
using System.Threading;
using Digiphoto.Lumen.Imaging.Nativa;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Imaging;

namespace Digiphoto.Lumen.Core.VsTest
{
    
    
    /// <summary>
    ///This is a test class for SpoolStampeSrvImplTest and is intended
    ///to contain all SpoolStampeSrvImplTest Unit Tests
    ///</summary>
	[TestClass()]
	public class SpoolStampeSrvImplTest : IObserver<StampatoMsg>  {

		private SpoolStampeSrvImpl _impl;

		private int _contaStampe = 0;

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
		public static void MyClassInitialize( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}

		[TestInitialize]
		public void Init() {

			LumenApplication app = LumenApplication.Instance;

			IObservable<StampatoMsg> observable = app.bus.Observe<StampatoMsg>();
			observable.Subscribe( this );

			_impl = new SpoolStampeSrvImpl();
			_impl.start();
		}

		[TestCleanup]
		public void Cleanup() {
			_impl.Dispose();
		}


		/// <summary>
		///A test for accodaStampa
		///</summary>
		[TestMethod()]
		public void accodaStampaTest() {

			// EsecutoreStampaNet s = new EsecutoreStampaNet();
			const int QUANTE = 6;
			using( new UnitOfWorkScope() ) {

				ParamStampaFoto param = new ParamStampaFoto();
				param.numCopie = 1;
				param.autoRuota = true;
				param.nomeStampante = "doPDF v7";

				LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;
				var fotos = dbContext.Fotografie.Top( QUANTE.ToString() );
				foreach( Fotografia foto in fotos ) {
					
					param.autoZoomToFit = true;
					_impl.accodaStampa( foto, param );

					ParamStampaFoto p2 = (ParamStampaFoto)param.Clone();
					p2.autoZoomToFit = false;
					_impl.accodaStampa( foto, p2 );
				}

				// Attendo che le due stampe siano terminate
				do {
					Thread.Sleep( 5000 );
				} while( _contaStampe < QUANTE*2 );
			}

			
		}

		public void OnNext( StampatoMsg value ) {
			Assert.IsTrue( value.lavoroDiStampa.esitostampa == EsitoStampa.Ok );
			_contaStampe++;
		}

		public void OnCompleted() {
			throw new NotImplementedException();
		}

		public void OnError( Exception error ) {
			throw new NotImplementedException();
		}
	}
}
