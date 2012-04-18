
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
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.Core.VsTest.Servizi.Stampare
{
    [TestClass]
    public class StampantiAbbinateSrvImplTest
    {
        private StampantiAbbinateSrvImpl _impl = new StampantiAbbinateSrvImpl();

        private LumenApplication app;

        [TestInitialize]
        public void initTest()
        {
            System.Diagnostics.Trace.WriteLine("INIZIO");
            app = LumenApplication.Instance;
            app.avvia();
            _impl.start();
		}
        public void TestListaAbbinamenti()
        {
            IList<StampanteAbbinata> listStampantiAbbinate = _impl.listaStampantiAbbinate(ConfigurazioneUserConfigLumen.stampantiAbbinate);
            foreach (StampanteAbbinata stampanteAbbinata in listStampantiAbbinate)
            {
                System.Diagnostics.Trace.WriteLine("[Stampante]: " + stampanteAbbinata.StampanteInstallata.NomeStampante + " " + stampanteAbbinata.FormatoCarta.prezzo + " " + stampanteAbbinata.FormatoCarta.descrizione);
            }
        }

        [TestMethod]
        public void TestListaAbbinamentiToString()
        {
            IList<StampanteAbbinata> listStampantiAbbinate = _impl.listaStampantiAbbinate(ConfigurazioneUserConfigLumen.stampantiAbbinate);
            _impl.sostituisciAbbinamento(listStampantiAbbinate);
            System.Diagnostics.Trace.WriteLine("[Stampante]: " +_impl.listaStampantiAbbinateToString());
        }
        
        [TestCleanup]
        public void Cleanup()
        {
            _impl.Dispose();
        }
    }
}
