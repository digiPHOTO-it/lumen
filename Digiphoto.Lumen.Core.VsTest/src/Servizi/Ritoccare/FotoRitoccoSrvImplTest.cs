using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Model;
using System.Data.Entity;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Core.VsTest.src.Servizi.Ritoccare {

	[TestClass]
	public class FotoRitoccoSrvImplTest {

		FotoRitoccoSrvImpl _impl;

		[ClassInitialize]
		public static void inizializzaClasse( TestContext ctx ) {
			Lumen.Applicazione.LumenApplication.Instance.avvia();
		}

		[ClassCleanup]
		public static void cleanupClasse() {
			Lumen.Applicazione.LumenApplication.Instance.ferma();
		}

		[TestInitialize]
		public void inizializzaTest() {
			_impl = new FotoRitoccoSrvImpl();
		}

		[TestCleanup]
		public void cleanupTest() {
			_impl.Dispose();
		}

		[TestMethod]
		public void modificaConProgrammaEsterno() {
			
			// Carico una foto a caso
			using( LumenEntities dbContext = new LumenEntities() ) {
				var fotos = dbContext.Fotografie.Take( 2 );
				_impl.modificaConProgrammaEsterno( fotos.ToArray() );
			}
		}
	}
}
