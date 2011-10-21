using Digiphoto.Lumen.Servizi.EliminaFotoVecchie;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.src.Database;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Database;
using System.Linq;
using System.Text;
using System.IO;
using System.Transactions;

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

        private Fotografo _fotografo;

        [TestInitialize()]
        public void initTest()
        {
            LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
            _fotografo = objContext.Fotografi.Single<Fotografo>(ff => ff.id == "ROSSIMARIO");
            LumenApplication app = LumenApplication.Instance;
            app.avvia();
            ParamEliminaFotoVecchieSrv paramEliminaFotoVecchie = new ParamEliminaFotoVecchieSrv();
            paramEliminaFotoVecchie.fotografo = _fotografo;
            paramEliminaFotoVecchie.dataIntervallo = DateTime.Now;
            _impl.init(paramEliminaFotoVecchie);
            _impl = new EliminaFotoVecchieSrvImpl();
            _impl.start();
        }

        [TestMethod()]
        public void diChiSonoQuesteFoto()
        {
            Assert.IsTrue(_impl.diChiSonoQuesteFoto().Equals("ROSSIMARIO"));
        }

         [TestMethod()]
        public void getListaCartelleDaEliminare()
        {
            Assert.IsTrue(_impl.getListaCartelleDaEliminare()[0].Equals(@"C:\ProgramData\digiPHOTO\Lumen\Foto\2011-10-21\ROSSIMARIO"));
        }
    }
}
