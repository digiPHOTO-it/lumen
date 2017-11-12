using Digiphoto.Lumen.Servizi.Vendere;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Stampare;
using System.Linq;
using System.Collections.Generic;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Servizi.Masterizzare;
using System.IO;
using Digiphoto.Lumen.Servizi.Reports;
using System.Diagnostics;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Core.Test.Util;

namespace Digiphoto.Lumen.Core.VsTest {


	/// <summary>
	///This is a test class for VenditoreSrvImplTest and is intended
	///to contain all VenditoreSrvImplTest Unit Tests
	///</summary>
	[TestClass()]
	public class VenditoreSrvImplTest : IObserver<Messaggio> {

		const int QUANTE = 3;

		int contaStampate = 0;

		#region ButtaSu

		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void classInitialize( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}

		[ClassCleanup()]
		public static void classCleanup() {
			LumenApplication.Instance.ferma();
		}

		IDisposable ascoltami;

		[TestInitialize()]
		public void MyTestInitialize() {

			this._impl = (VenditoreSrvImpl)LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();

			IObservable<Messaggio> observable = LumenApplication.Instance.bus.Observe<Messaggio>();
			ascoltami = observable.Subscribe( this );
		}

		[TestCleanup()]
		public void MyTestCleanup() {
			ascoltami.Dispose();
			LumenApplication.Instance.ferma();
		}


		#endregion

		private VenditoreSrvImpl _impl;

		[TestMethod]
		public void codaDiStampaTestJoin() {

			ParamStampaFoto param = new ParamStampaFoto();

			// Istanzio una coda di stampa e la chiudo
			CodaDiStampe c1 = new CodaDiStampe( param, "coda1" );
			c1.Start();
			c1.Stop();
			c1.Dispose();

			CodaDiStampe c2 = new CodaDiStampe( param, "coda2" );
			c2.Dispose();

			CodaDiStampe c3 = new CodaDiStampe( param, "coda3" );
			c3.Stop();
			c3.Stop();
			c3.Dispose();
		}

		[TestMethod]
		public void codaDiStampeConAbort() {

			ParamStampaFoto param = new ParamStampaFoto();

			CodaDiStampe c3 = new CodaDiStampe( param, "doPDF v7" );
			c3.Stop();

			c3.EnqueueItem( new LavoroDiStampaFoto( new Fotografia(), new ParamStampaFoto() ) );
			// Accodo una stampa in modo da testare l'abort

			c3.Stop( Threading.PendingItemAction.AbortPendingItems );
			c3.Dispose();
		}



		[TestMethod]
		public void vendiFotoTest() {


			using( new UnitOfWorkScope( false ) ) {

				_impl.creareNuovoCarrello();


				ParamStampaFoto p = ricavaParamStampa();

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include( "fotografo" )
										  select f).Take( QUANTE ).ToList();

				if( fotos.Count == QUANTE ) {

					contaStampate = 0;

					_impl.aggiungereStampe( fotos, p );

					_impl.aggiungereMasterizzate( fotos );
					_impl.setDatiDischetto( MasterizzaTarget.Cartella, Path.GetTempPath(), 7m );

					Assert.IsFalse( _impl.carrello.venduto );

					Assert.IsTrue( _impl.isPossibileSalvareCarrello );
					Assert.IsTrue( _impl.isPossibileVendereCarrello );
					Assert.IsTrue( _impl.isPossibileModificareCarrello );

					_impl.vendereCarrello();

					Assert.IsTrue( _impl.carrello.venduto );
					Assert.IsTrue( _impl.carrello.totaleAPagare == 15 + 7 );
				}
			}



			// TODO Qui non funziona e non capisco perché.
			// Mi va in fail durante la sleep
			//while( !venditaCompletata ) {
			//    System.Threading.Thread.Sleep( 6000 );
			//}

			//			_impl.stop();


			Console.WriteLine( "FINITO" );
		}


		[TestMethod]
		public void vendiFotoStampaDirettaTest()
		{
			using (new UnitOfWorkScope(false))
			{
				
				// Ho bisogno di una stampante con il formato carta A5
				StampantiAbbinateCollection stampantiAbbinate = StampantiAbbinateUtil.deserializza( Configurazione.UserConfigLumen.stampantiAbbinate );
				

				// TODO  il test presume che ci sia una stampante abbinata con la carta A5.
				//       se non esiste, creare un abbinamento ad hoc.

				_impl.carrello.intestazione = VenditoreSrvImpl.INTESTAZIONE_STAMPA_RAPIDA;

				ParamStampaFoto p = ricavaParamStampa();

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include("fotografo")
										  select f).Take(QUANTE).ToList();

				contaStampate = 0;

				_impl.aggiungereStampe(fotos, p);

				_impl.vendereCarrello();

				Assert.IsTrue(_impl.carrello.venduto);
				Assert.IsTrue(_impl.carrello.totaleAPagare > 0);
			}

