
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
using Digiphoto.Lumen.Core.Database;

namespace Digiphoto.Lumen.Core.Test.Servizi.Stampare {
    [TestClass]
    public class StampantiAbbinateSrvImplTest
    {
        private StampantiAbbinateCollection _impl = null;

        private LumenApplication app;

        [TestInitialize]
        public void initTest()
        {
            System.Diagnostics.Trace.WriteLine("INIZIO");
            app = LumenApplication.Instance;
            app.avvia();
		}
        public void TestListaAbbinamenti()
        {
			using (new UnitOfWorkScope(false))
			{
				_impl = new StampantiAbbinateCollection( Configurazione.UserConfigLumen.stampantiAbbinate );

				foreach( StampanteAbbinata stampanteAbbinata in _impl )
				{
					System.Diagnostics.Trace.WriteLine("[Stampante]: " + stampanteAbbinata.StampanteInstallata.NomeStampante + " " + stampanteAbbinata.FormatoCarta.prezzo + " " + stampanteAbbinata.FormatoCarta.descrizione);
				}
			}

			Assert.IsTrue( String.IsNullOrEmpty( Configurazione.UserConfigLumen.stampantiAbbinate ) && _impl.Count == 0 );


			if( Configurazione.UserConfigLumen.stampantiAbbinate != null ) {
				Assert.IsTrue( Configurazione.UserConfigLumen.stampantiAbbinate.Length > 0 && _impl.Count > 0 );
			}
        }

        [TestMethod]
        public void TestListaAbbinamentiToString()
        {
			using (new UnitOfWorkScope(false))
			{
				_impl = new StampantiAbbinateCollection( Configurazione.UserConfigLumen.stampantiAbbinate );
				System.Diagnostics.Trace.WriteLine( "[Stampante]: " + _impl.serializzaToString() );
			}
        }
        
        [TestCleanup]
        public void Cleanup()
        {
        }
    }
}
