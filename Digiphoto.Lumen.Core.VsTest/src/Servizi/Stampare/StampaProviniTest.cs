using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Vendere;
using Digiphoto.Lumen.Eventi;
using System.Threading;

namespace Digiphoto.Lumen.Core.VsTest.src.Servizi.Stampare
{
	[TestClass]
	public class StampaProviniTest : IObserver<StampatoMsg>
	{
		const int QUANTE = 5;

		private LumenApplication app;

		private VenditoreSrvImpl _impl;

		IDisposable ascoltami;

		[TestInitialize]
		public void initTest()
		{
			System.Diagnostics.Trace.WriteLine("INIZIO");
			app = LumenApplication.Instance;
			app.avvia();
			this._impl = (VenditoreSrvImpl)LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();

			IObservable<StampatoMsg> observable = app.bus.Observe<StampatoMsg>();
			ascoltami = observable.Subscribe(this);
		}

		[TestCleanup()]
		public void MyTestCleanup()
		{
			ascoltami.Dispose();
			LumenApplication.Instance.ferma();
		}

		[TestMethod]
		public void stampaProviniTest()
		{
			ParamStampaProvini param = new ParamStampaProvini();

			using (new UnitOfWorkScope(false))
			{
				ParamStampaProvini p = ricavaParamStampaProvini();
				p.numeroColonne = 6;
				p.numeroRighe = 5;
				p.macchiaProvini = false;

				LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include("fotografo")
										  select f).Take(QUANTE).ToList();

				_impl.aggiungiStampe(fotos,p);

				List<Fotografia> fotos2 = (from f in dbContext.Fotografie.Include("fotografo")
										   select f).Take(2 * QUANTE + 1).ToList();

				// Carico una stampa Provini
				ParamStampaProvini p1 = ricavaParamStampaProvini();
				p1.numeroColonne = 3;
				p1.numeroRighe = 4;
				p1.macchiaProvini = true;

				fotos2.RemoveRange(0, QUANTE + 1);

				_impl.aggiungiStampe(fotos2, p1);
			}

			while (!_elaborazioneTerminata)
			{
				Thread.Sleep(1000);
			}
			Assert.IsTrue(esitoStampa == EsitoStampa.Ok);
		}

		[TestMethod]
		public void stampaProviniFotoProviniTest()
		{

			using (new UnitOfWorkScope(false))
			{

				LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include("fotografo")
										  select f).Take(QUANTE).ToList();

				//Carico una stampa Provini
				ParamStampaProvini p = ricavaParamStampaProvini();
				p.numeroColonne = 5;
				p.numeroRighe = 5;
				p.macchiaProvini = false;

				_impl.aggiungiStampe(fotos, p);

				//Carico una stampa Foto
				ParamStampaFoto p2 = ricavaParamStampaFoto();

				CodaDiStampe c1 = new CodaDiStampe(p2, "doPDF v7");

				Fotografia foto = (from f in dbContext.Fotografie.Include("fotografo")
								   select f).Take(QUANTE + 1).ToList().Last();

				c1.EnqueueItem(new LavoroDiStampaFoto(foto, p2));
				c1.Start();

				List<Fotografia> fotos2 = (from f in dbContext.Fotografie.Include("fotografo")
										  select f).Take(2*QUANTE+1).ToList();

				// Carico una stampa Provini
				ParamStampaProvini p3 = ricavaParamStampaProvini();
				p3.numeroColonne = 3;
				p3.numeroRighe = 4;
				p3.macchiaProvini = true;
				
				fotos2.RemoveRange(0, QUANTE+1);

				_impl.aggiungiStampe(fotos2, p3);
				/*
				//*****************************
				//*****************************
				ParamStampaProvini p44 = ricavaParamStampaProvini();
				p44.numeroColonne = 3;
				p44.numeroRighe = 4;
				p44.macchiaProvini = true;

				CodaDiStampe c4 = new CodaDiStampe(p44, "doPDF v7");

				c4.EnqueueItem(new LavoroDiStampaProvini(fotos2, p44));
				c4.Start();
				//*****************************
				//*****************************
				*/

				//Carico una stampa Foto
				ParamStampaFoto p4 = ricavaParamStampaFoto();

				CodaDiStampe c2 = new CodaDiStampe(p4, "doPDF v7");

				Fotografia foto2 = (from f in dbContext.Fotografie.Include("fotografo")
								   select f).Take(2*QUANTE + 2).ToList().Last();

				c2.EnqueueItem(new LavoroDiStampaFoto(foto2, p4));
				c2.Start();
			}

