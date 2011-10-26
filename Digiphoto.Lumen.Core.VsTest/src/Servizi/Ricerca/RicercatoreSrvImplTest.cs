using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Database;
using System.Linq;
using System.Transactions;
using System.Data.Objects;
using Digiphoto.Lumen.Model;
using System.Reflection;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Servizi;
using System.Collections.Generic;

namespace Digiphoto.Lumen.Core.VsTest {
	
	[TestClass]
	public class RicercatoreSrvImplTest {

		private RicercatoreSrvImpl _impl;

		[TestInitialize]
		public void Init() {

			LumenApplication app = LumenApplication.Instance;
			app.avvia();

			this._impl = new RicercatoreSrvImpl();

			IServizio srv2 = app.creaServizio<IRicercatoreSrv>();

			// -------

			using( LumenEntities dbContext = new LumenEntities() ) {

			}
		}

		[TestMethod]
		public void ricercaTest() {

			ParamRicercaFoto param = new ParamRicercaFoto();
			param.giornataIniz = new DateTime( 2000, 1, 1 );
			param.giornataFine = new DateTime( 2299, 12, 31 );

			IList<Fotografia> ris = _impl.cerca( param );

			Console.WriteLine( ris.Count() );
		}

		[TestCleanup]
		public void Cleanup() {
			_impl.Dispose();
		}


	}
}

