﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Masterizzare;
using System.Threading;
using Digiphoto.Lumen.Servizi.Masterizzare.MyBurner;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.Core.Test.Servizi.Masterizzare {

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

            Console.WriteLine("FINE");
		}
        public void TestMasterizzaMasterizzatore()
        {
            using (LumenEntities dbContext = new LumenEntities())
            {
                foreach (Fotografia fot in dbContext.Fotografie.ToList<Fotografia>())
                {
                    _impl.addFotografia(fot);
                }
            }
            _impl.impostaDestinazione( MasterizzaTarget.Masterizzatore, @"E:\");
			BurnerSrvImpl burnerSrvImpl = new BurnerSrvImpl();
			if (burnerSrvImpl.testMedia())
			{
				_impl.masterizza();
				while (!_elaborazioneTerminata)
				{
					Thread.Sleep(10000);
				}
				Assert.IsTrue(_elaborazioneTerminata);
			}else{
				Assert.IsTrue(true);
			}
        }
        public void TestMasterizzaCartella()
        {
            using (LumenEntities dbContext = new LumenEntities())
            {
                foreach (Fotografia fot in dbContext.Fotografie.ToList<Fotografia>())
                {
                    System.Diagnostics.Trace.WriteLine("[Foto Aggiunta all'Abum per la Copia su Chiavetta]: "+fot.nomeFile);
                    _impl.addFotografia(fot);
                }
            }
            string strPathDesktop = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
            _impl.impostaDestinazione(MasterizzaTarget.Cartella, strPathDesktop + @"\Chiavetta");
			BurnerSrvImpl burnerSrvImpl = new BurnerSrvImpl();
			if (burnerSrvImpl.testMedia())
			{
				_impl.masterizza();

				while (!_elaborazioneTerminata)
				{
					Thread.Sleep(10000);
				}
				Assert.IsTrue(_elaborazioneTerminata);
			}else{
				Assert.IsTrue(true);
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

        public void OnNext(MasterizzaMsg msg)
        {
            System.Diagnostics.Trace.WriteLine("");
			System.Diagnostics.Trace.WriteLine("[TotFotoNonAggiunte]: " + msg.totFotoNonAggiunte);
			System.Diagnostics.Trace.WriteLine("[TotFotoAggiunte]: " + msg.totFotoAggiunte);
			System.Diagnostics.Trace.WriteLine("[Esito]: " + msg.esito);
			System.Diagnostics.Trace.WriteLine("[FotoAggiunta]: " + msg.fotoAggiunta);
			System.Diagnostics.Trace.WriteLine("[Fase]: " + msg.fase);
			System.Diagnostics.Trace.WriteLine("[Result]: " + msg.result);
			System.Diagnostics.Trace.WriteLine("[Progress]: " + msg.progress);

			if (msg.fase == Digiphoto.Lumen.Servizi.Masterizzare.Fase.CopiaCompletata)
			{
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
