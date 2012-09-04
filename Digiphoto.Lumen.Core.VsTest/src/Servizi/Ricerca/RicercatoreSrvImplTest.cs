using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Database;
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

			IRicercatoreSrv srv2 = app.creaServizio<IRicercatoreSrv>();

			// -------
			using( LumenEntities dbContext = new LumenEntities() ) {

			}
		}

		[TestMethod]
		public void ricercaTest() {

			ParamCercaFoto param = new ParamCercaFoto();
			param.giornataIniz = new DateTime( 2000, 1, 1 );
			param.giornataFine = new DateTime( 2299, 12, 31 );

			using( LumenEntities dbContext = new LumenEntities() ) {

				Evento ev = dbContext.Eventi.First();
				Fotografo op = dbContext.Fotografi.First();

				Fotografia f = dbContext.Fotografie.First();
				f.evento = ev;
				f.fotografo = op;
				f.faseDelGiorno = (short)FaseDelGiorno.Mattino;
				f.didascalia = "W IL POLLO ARROSTO";
	
				dbContext.SaveChanges();


				param.numeriFotogrammi = new int [] { 3, 5, 7, f.numero };
				param.eventi = new Evento[] { ev };
				param.fotografi = new Fotografo [] { op };
				param.fasiDelGiorno = new FaseDelGiorno [] { FaseDelGiorno.Mattino, FaseDelGiorno.Sera };
				param.didascalia = "POLLO";  // deve trovarla ugualmente


				IList<Fotografia> ris = _impl.cerca( param );
				Assert.IsTrue( ris.Count > 0 );
				Console.WriteLine( ris.Count );

			}
		}

		[TestCleanup]
		public void Cleanup() {
			_impl.Dispose();
		}


	}
}

