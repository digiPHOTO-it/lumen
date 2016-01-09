using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.BarCode;
using Digiphoto.Lumen.Core.Database;
using System;
using System.IO;
using Digiphoto.Lumen.Servizi.Scaricatore;
using System.Reflection;
using System.Threading;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.Core.Test.Servizi.BarCode {
	[TestClass]
	public class BarCodeSrvImplTest : IObserver<ScaricoFotoMsg>
    {
        private const String BARCODE_VALUE = "0123456789012";

        private bool _puoiTogliereLaFlashCard;
        public bool _elaborazioneTerminata
        {
            get;
            set;
        }
        Fotografo _artista = null;

        private ScaricatoreFotoSrvImpl _scaricatoreImpl;
        private BarCodeSrvImpl _barCodeimpl;


        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            LumenApplication.Instance.avvia();
        }

        [TestInitialize]
		public void Init()
		{
            LumenApplication app = LumenApplication.Instance;
            IObservable<ScaricoFotoMsg> observable = app.bus.Observe<ScaricoFotoMsg>();
            observable.Subscribe(this);

            this._scaricatoreImpl = new ScaricatoreFotoSrvImpl();
            _scaricatoreImpl.start();

            // -------
            using (LumenEntities dbContext = new LumenEntities())
            {

                InfoFissa i = dbContext.InfosFisse.Single<InfoFissa>(f => f.id == "K");

                // Se hai fatto bene la configurazione, il fotografo artista deve sempre esistere
                _artista = dbContext.Fotografi.Single(f => f.id == Configurazione.ID_FOTOGRAFO_DEFAULT);
            }

            String doveSono = Assembly.GetExecutingAssembly().Location;

            string appPath = Path.GetDirectoryName(doveSono);
            string cartella = Path.Combine(appPath, "images");
            string nomeSrc = Directory.GetFiles(cartella, "barCode.jpg").ElementAt(0);

            FileInfo fiInfo = new FileInfo(nomeSrc);

            ParamScarica param = new ParamScarica();
            param.nomeFileSingolo = nomeSrc;
            param.cartellaSorgente = null;
            param.eliminaFilesSorgenti = false;

            param.flashCardConfig = new Config.FlashCardConfig(_artista);
            _scaricatoreImpl.scarica(param);

            while (!_puoiTogliereLaFlashCard)
            {
                Thread.Sleep(10000);
            }

            Console.Write("ok puoi togliere la flash card. Attendere elaborazione in corso ...");

            while (!_elaborazioneTerminata)
            {
                Thread.Sleep(10000);
            }

            Console.WriteLine("Ecco finito");


            this._barCodeimpl = new BarCodeSrvImpl();

            IRicercatoreSrv srv2 = app.creaServizio<IRicercatoreSrv>();

		}

		[TestMethod]
		public void searchBarCode()
		{

            String result = null;
			using (new UnitOfWorkScope(false))
			{
				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				List<Fotografia> fotos = dbContext.Fotografie.Where( f => f.nomeFile.Contains("barCode.jpg")).ToList<Fotografia>();
                foreach(Fotografia foto in fotos)
                {
                    result = _barCodeimpl.searchBarCode(foto);
                }

			}

			Assert.IsTrue(BARCODE_VALUE.Equals(result));
			
		}

        [TestMethod]
        public void applicaBarCodeDidascalia()
        {

            int trovati = 0;
            using (new UnitOfWorkScope(false))
            {
                LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
                List<Fotografia> fotos = dbContext.Fotografie.ToList<Fotografia>();

                trovati = _barCodeimpl.applicaBarCodeDidascalia(fotos);
            }

            Assert.IsTrue(trovati >= 1);

        }



        [TestCleanup]
		public void Cleanup()
		{
            _scaricatoreImpl.Dispose();
            _barCodeimpl.Dispose();
		}

        public void OnNext(ScaricoFotoMsg msg)
        {
            // ok è arrivato il messaggio.
            if (msg.fase == FaseScaricoFoto.FineScarico)
                _puoiTogliereLaFlashCard = true;

            if (msg.fase == FaseScaricoFoto.FineLavora)
            {
                _puoiTogliereLaFlashCard = true;
                _elaborazioneTerminata = true;
            }
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }
}
