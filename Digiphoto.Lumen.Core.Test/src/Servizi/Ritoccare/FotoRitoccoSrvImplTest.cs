using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Model;
using System.Data.Entity;
using Digiphoto.Lumen.Util;
using System.IO;
using Digiphoto.Lumen.Imaging.Wic;
using Digiphoto.Lumen.Core.Test.Util;

namespace Digiphoto.Lumen.Core.Test.Servizi.Ritoccare {

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
			using( LumenEntities entities = new LumenEntities() ) {

				Fotografia[] fotos = new Fotografia[2];
				fotos[0] = Costanti.findUnaFotografiaRandom( entities );
				fotos[1] = Costanti.findUnaFotografiaRandom( entities );
	
				Fotografia [] modificate = _impl.modificaConProgrammaEsterno( fotos.ToArray() );

				Assert.IsTrue( modificate.Length > 0 );
			}
		}

		[TestMethod]
		public void clonaFotoTest()
		{
			// Carico 3 foto da clonare
			using (LumenEntities dbContext = new LumenEntities())
			{
				Fotografia[] fotos = dbContext.Fotografie.Take(3).ToArray<Fotografia>();

				_impl.clonaFotografie(fotos);

				foreach(Fotografia foto in fotos)
				{
					string pathCartellaFoto = PathUtil.decidiCartellaFoto(foto);

					string pathCartellaProvino = PathUtil.decidiCartellaProvini(foto);

					string pathCartellaRisultante = PathUtil.decidiCartellaRisultanti(foto);

					// Verifico che il file sia stato copiato su disco
					int countFile = Directory.EnumerateFiles(pathCartellaFoto, Path.GetFileNameWithoutExtension(foto.nomeFile) + "_CLONE_[*.*").Count();

					// Verifico che il provino sia stato copiato su disco
					int countProvino = Directory.EnumerateFiles(pathCartellaProvino, Path.GetFileNameWithoutExtension(foto.nomeFile) + "_CLONE_[*.*").Count();

					// Verifico che la risultante sia stata copiato su disco
					if(foto.imgRisultante != null){
						int countRisultante = Directory.EnumerateFiles(pathCartellaRisultante, Path.GetFileNameWithoutExtension(foto.nomeFile) + "_CLONE_[*.*").Count();
					}

					// Verifico il salvataggio sul db
					int count = dbContext.Fotografie.Where(f=> f.numero == foto.numero).Count<Fotografia>();

					Assert.IsTrue(count >= 2 && countFile >0 && countProvino > 0 && countProvino > 0);
				}
				dbContext.SaveChanges();
			}
		}
	}
}
