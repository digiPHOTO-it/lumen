using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Model;
using System.Data.Entity;
using Digiphoto.Lumen.Util;
using System.IO;
using Digiphoto.Lumen.Imaging.Wic;
using Digiphoto.Lumen.Core.Test.Util;
using Digiphoto.Lumen.Servizi.Vendere;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.Core.Database;
using System.Collections.Generic;
using System;

namespace Digiphoto.Lumen.Core.Test.Servizi.Ritoccare {

	[TestClass]
	public class CarrelloGalleryRitoccoTest {

		IFotoRitoccoSrv _ritoccoSrv;
		IVenditoreSrv _venditoreSrv;
		IFotoExplorerSrv _explorerSrv;
		IGestoreImmagineSrv _gestoreImmaginiSrv;

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

			_ritoccoSrv = LumenApplication.Instance.getServizioAvviato<IFotoRitoccoSrv>();

			_explorerSrv = LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();

			_venditoreSrv = LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();

			_gestoreImmaginiSrv = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
		}

		[TestCleanup]
		public void cleanupTest() {

			_gestoreImmaginiSrv.stop();

			_venditoreSrv.stop();

			_explorerSrv.stop();

			_ritoccoSrv.stop();
		}

		// Vedere ticket 494
		[TestMethod]
		public void testBug494() {

			Carrello c;
			IEnumerable<Guid> tantiIds;
			Fotografia fotoMod;

			// ---
			Console.Out.WriteLine( "1: trovo carrello" );
			using( LumenEntities entities = new LumenEntities() ) {
				c = entities.Carrelli.First( q => q.intestazione == "prova2" );
				Assert.IsTrue( c != null );
			}

			// ---
			using( new UnitOfWorkScope() ) {

				// ---
				Console.Out.WriteLine( "2: carico carrello nel servizio" );
				_venditoreSrv.caricareCarrello( c );
				tantiIds = _explorerSrv.caricaFotoDalCarrello();


				// ---
				ParamCercaFoto param = new ParamCercaFoto {
					idsFotografie = tantiIds.ToArray(),
					evitareJoinEvento = true
				};

				Console.Out.WriteLine( "3: carico tutte le foto del carrello nella gallery" );
				_explorerSrv.cercaFoto( param );
				fotoMod = _explorerSrv.fotografie.First();
				Assert.IsTrue( fotoMod != null );
			}

			using( new UnitOfWorkScope() ) {

				_ritoccoSrv.addCorrezione( fotoMod, new Sepia() );

				Console.Out.WriteLine( "4: applico correzione sepia" );
				_gestoreImmaginiSrv.salvaCorrezioniTransienti( fotoMod );
			}

			Console.Out.WriteLine( "5: fine" );
		}

	}
}
