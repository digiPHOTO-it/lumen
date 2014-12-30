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
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using Digiphoto.Lumen.Core.VsTest.Util;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Core.VsTest.src.Servizi.Stampare
{
	[TestClass]
	public class StampaProviniTest : IObserver<StampatoMsg>
	{
		const int QUANTE = 5;
		const long maxMem = 0;

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

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include("fotografo")
										  select f).Take(QUANTE).ToList();

				_impl.aggiungereStampe(fotos,p);

				List<Fotografia> fotos2 = (from f in dbContext.Fotografie.Include("fotografo")
										   select f).Take(2 * QUANTE + 1).ToList();

				// Carico una stampa Provini
				ParamStampaProvini p1 = ricavaParamStampaProvini();
				p1.numeroColonne = 3;
				p1.numeroRighe = 4;
				p1.macchiaProvini = true;

				fotos2.RemoveRange(0, QUANTE + 1);

				_impl.aggiungereStampe(fotos2, p1);
			}

			while (_contaElaborazioniTerminate != 2)
			{
				Thread.Sleep(1000);
			}
			Assert.IsTrue(esitoStampa == EsitoStampa.Ok);
		}

		[TestMethod]
		public void outOfMemoryStampaProviniTest() {

			FormuleMagiche.sonoNellaUI = false;

			ParamStampaProvini param = new ParamStampaProvini();
			const int quante = 30;
			const int totpag = 1;
			List<Fotografia> fotos;
			ParamStampaProvini p;

			long memoryPrima = Process.GetCurrentProcess().WorkingSet64;

			using( new UnitOfWorkScope( false ) ) {
				p = ricavaParamStampaProvini();
				p.numeroColonne = 6;
				p.numeroRighe = 5;
				p.macchiaProvini = false;

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				fotos = (from f in dbContext.Fotografie.Include( "fotografo" )
					     select f).Take( quante ).ToList();
			}

			// Stampo un tot. pagine di provini
			for( int pag=1; pag <= totpag; pag++ ) {
				
				_impl.aggiungereStampe( fotos, p );
			}

			while( _contaElaborazioniTerminate != totpag) {
				Thread.Sleep( 10000 );

				long memoryDopo = Process.GetCurrentProcess().WorkingSet64;
				long consumata = (memoryDopo - memoryPrima);

				// Se supero il massimo impostato, probabilmente il gc non sta pulendo.
				if( maxMem > 0 && consumata > maxMem )
					Assert.Fail( "Probabilmente si sta consumando troppa memoria: diff(MB)=" + consumata / 1024 );

			}
			Assert.IsTrue( esitoStampa == EsitoStampa.Ok );
		}

		[TestMethod]
		public void stampaProviniFotoProviniTest()
		{
			using (new UnitOfWorkScope(false))
			{

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include("fotografo")
										  select f).Take(QUANTE).ToList();

				//Carico una stampa Provini
				ParamStampaProvini p = ricavaParamStampaProvini();
				p.numeroColonne = 5;
				p.numeroRighe = 5;
				p.macchiaProvini = false;

				_impl.aggiungereStampe(fotos, p);

				//Carico una stampa Foto
				ParamStampaFoto p2 = ricavaParamStampaFoto();

				CodaDiStampe c1 = new CodaDiStampe(p2, Costanti.NomeStampantePdf);

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

				_impl.aggiungereStampe(fotos2, p3);
		
				//Carico una stampa Foto
				ParamStampaFoto p4 = ricavaParamStampaFoto();

				CodaDiStampe c2 = new CodaDiStampe(p4, Costanti.NomeStampantePdf);

				Fotografia foto2 = (from f in dbContext.Fotografie.Include("fotografo")
								   select f).Take(2*QUANTE + 2).ToList().Last();

				c2.EnqueueItem(new LavoroDiStampaFoto(foto2, p4));
				c2.Start();
			}

			while (_contaElaborazioniTerminate != 2)
			{
				Thread.Sleep(1000);
			}
			Assert.IsTrue(esitoStampa == EsitoStampa.Ok);
		}
		  
		[TestMethod]
		public void stampaProviniTestAbort()
		{
			ParamStampaProvini param = new ParamStampaProvini();

			CodaDiStampe c1 = new CodaDiStampe(param, Costanti.NomeStampantePdf);
			c1.Stop();

			using (new UnitOfWorkScope(false))
			{
				ParamStampaProvini p = ricavaParamStampaProvini();
				p.numeroColonne = 3;
				p.numeroRighe = 4;
				p.macchiaProvini = true;

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
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

			LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

			FormatoCarta formato = Utilita.ottieniFormatoCarta(dbContext, "A5");
			formato.prezzo = 5;
			p.formatoCarta = formato;
			p.intestazione = "Prova";
			p.nomeStampante = Costanti.NomeStampante;
			p.numeroColonne = 5;
			p.numeroRighe = 6;

			// Qui non si deve spaccare
			Digiphoto.Lumen.Database.OrmUtil.forseAttacca<FormatoCarta>( ref formato );

			return p;
		}

		private ParamStampaFoto ricavaParamStampaFoto()
		{

			ParamStampaFoto p = new ParamStampaFoto();

			// Vediamo se esiste il formato
			// TODO : creare un nuovo attributo che identifica il formato carta come chiave naturale (per esempio A4 oppure 6x8)

			LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

			FormatoCarta formato = Utilita.ottieniFormatoCarta(dbContext, "A5");
			formato.prezzo = 5;
			p.formatoCarta = formato;
			p.nomeStampante = Costanti.NomeStampantePdf;

			// Qui non si deve spaccare
			Digiphoto.Lumen.Database.OrmUtil.forseAttacca<FormatoCarta>( ref formato);

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
			if( esitoStampa != EsitoStampa.Errore ) // un errore invalida tutto
				esitoStampa = value.lavoroDiStampa.esitostampa;
			++_contaElaborazioniTerminate;
		}

		public EsitoStampa esitoStampa
		{
			get;
			set;
		}


		public int _contaElaborazioniTerminate
		{
			get;
			set;
		}
	}
}
