using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Database;
using System.Linq;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Ricerca;
using System.Collections.Generic;

namespace Digiphoto.Lumen.Core.Test.Servizi.Ricerca {

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


				param.numeriFotogrammi =  "3, 5, 7," + f.numero;
				param.eventi = new Evento[] { ev };
				param.fotografi = new Fotografo [] { op };
				param.fasiDelGiorno = new FaseDelGiorno [] { FaseDelGiorno.Mattino, FaseDelGiorno.Sera };
				param.didascalia = "%POLLO%";  // Ricerca like


				IList<Fotografia> ris = _impl.cerca( param );
				Assert.IsTrue( ris.Count > 0 );
				Console.WriteLine( ris.Count );

			}
		}


		[TestMethod]
		public void ricercaPaginataTest() {

			ParamCercaFoto param = new ParamCercaFoto();
			param.giornataIniz = new DateTime( 2000, 1, 1 );
			param.giornataFine = new DateTime( 2299, 12, 31 );

			using( new UnitOfWorkScope() ) {

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				IList<Fotografia> ris = _impl.cerca( param );

				int totRecord = ris.Count;
				const int max = 17;  // Ampiezza della pagina uso un numero primo

				// Ora faccio dei cicli di paginazione da 5 alla volta
				ParamCercaFoto param2 = param.ShallowCopy();
				param2.paginazione = new Lumen.Util.Paginazione {
					take = max
				};

				for( int pag = 1; pag < 29; pag++ ) {
					param2.paginazione.skip = (pag - 1) * max;
					IList<Fotografia> ris2 = _impl.cerca( param2 );

					Assert.IsTrue( ris2.Count <= max );

					// Ora controllo che le liste corrispondano.
					for( int ii = 0; ii < ris2.Count; ii++ ) {
						Fotografia fAttesa = ris.ElementAt( ((pag - 1) * max) + ii );
						Fotografia fTrovata = ris2.ElementAt( ii );
						Assert.AreEqual( fAttesa, fTrovata );
					}
				}
				// Ora faccio un giro alla rovescio
				for( int pag = 31; pag > 0; pag-- ) {
					param2.paginazione.skip = (pag - 1) * param2.paginazione.take;
					IList<Fotografia> ris2 = _impl.cerca( param2 );

					// Ora controllo che le liste corrispondano.
					for( int ii = 0; ii < ris2.Count; ii++ ) {
						Fotografia fAttesa = ris.ElementAt( ((pag - 1) * max) + ii );
						Fotografia fTrovata = ris2.ElementAt( ii );
						Assert.AreEqual( fAttesa, fTrovata );
					}
				}
			}
		}



		[TestCleanup]
		public void Cleanup() {
			_impl.Dispose();
		}


	}
}

