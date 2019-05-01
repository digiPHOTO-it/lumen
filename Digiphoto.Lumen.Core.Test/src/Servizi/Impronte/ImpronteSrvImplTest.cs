using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Database;
using System.Linq;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Ricerca;
using System.Collections.Generic;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Core.Servizi.Impronte;

namespace Digiphoto.Lumen.Core.Test.Servizi.Impronte {

	[TestClass]
	public class ImpronteSrvImplTest {

		private ImpronteZKTecoSrvImpl _impl;


		[TestInitialize]
		public void Init() {

			LumenApplication app = LumenApplication.Instance;
			app.avvia();

			IImpronteSrv srv2 = LumenApplication.Instance.getServizioAvviato<IImpronteSrv>();
			this._impl = (ImpronteZKTecoSrvImpl) srv2;

			Assert.IsTrue( srv2.statoRun == Lumen.Servizi.StatoRun.Running, "Lo scanner deve essere connesso" );

			// -------
			using( LumenEntities dbContext = new LumenEntities() ) {

			}
		}

		int contaAcquisite = 0;
		void OnImmagineAcquisita( object sender, ScansioneEvent eventArgs ) {

			++contaAcquisite;

			Console.WriteLine( "acquisito " + contaAcquisite + " img = " + eventArgs.bmpFileName );
			
		}

		[TestMethod]
		public void acquisireTest() {

			_impl.Listen( OnImmagineAcquisita );

			contaAcquisite = 0;

			Console.WriteLine( "scansionare il dito" );
			do {

				System.Threading.Thread.Sleep( 2000 );
				
			} while( contaAcquisite < 3 );
			

		}

		


		[TestCleanup]
		public void Cleanup() {
			_impl.Dispose();
		}


	}
}

