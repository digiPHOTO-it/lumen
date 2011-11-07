using Digiphoto.Lumen.Servizi.EliminaFotoVecchie;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Database;
using System.Linq;
using System.Text;
using System.IO;
using System.Transactions;
using System.Diagnostics;
using Digiphoto.Lumen.Properties;

namespace Digiphoto.Lumen.Core.VsTest
{
    
    
    /// <summary>
    ///This is a test class for EliminaFotoVecchieSrvImpTest and is intended
    ///to contain all EliminaFotoVecchieSrvImpTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EliminaFotoVecchieSrvImplTest
    {

        private EliminaFotoVecchieSrvImpl _impl = new EliminaFotoVecchieSrvImpl();

		Fotografo _mario = null;

		private int giorni = 0;

		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void inizializzaClasse( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}


        [TestInitialize()]
        public void initTest()
        {
			Console.WriteLine("INIZIO");
			LumenApplication app = LumenApplication.Instance;
			giorni = Settings.Default.giorniDeleteFoto;
			Console.WriteLine("GIORNI " +giorni);

			using (LumenEntities dbContext = new LumenEntities()) {
				_mario = Utilita.ottieniFotografoMario(dbContext);
			}
            
            ParamEliminaFotoVecchieSrv paramEliminaFotoVecchie = new ParamEliminaFotoVecchieSrv();
			paramEliminaFotoVecchie.fotografo = _mario;
            paramEliminaFotoVecchie.dataIntervallo = DateTime.Now;
			_impl = new EliminaFotoVecchieSrvImpl();
			//_impl.start();
            _impl.init(paramEliminaFotoVecchie);
           
        }

        [TestMethod()]
        public void diChiSonoQuesteFoto()
        {
			Assert.IsTrue(_impl.diChiSonoQuesteFoto().id.Equals("ROSSIMARIO"));
        }

         [TestMethod()]
        public void getListaCartelleDaEliminare()
        {
			if (_impl.getListaCartelleDaEliminare().Count()==0)
			{
				Assert.IsTrue(true);
			}
			else
			{
				Assert.IsTrue(_impl.getListaCartelleDaEliminare()[0].Equals(@"C:\ProgramData\digiPHOTO\Lumen\Foto\"+DateTime.Now.AddDays(-giorni).ToString("yyyy-MM-dd")+@"\ROSSIMARIO"));
			}
        }

		 [TestMethod()]
		 public void getElimina()
		 {
			 if (_impl.getListaCartelleDaEliminare().Count() == 0)
			 {
				 Console.WriteLine("Lista Nulla");
				 Assert.IsTrue(true);
			 }
			 else
			 {
				 Console.WriteLine("Lista NON Nulla");
				 IList<String> list = _impl.getListaCartelleDaEliminare();
				 foreach(String path in list){
					 Trace.WriteLine(path);
					_impl.elimina(path);
				 }
			 }
		 }

		 [TestMethod()]
		 public void testEliminaRecord()
		 {
			 _impl.eliminaRecord();
			 Assert.IsTrue(true);
		 }
    }
}