			while (!_elaborazioneTerminata)
			{
				Thread.Sleep(1000);
			}
			Assert.IsTrue(esitoStampa == EsitoStampa.Ok);
		}
		  
		[TestMethod]
		public void stampaProviniTestAbort()
		{
			ParamStampaProvini param = new ParamStampaProvini();

			CodaDiStampe c1 = new CodaDiStampe(param, "doPDF v7");
			c1.Stop();

			using (new UnitOfWorkScope(false))
			{
				ParamStampaProvini p = ricavaParamStampaProvini();
				p.numeroColonne = 3;
				p.numeroRighe = 4;
				p.macchiaProvini = true;

				LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include("fotografo")
										  select f).Take(QUANTE).ToList();

				c1.EnqueueItem(new LavoroDiStampaProvini(fotos, p));
				// Accodo una stampa in modo da testare l'abort

				c1.Stop(Threading.PendingItemAction.AbortPendingItems);
				c1.Dispose();
			}
		}

		private ParamStampaProvini ricavaParamStampaProvini()
		{

			ParamStampaProvini p = new ParamStampaProvini();

			// Vediamo se esiste il formato
			// TODO : creare un nuovo attributo che identifica il formato carta come chiave naturale (per esempio A4 oppure 6x8)

			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

			FormatoCarta formato = Utilita.ottieniFormatoCarta(dbContext, "A5");
			formato.prezzo = 5;
			p.formatoCarta = formato;
			p.intestazione = "Prova";
			p.nomeStampante = "doPDF v7";
			p.numeroColonne = 5;
			p.numeroRighe = 6;

			// Qui non si deve spaccare
			Digiphoto.Lumen.Database.OrmUtil.forseAttacca<FormatoCarta>(dbContext.ObjectContext, "LumenEntities.FormatiCarta", ref formato);

			return p;
		}

		private ParamStampaFoto ricavaParamStampaFoto()
		{

			ParamStampaFoto p = new ParamStampaFoto();

			// Vediamo se esiste il formato
			// TODO : creare un nuovo attributo che identifica il formato carta come chiave naturale (per esempio A4 oppure 6x8)

			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

			FormatoCarta formato = Utilita.ottieniFormatoCarta(dbContext, "A5");
			formato.prezzo = 5;
			p.formatoCarta = formato;
			p.nomeStampante = "doPDF v7";

			// Qui non si deve spaccare
			Digiphoto.Lumen.Database.OrmUtil.forseAttacca<FormatoCarta>(dbContext.ObjectContext, "LumenEntities.FormatiCarta", ref formato);

			return p;
		}

		public void OnCompleted()
		{
			throw new NotImplementedException();
		}

		public void OnError(Exception error)
		{
			throw new NotImplementedException();
		}

		public void OnNext(StampatoMsg value)
		{
			System.Diagnostics.Trace.WriteLine("DESCRIZIONE: " + value.descrizione + " ESITO: " + value.lavoroDiStampa.esitostampa);
			esitoStampa = value.lavoroDiStampa.esitostampa;
			_elaborazioneTerminata = true;
		}

		public EsitoStampa esitoStampa
		{
			get;
			set;
		}


		public bool _elaborazioneTerminata
		{
			get;
			set;
		}
	}
}
