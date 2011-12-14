using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Masterizza;
using System.Threading;
using Digiphoto.Lumen.Servizi.Masterizzare;

namespace Digiphoto.Lumen.Core.VsTest
{
    [TestClass]
    public class MasterizzaSrvImplTest : IObserver<MasterizzaMsg> 
    {

        private MasterizzaSrvImpl _impl = new MasterizzaSrvImpl();

        [TestInitialize]
        public void initTest()
        {
            Console.WriteLine("INIZIO");
            LumenApplication app = LumenApplication.Instance;
            app.avvia();
            IObservable<MasterizzaMsg> observable = app.bus.Observe<MasterizzaMsg>();
            observable.Subscribe(this);

            _impl.start();

            using (LumenEntities dbContext = new LumenEntities())
            {
                Album album = new Album();
                album.id = 1;
                album.titolo = "Test Masterizzazione";
                album.note = "Note Test Masterizzazione";
                album.timestamp = DateTime.Now;
                dbContext.Albums.AddObject(album);
            }
            Console.WriteLine("FINE");
        }

        [TestMethod]
        public void TestMasterizzaMasterizzatore()
        {
            using (LumenEntities dbContext = new LumenEntities())
            {
                foreach (Fotografia fot in dbContext.Fotografie.ToList<Fotografia>())
                {
                    _impl.Add(fot);
                }
            }
            _impl.impostaDestinazione(TipoDestinazione.MASTERIZZATORE, @"E:\");
			_impl.confermaVendita( new Decimal( 321 ) );
            while (!_elaborazioneTerminata)
            {
                Thread.Sleep(10000);
            }
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestMasterizzaCartella()
        {
            using (LumenEntities dbContext = new LumenEntities())
            {
                foreach (Fotografia fot in dbContext.Fotografie.ToList<Fotografia>())
                {
                    System.Diagnostics.Trace.WriteLine("[Foto Aggiunta all'Abum per la Copia su Chiavetta]: "+fot.nomeFile);
                    _impl.Add(fot);
                }
            }
            string strPathDesktop = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
            _impl.impostaDestinazione(TipoDestinazione.CARTELLA, strPathDesktop + @"\Chiavetta");
            _impl.confermaVendita( new Decimal(456) );

            while (!_elaborazioneTerminata)
            {
                Thread.Sleep(10000);
            }
            Assert.IsTrue(_elaborazioneTerminata);
        }

        [TestMethod]
        public void TestMasterizzaVendita()
        {
			Carrello carrello = _impl.confermaVendita( 5 );
            System.Diagnostics.Trace.WriteLine("[ID]: "+carrello.id);
            System.Diagnostics.Trace.WriteLine("[GIORNATA]: " + carrello.giornata);
            System.Diagnostics.Trace.WriteLine("[TEMPO]: " + carrello.tempo);
            System.Diagnostics.Trace.WriteLine("[TOTALE_A_PAGARE]: " + carrello.totaleAPagare);
            Assert.IsTrue(carrello.totaleAPagare==5.00m);
        }

        [TestMethod]
        public void TestMasterizzaAggiungiAlbum()
        {
            using (LumenEntities dbContext = new LumenEntities())
            {
                foreach (Album album in dbContext.Albums.ToList<Album>())
                {
                    _impl.addAlbum(album);
                }
            }
            string strPathDesktop = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
            _impl.impostaDestinazione(TipoDestinazione.CARTELLA, strPathDesktop + @"\Chiavetta");
			_impl.confermaVendita( new Decimal( 123 ) );

            while (!_elaborazioneTerminata)
            {
                Thread.Sleep(10000);
            }
            Assert.IsTrue(_elaborazioneTerminata);
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

        public void OnNext(MasterizzaMsg msg)
        {
            if (msg.fase == Fase.MasterizzazioneCompletata)
                _elaborazioneTerminata = true;
        }

        public bool _elaborazioneTerminata
        {
            get;
            set;
        }
    }
}
