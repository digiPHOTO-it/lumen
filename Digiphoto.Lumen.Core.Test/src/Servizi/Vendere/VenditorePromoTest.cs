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
	/// Per poter svolgere correttamente i test, occorre dichiarare 3 formati carta
	/// uno piccolo, uno medio, uno grande
	/// del rispettivo prezzo di : 
	/// 4,5 5, 9 euro.
	/// altrimenti i calcoli non funzionano.
	/// 
	///</summary>
	[TestClass()]
	public class VenditorePromoTest : IObserver<Messaggio> {

		const int QUANTE = 3;

		const Decimal PRZ_FRM_PIC = (decimal)4.5;
		const Decimal PRZ_FRM_MED = (decimal)5;
		const Decimal PRZ_FRM_BIG = (decimal)9;
		const Decimal PRZ_FRM_FILE = (decimal)3.5;

		int contaStampate = 0;

		#region ButtaSu

		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void classInitialize( TestContext testContext ) {

			LumenApplication.Instance.avvia();

			//
			//

			using( new UnitOfWorkScope( false ) ) {

				// Controllo che le condizioni per il test siano verificate (formati carta e prezzi)
				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

				FormatoCarta frmPiccolo = dbContext.FormatiCarta.FirstOrDefault( fc => fc.grandezza == "P" && fc.prezzo == PRZ_FRM_PIC );
				if( frmPiccolo == null )
					throw new InvalidOperationException( "Manca il formato carta piccolo da " + PRZ_FRM_PIC + " euro" );

				FormatoCarta frmMedio = dbContext.FormatiCarta.FirstOrDefault( fc => fc.grandezza == "M" && fc.prezzo == PRZ_FRM_MED );
				if( frmMedio == null )
					throw new InvalidOperationException( "Manca il formato carta medio da " + PRZ_FRM_MED + " euro" );

				FormatoCarta frmGrande = dbContext.FormatiCarta.FirstOrDefault( fc => fc.grandezza == "G" && fc.prezzo == PRZ_FRM_BIG );
				if( frmGrande == null )
					throw new InvalidOperationException( "Manca il formato carta grande da " + PRZ_FRM_BIG + " euro" );

				ProdottoFile prodFile = dbContext.ProdottiFile.FirstOrDefault( pf => pf.prezzo == PRZ_FRM_FILE );
				if( prodFile == null )
					throw new InvalidOperationException( "Manca il prodotto file da " + PRZ_FRM_FILE + " euro" );
			}
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

		private ParamStampaFoto ricavaParamStampa( string grandezza ) {

			ParamStampaFoto p = new ParamStampaFoto();
			p.nomeStampante = "doPDF 9";

			LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

			FormatoCarta formato = dbContext.FormatiCarta.FirstOrDefault( fc => fc.grandezza == grandezza );
			p.formatoCarta = formato;

			return p;
		}

		[TestMethod, TestCategory( "Promozioni" )]
		public void PromoAncheFileTest() {

			using( new UnitOfWorkScope( false ) ) {

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

				var psfsf = dbContext.Promozioni.OfType<PromoStessaFotoSuFile>().SingleOrDefault( af => af.attiva == true );
				if( psfsf == null )
					throw new InvalidOperationException( "La PromoStessaFotoSuFile non è attiva. Impossibile testare" );

				_impl.creareNuovoCarrello();

				ParamStampaFoto p = ricavaParamStampa( "P" );

				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include( "fotografo" )
										  select f).Take( 4 ).ToList();

				Assert.IsTrue( fotos.Count == 4 );

				contaStampate = 0;

				// Prendo solo i primi 3 elementi
				_impl.aggiungereStampe( fotos.Take( 3 ), p );
				_impl.aggiungereMasterizzate( new[] { fotos[0], fotos[3] } );   // Solo una foto in promo
				_impl.carrello.prezzoDischetto = 123;

				Assert.IsFalse( _impl.carrello.venduto );

				Assert.IsTrue( _impl.isPossibileSalvareCarrello );
				Assert.IsTrue( _impl.isPossibileVendereCarrello );
				Assert.IsTrue( _impl.isPossibileModificareCarrello );

				var newCart = _impl.CalcolaPromozioni();

				Assert.IsTrue( newCart.totaleAPagare == (decimal)((3 * 4.5) + (1 * 1) + (1 * 3.5)) );
			}

			Console.WriteLine( "FINITO" );
		}

		[TestMethod, TestCategory( "Promozioni" )]
		public void PromoCalTest1() {
			PromoCalcGenerico( 5, 5, 5, (decimal)83.5 );
		}
		[TestMethod, TestCategory( "Promozioni" )]
		public void PromoCalTest2() {
			PromoCalcGenerico( 0, 1, 0, (decimal)5 );
		}
		[TestMethod, TestCategory( "Promozioni" )]
		public void PromoCalTest3() {
			PromoCalcGenerico( 0, 4, 12, (decimal)110 );
		}
		[TestMethod, TestCategory( "Promozioni" )]
		public void PromoCalTest4() {
			PromoCalcGenerico( 0, 4, 11, (decimal)105 );
		}
		[TestMethod, TestCategory( "Promozioni" )]
		public void PromoCalTest5() {
			PromoCalcGenerico( 2, 2, 2, (decimal)32.5 );
		}
		[TestMethod, TestCategory( "Promozioni" )]
		public void PromoCalTest6() {
			PromoCalcGenerico( 1, 6, 0, (decimal)29.5 );
		}

		[TestMethod, TestCategory( "Promozioni" )]
		public void PromoFile6x5Test() {

			using( new UnitOfWorkScope( false ) ) {

				decimal prezzoPromoDesiderato = (decimal)(3.5 * 5);
				_impl.creareNuovoCarrello();

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include( "fotografo" )
										  select f).Take( 6 ).ToList();

				// Controllo che ci siano abbastanza foto nel database
				Assert.IsTrue( fotos.Count == 6 );

				_impl.aggiungereMasterizzate( fotos );

				_impl.ricalcolaTotaleCarrello();
				var totPagarePrima = _impl.carrello.totaleAPagare;

				Carrello cart = _impl.CalcolaPromozioni( true );

				var totPagareDopo = cart.totaleAPagare;

				Assert.AreEqual( totPagareDopo, prezzoPromoDesiderato );
			}

		}

		private void PromoCalcGenerico( int qtaP, int qtaM, int qtaG, decimal prezzoPromoDesiderato ) {

			using( new UnitOfWorkScope( false ) ) {

				_impl.creareNuovoCarrello();

				ParamStampaFoto paramP = ricavaParamStampa( "P" );
				ParamStampaFoto paramM = ricavaParamStampa( "M" );
				ParamStampaFoto paramG = ricavaParamStampa( "G" );

				Dictionary<string, int> mappa = new Dictionary<string, int> {
					{ "P", qtaP },
					{ "M", qtaM },
					{ "G", qtaG }
				};
				int totFoto = mappa.Values.Sum();

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include( "fotografo" )
										  select f).Take( totFoto ).ToList();

				// Controllo che ci siano abbastanza foto nel database
				Assert.IsTrue( fotos.Count == totFoto );

				contaStampate = 0;

				int qq = 0;
				_impl.aggiungereStampe( fotos.Skip( qq ).Take( mappa["P"] ), paramP );
				qq += mappa["P"];
				_impl.aggiungereStampe( fotos.Skip( qq ).Take( mappa["M"] ), paramM );
				qq += mappa["M"];
				_impl.aggiungereStampe( fotos.Skip( qq ).Take( mappa["G"] ), paramG );

				_impl.ricalcolaTotaleCarrello();
				var totPagarePrima = _impl.carrello.totaleAPagare;

				Carrello cart = _impl.CalcolaPromozioni( true );

				var totPagareDopo = cart.totaleAPagare;

				Assert.AreEqual( totPagareDopo, prezzoPromoDesiderato );

			}



			Console.WriteLine( "FINITO" );
		}

		/// <summary>
		/// Questo test era per risolvere un bug di quando metto a carrello
		/// più copie della stessa foto. Sbagliava il prezzo finale.
		/// </summary>
		[TestMethod, TestCategory( "Promozioni" )]
		public void PromoCalTest7() {

			using( new UnitOfWorkScope() ) {

				_impl.creareNuovoCarrello();

				ParamStampaFoto paramM4 = ricavaParamStampa( "M" );
				paramM4.numCopie = 4;
				ParamStampaFoto paramM2 = ricavaParamStampa( "M" );
				paramM2.numCopie = 2;

				int totFoto = 2;

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include( "fotografo" )
										  select f).Take( totFoto ).ToList();

				// Controllo che ci siano abbastanza foto nel database
				Assert.IsTrue( fotos.Count == totFoto );

				contaStampate = 0;

				int qq = 0;
				_impl.aggiungereStampe( fotos[0], paramM4 );

				Assert.AreEqual( _impl.carrello.totaleAPagare, paramM4.numCopie * PRZ_FRM_MED );
				Assert.AreEqual( _impl.carrello.righeCarrello.Count, 1 );

				_impl.aggiungereStampe( fotos[1], paramM2 );

				Assert.AreEqual( _impl.carrello.righeCarrello.Count, 2 );

				_impl.ricalcolaTotaleCarrello();
				var totPagarePrima = _impl.carrello.totaleAPagare;

				Assert.AreEqual( totPagarePrima, PRZ_FRM_MED * 6 );

				Carrello cart = _impl.CalcolaPromozioni( true );

				var totPagareDopo = cart.totaleAPagare;

				Assert.AreEqual( totPagareDopo, PRZ_FRM_MED * 5, "valore carrello errato" );

			}
			
		}


		/// <summary>
		/// Questo test era per risolvere un bug di quando metto a carrello
		/// più copie della stessa foto. Sbagliava il prezzo finale.
		/// </summary>
		[TestMethod, TestCategory( "Promozioni" )]
		public void PromoCalTest8() {

			using( new UnitOfWorkScope() ) {

				_impl.creareNuovoCarrello();

				ParamStampaFoto paramM2 = ricavaParamStampa( "M" );
				paramM2.numCopie = 2;
				ParamStampaFoto paramM16 = ricavaParamStampa( "M" );
				paramM16.numCopie = 16;

				int totFoto = 2;

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include( "fotografo" )
										  select f).Take( totFoto ).ToList();

				// Controllo che ci siano abbastanza foto nel database
				Assert.IsTrue( fotos.Count == totFoto );

				contaStampate = 0;

				int qq = 0;
				_impl.aggiungereStampe( fotos[0], paramM2 );

				Assert.AreEqual( _impl.carrello.totaleAPagare, paramM2.numCopie * PRZ_FRM_MED );
				Assert.AreEqual( _impl.carrello.righeCarrello.Count, 1 );

				_impl.aggiungereStampe( fotos[1], paramM16 );

				Assert.AreEqual( _impl.carrello.righeCarrello.Count, 2 );

				_impl.ricalcolaTotaleCarrello();
				var totPagarePrima = _impl.carrello.totaleAPagare;

				Assert.AreEqual( totPagarePrima, PRZ_FRM_MED * 18 );

				Carrello cart = _impl.CalcolaPromozioni( true );

				var totPagareDopo = cart.totaleAPagare;

				Assert.AreEqual( totPagareDopo, PRZ_FRM_MED * 15, "valore carrello errato" );
			}
		}



		/// <summary>
		/// Prendo 12 foto, ne metto 4 in stampa e 12 masterizzate 
		/// (le 4 in stampa sono le stesse masterizzate)
		/// </summary>
		[TestMethod, TestCategory( "Promozioni" )]
		public void PromoCalTest9() {

			using( new UnitOfWorkScope() ) {

				_impl.creareNuovoCarrello();

				ParamStampaFoto paramM4 = ricavaParamStampa( "M" );

				int totFoto = 12;

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include( "fotografo" )
										  select f).Take( totFoto ).ToList();

				// Controllo che ci siano abbastanza foto nel database
				Assert.IsTrue( fotos.Count == totFoto );

				contaStampate = 0;

				int qq = 0;
				// Delle 12 foto, metto solo le prime 4 in stampa.
				_impl.aggiungereStampe( fotos.Take( 4 ), paramM4 );

				// Poi metto tutte e 12 da masterizzare
				_impl.aggiungereMasterizzate( fotos );

				Assert.AreEqual( _impl.carrello.righeCarrello.Count, 4+12 );

				_impl.ricalcolaTotaleCarrello();

				decimal totalePrePromo = (4 * PRZ_FRM_MED) + (totFoto * PRZ_FRM_FILE);
				Assert.AreEqual( _impl.carrello.totaleAPagare, totalePrePromo );

				Carrello cart = _impl.CalcolaPromozioni( true );

				var totalePostPromo = (4 * PRZ_FRM_MED) + (2 * 0) + (6 * PRZ_FRM_FILE) + (4 * 1);

				Assert.AreEqual( cart.totaleAPagare, totalePostPromo, "tot carrello errato x promozioni" );
			}
		}

		/// <summary>
		/// Questo test è il contrario del n.9 ovverto
		/// sempre 12 foto, ma ne stampo 12 e ne masterizzo le prime 4
		/// </summary>
		[TestMethod, TestCategory( "Promozioni" )]
		public void PromoCalTest10() {

			using( new UnitOfWorkScope() ) {

				_impl.creareNuovoCarrello();

				ParamStampaFoto paramM = ricavaParamStampa( "M" );

				int totFoto = 12;

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include( "fotografo" )
										  select f).Take( totFoto ).ToList();

				// Controllo che ci siano abbastanza foto nel database
				Assert.IsTrue( fotos.Count == totFoto );

				contaStampate = 0;

				int qq = 0;
				// Delle 12 foto in stampa.
				_impl.aggiungereStampe( fotos, paramM );

				// Poi metto 4 da masterizzare
				_impl.aggiungereMasterizzate( fotos.Take( 4 ) );

				Assert.AreEqual( _impl.carrello.righeCarrello.Count, 4 + 12 );

				_impl.ricalcolaTotaleCarrello();

				decimal totalePrePromo = (totFoto * PRZ_FRM_MED) + (4 * PRZ_FRM_FILE);
				Assert.AreEqual( _impl.carrello.totaleAPagare, totalePrePromo );

				Carrello cart = _impl.CalcolaPromozioni( true );

				var totalePostPromo = ((12-2) * PRZ_FRM_MED) + (4 * 1);

				Assert.AreEqual( cart.totaleAPagare, totalePostPromo, "tot carrello errato x promozioni" );
			}
		}
	}
}