			Console.WriteLine("FINITO");
		}

		private ParamStampaFoto ricavaParamStampa() {

			ParamStampaFoto p = new ParamStampaFoto();

			// Vediamo se esiste il formato
			// TODO : creare un nuovo attributo che identifica il formato carta come chiave naturale (per esempio A4 oppure 6x8)

			LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

			FormatoCarta formato = Utilita.ottieniFormatoCarta( dbContext, "A5" );
			formato.prezzo = 5;
			p.formatoCarta = formato;

			// Qui non si deve spaccare
			Digiphoto.Lumen.Database.OrmUtil.forseAttacca<FormatoCarta>( ref formato );

			return p;
		}



		public IDisposable Subscribe( IObserver<Messaggio> observer ) {
			throw new NotImplementedException();
		}

		public void OnCompleted() {
			throw new NotImplementedException();
		}

		public void OnError( Exception error ) {
			throw new NotImplementedException();
		}

		public void OnNext( Messaggio msg ) {

			if( msg is StampatoMsg )
				++contaStampate;

			if( msg is MasterizzaMsg ) {
				if( ((MasterizzaMsg)msg).fase == Fase.CopiaCompletata ) {
				}
			}
		}

		bool venditaCompletata {
			get {
				return this._impl.carrello.venduto;
				// return contaStampate == QUANTE && this._impl.masterizzaSrv.isCompletato;
			}
		}


		[TestMethod]
		public void queryVenditeConTotali() {

			using( new UnitOfWorkScope() ) {
			
				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				DateTime dataIniz = new DateTime( 2012, 04, 01 );
				DateTime dataFine = new DateTime( 2012, 04, 30 );


				var porc = from c in dbContext.Carrelli.Include( "righeCarrello" )
						   from r in c.righeCarrello.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA )
						   select new { c, r }
						   ;

				var porc2 = from d in porc
							group d by new {
								d.c.giornata,
								d.r.formatoCarta.descrizione
							} into grp
							select new {
								gg = grp.Key.giornata,
								fc = grp.Key.descrizione,
								fogli = grp.Sum( a => a.r.totFogliStampati )
							};


				foreach( var item in porc2 ) {
					Trace.WriteLine( item );
				}




				var query =
						from c in dbContext.Carrelli
							.Include( "righeCarrello" )
						where c.giornata >= dataIniz && c.giornata <= dataFine
						orderby c.giornata descending
						select c;


				RigaReportVendite riga = null;

				foreach( var carrello in query ) {

					if( riga == null || !riga.giornata.Equals( carrello.giornata ) )
						riga = new RigaReportVendite {
							giornata = carrello.giornata
						};

					foreach( RigaCarrello rc in carrello.righeCarrello ) {

						if( rc.isTipoStampa ) {
						}

						if( rc.isTipoMasterizzata ) {
						}

					}

					var qq = carrello.righeCarrello.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA )
							 .GroupBy( t => t.formatoCarta.descrizione )
							 .Select( r => new {
								 ff = r.Key,
								 tot = r.Sum( t => t.quantita )
							 } );


					foreach( var qq2 in qq ) {
						Trace.WriteLine( qq2 );
					}
				}

			}

		}

		[TestMethod]
		public void spaccatoIncassiFotografiTest() {

			using( new UnitOfWorkScope() ) {

				FormatoCarta formatoCarta = UnitOfWorkScope.currentDbContext.FormatiCarta.First();

				ParamStampaFoto paramStampa = new ParamStampaFoto {
					nomeStampante = "doPDF v7",
					formatoCarta = formatoCarta,
				};



				_impl.creareNuovoCarrello();

				_impl.carrello.prezzoDischetto = (decimal)27.9;

				// Carico 3 fotografi tra quelli che hanno delle foto
				var idFotografi = UnitOfWorkScope.currentDbContext.Fotografie.Select( f => f.fotografo ).Distinct().Take( 3 );
				Fotografo [] arrayF = idFotografi.ToArray();
				if( idFotografi.Count() != 3 )
					return;

				string par1 = arrayF[0].id;
				var fotos1 = UnitOfWorkScope.currentDbContext.Fotografie.Where( f => f.fotografo.id == par1 ).Take( 2 ).ToList();
				string par2 = arrayF[1].id;
				var fotos2 = UnitOfWorkScope.currentDbContext.Fotografie.Where( f => f.fotografo.id == par2 ).Take( 3 ).ToList();
				string par3 = arrayF[2].id;
				var fotos3 = UnitOfWorkScope.currentDbContext.Fotografie.Where( f => f.fotografo.id == par3 ).Take( 4 ).ToList();

				paramStampa.numCopie = 1;
				_impl.aggiungereStampe( fotos1, paramStampa );

				paramStampa.numCopie = 2;
				_impl.aggiungereStampe( fotos2, paramStampa );

				paramStampa.numCopie = 3;
				_impl.aggiungereStampe( fotos3, paramStampa );

				_impl.aggiungereMasterizzate( fotos1 );
				_impl.aggiungereMasterizzate( fotos2 );
				_impl.aggiungereMasterizzate( fotos3 );

				string msgEsito = _impl.salvareCarrello();
				bool esito = (msgEsito == null);
				Assert.IsTrue( esito );

				Carrello cc = _impl.carrello;


				decimal soldi1 = (fotos1.Count() * 1 * formatoCarta.prezzo);
				decimal soldi2 = (fotos2.Count() * 2 * formatoCarta.prezzo);
				decimal soldi3 = (fotos3.Count() * 3 * formatoCarta.prezzo);
				decimal soldi = soldi1 + soldi2 + soldi3 + (decimal)27.9;

				// Ora faccio un pò di verifiche
				Assert.IsTrue( cc.totaleAPagare == soldi );


				// Il totale da pagare, deve essere la somma degli incassi dei fotografi
				decimal somma = cc.incassiFotografi.Sum( ii => ii.incasso );
				Assert.IsTrue( cc.totaleAPagare == somma );

				// La somma dei ...DI CUI.. deve essere uguale al totale
				decimal ima = cc.incassiFotografi.Sum( ii => ii.incassoMasterizzate );
				decimal ist = cc.incassiFotografi.Sum( ii => ii.incassoStampe );
				Assert.IsTrue( ima + ist == somma );

				Assert.IsTrue( ima == cc.prezzoDischetto );

			}
		}

	}
}
