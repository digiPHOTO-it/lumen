
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Servizi.Masterizza.MyBurner;
using IMAPI2.Interop;
using Digiphoto.Lumen.Applicazione;
using System.Threading;
using Digiphoto.Lumen.Model;
using System.IO;

namespace Digiphoto.Lumen.Core.VsTest.Servizi.Masterizzare
{
    [TestClass]
    public class BurnerSrvImplTest : IObserver<BurnerMsg> 
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
            IObservable<BurnerMsg> observable = app.bus.Observe<BurnerMsg>();
            observable.Subscribe(this);
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
                foreach (Fotografia fot in dbContext.Fotografie.ToList<Fotografia>())
                {
                    System.Diagnostics.Trace.WriteLine("[Foto Aggiunta per la Masterizzazione]: " + app.configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile);
                    _impl.addFileToBurner(app.configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile);
                }
            }
        }

        [TestMethod]
        public void TestBurning()
        {
            using (LumenEntities dbContext = new LumenEntities())
            {
                foreach (Fotografia fot in dbContext.Fotografie.ToList<Fotografia>())
                {
                    System.Diagnostics.Trace.WriteLine("[Foto Aggiunta per la Masterizzazione]: " + app.configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile);
                    _impl.addFileToBurner(app.configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile);
                }
            }
            _impl.testMedia();
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
            _impl.formatting();
            while (!_elaborazioneTerminata)
            {
                Thread.Sleep(10000);
            }
        }


        [TestCleanup]
        public void Cleanup()
        {
            _impl.Dispose();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(BurnerMsg msg)
        {
            System.Diagnostics.Trace.WriteLine("[Capacity]: "+msg.capacity);
            System.Diagnostics.Trace.WriteLine("[Fase]: " + msg.fase);
            System.Diagnostics.Trace.WriteLine("[StatusMessage]: "+msg.statusMessage);
            System.Diagnostics.Trace.WriteLine("[Progress]: " + msg.progress);
            if(msg.fase == Fase.FormattazioneCompletata||
                msg.fase == Fase.MasterizzazioneCompletata||
                msg.fase == Fase.MasterizzazioneFallita ||
                msg.fase == Fase.FormattazioneFallita 
                //||
                //msg.fase == Fase.NessunaOperazione
                ){
                _elaborazioneTerminata = true;
            }
        }

        public bool _elaborazioneTerminata
        {
            get;
            set;
        }
    }
}
