
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Servizi.Masterizzare.MyBurner;
using IMAPI2.Interop;
using Digiphoto.Lumen.Applicazione;
using System.Threading;
using Digiphoto.Lumen.Model;
using System.IO;
using Digiphoto.Lumen.Servizi.Stampare;

namespace Digiphoto.Lumen.Core.Test.Servizi.Stampare {
    [TestClass]
    public class StampantiInstallateSrvImplTest
    {
        private StampantiInstallateSrvImpl _impl = new StampantiInstallateSrvImpl();

        private LumenApplication app;

        [TestInitialize]
        public void initTest()
        {
            System.Diagnostics.Trace.WriteLine("INIZIO");
            app = LumenApplication.Instance;
            app.avvia();
            _impl.start();
        }

        [TestMethod]
        public void TestListaStampanti()
        {
            foreach (StampanteInstallata stampanteInstallata in _impl.stampantiInstallate)
            {
                System.Diagnostics.Trace.WriteLine("[Stampante]: " + stampanteInstallata.NomeStampante+ " "+ stampanteInstallata.PortaStampante);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            _impl.Dispose();
        }
    }
}
