using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Core.Servizi.Utilita;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Database;

namespace Digiphoto.Lumen.Core.Servizi.Utilita.Tests {

	[TestClass()]
	public class UtilitaSrvImplTests {

		private UtilitaSrvImpl _impl;

		[TestMethod()]
		public void inviaLogTest() {

			bool esito = _impl.inviaLog();

			Assert.IsTrue( esito );
		}

		[ClassInitialize()]
		public static void MyClassInitialize( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}

		[TestInitialize]
		public void Init() {

			LumenApplication app = LumenApplication.Instance;

			using( new UnitOfWorkScope() ) {

				_impl = new UtilitaSrvImpl();
				_impl.start();
			}
		}

		[TestCleanup]
		public void Cleanup() {
			_impl.Dispose();
		}


	}
}