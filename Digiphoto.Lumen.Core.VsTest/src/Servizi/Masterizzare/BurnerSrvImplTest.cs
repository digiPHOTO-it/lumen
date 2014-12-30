
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
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.Core.VsTest.Servizi.Stampare
{
    [TestClass]
    public class BurnerSrvImplTest
    {
        private BurnerSrvImpl _impl = new BurnerSrvImpl();

        private LumenApplication app;

        [TestInitialize]
        public void initTest()
        {
            System.Diagnostics.Trace.WriteLine("INIZIO");
            app = LumenApplication.Instance;
            app.avvia();
            _impl.start();
            //aggancio l'ascoltatore
            _impl.InviaStatoMasterizzazione += new BurnerSrvImpl.StatoMasterizzazioneEventHandler(statoMasterizzazione);
        }

        [TestMethod]
        public void TestListaMasterizzatori()
        {
            foreach (IDiscRecorder2 masterizzatore in _impl.listaMasterizzatori())
            {
                System.Diagnostics.Trace.WriteLine("[Volume]: " + masterizzatore.VolumePathNames.GetValue(0));
            }
        }

        [TestMethod]
        public void TestSetDiscRecorder()
        {
            _impl.setDiscRecorder(@"E:\");
        }

        [TestMethod]
        public void TestTestMedia()
        {
            _impl.setDiscRecorder(@"E:\");
            _impl.testMedia();
            _impl.etichetta = "Test";
            System.Diagnostics.Trace.WriteLine("Etichetta "+_impl.etichetta);
        }

        [TestMethod]
        public void TestAddFileToBurn()
        {
            using (LumenEntities dbContext = new LumenEntities())
            {
                foreach (Fotografia fot in dbContext.Fotografie.ToList<Fotografia>().Take( 10 ) )
                {
                    System.Diagnostics.Trace.WriteLine("[Foto Aggiunta per la Masterizzazione]: " + Configurazione.cartellaRepositoryFoto + Path.DirectorySeparatorChar + fot.nomeFile);
                    _impl.addFileToBurner(Configurazione.cartellaRepositoryFoto + Path.DirectorySeparatorChar + fot.nomeFile);
                }
            }
        }

        [TestMethod]
        public void TestBurning()
        {
            using (LumenEntities dbContext = new LumenEntities())
            {
                foreach (Fotografia fot in dbContext.Fotografie.ToList<Fotografia>().Take( 50 ) )
                {
                    System.Diagnostics.Trace.WriteLine("[Foto Aggiunta per la Masterizzazione]: " + Configurazione.cartellaRepositoryFoto + Path.DirectorySeparatorChar + fot.nomeFile);

					try {
						_impl.addFileToBurner( Configurazione.cartellaRepositoryFoto + Path.DirectorySeparatorChar + fot.nomeFile );
					} catch( Exception ) {
					}
					
                }
            }
			if(!_impl.testMedia())
			{
				return;
			}
            _impl.etichetta = "Masterizza";
            _impl.burning();
            while (!_elaborazioneTerminata)
            {
                Thread.Sleep(10000);
            }
            _impl.Dispose();
        }

        [TestMethod]
        public void TestFormatting()
        {
			if (!_impl.testMedia())
			{
				return;
			}
            _impl.formatting();
            while (!_elaborazioneTerminata)
            {
                Thread.Sleep(10000);
            }
        }

        private void statoMasterizzazione(object sender, BurnerMsg burnerMsg)
        {
            System.Diagnostics.Trace.WriteLine("");
            System.Diagnostics.Trace.WriteLine("[Capacity]: " + burnerMsg.capacity);
            System.Diagnostics.Trace.WriteLine("[Fase]: " + burnerMsg.fase);
            System.Diagnostics.Trace.WriteLine("[StatusMessage]: " + burnerMsg.statusMessage);
            System.Diagnostics.Trace.WriteLine("[Progress]: " + burnerMsg.progress);
            if (burnerMsg.fase == Fase.FormattazioneCompletata ||
               burnerMsg.fase == Fase.MasterizzazioneCompletata ||
               burnerMsg.fase == Fase.MasterizzazioneFallita ||
               burnerMsg.fase == Fase.FormattazioneFallita ||
                burnerMsg.fase == Fase.ErrorMedia
                //||
                //msg.fase == Fase.NessunaOperazione
               )
            {
                _elaborazioneTerminata = true;
            }
        
        }

        public bool _elaborazioneTerminata
        {
            get;
            set;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _impl.Dispose();
        }
    }
}
