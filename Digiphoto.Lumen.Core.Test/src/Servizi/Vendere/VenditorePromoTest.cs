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
	public class VenditorePromoTest : IObserver<Messaggio> {

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

				_impl.creareNuovoCarrello();

				ParamStampaFoto p = ricavaParamStampa( "P" );

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include( "fotografo" )
										  select f).Take( 4 ).ToList();

				Assert.IsTrue( fotos.Count == 4);

				contaStampate = 0;

				// Prendo solo i primi 3 elementi
				_impl.aggiungereStampe( fotos.Take( 3 ), p );
				_impl.aggiungereMasterizzate( new [] { fotos[0], fotos[3] } );	// Solo una foto in promo
				_impl.carrello.prezzoDischetto = 123;

				Assert.IsFalse( _impl.carrello.venduto );

				Assert.IsTrue( _impl.isPossibileSalvareCarrello );
				Assert.IsTrue( _impl.isPossibileVendereCarrello );
				Assert.IsTrue( _impl.isPossibileModificareCarrello );

				var newCart = _impl.CalcolaPromozioni();

				Assert.IsTrue( newCart.totaleAPagare == (decimal)( (3*4.5) + (1*1) + (1*3.5) ) );
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
	}
}
