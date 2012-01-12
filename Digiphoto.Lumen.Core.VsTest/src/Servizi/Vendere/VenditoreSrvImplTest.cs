using Digiphoto.Lumen.Servizi.Vendere;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Stampare;
using System.Data.Objects;
using System.Linq;
using System.Collections.Generic;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Servizi.Masterizzare;
using System.IO;

namespace Digiphoto.Lumen.Core.VsTest
{
    
    
    /// <summary>
    ///This is a test class for VenditoreSrvImplTest and is intended
    ///to contain all VenditoreSrvImplTest Unit Tests
    ///</summary>
	[TestClass()]
	public class VenditoreSrvImplTest : IObserver<Messaggio> {

		const int QUANTE = 3;

		int contaStampate = 0;


		#region ButtaSu
		[ClassInitialize()]
		public static void MyClassInitialize( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}

		[ClassCleanup()]
		public static void MyClassCleanUp() {
			LumenApplication.Instance.ferma();
		}

		[TestInitialize()]
		public void MyTestInitialize() {

			this._impl = (VenditoreSrvImpl) LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();

			IObservable<Messaggio> observable = LumenApplication.Instance.bus.Observe<Messaggio>();
			observable.Subscribe( this );
		}

		[TestCleanup()]
		public void MyTestCleanup() {
			this._impl.Dispose();
		}


		#endregion

		private VenditoreSrvImpl _impl;


		[TestMethod]
		public void vendiFotoTest() {


			_impl.creaNuovoCarrello();

			// -------


			using( new UnitOfWorkScope(false) ) {

				ParamStampaFoto p = ricavaParamStampa();

				LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

				List<Fotografia> fotos = dbContext.Fotografie.Top( Convert.ToString(QUANTE) ).ToList();

				contaStampate = 0;

				_impl.aggiungiStampe( fotos, p );

				_impl.aggiungiMasterizzate( fotos );
				_impl.masterizzaSrv.impostaDestinazione( TipoDestinazione.CARTELLA, Path.GetTempPath() );
				_impl.masterizzaSrv.prezzoForfaittario = 7;

				_impl.confermaCarrello();

				Assert.IsTrue( _impl.carrello.totaleAPagare == 15 + 7 );
			}


			while( ! venditaCompletata )
				System.Threading.Thread.Sleep( 6000 );

			_impl.stop();


			Console.WriteLine( "FINITO" );
		}


		private ParamStampaFoto ricavaParamStampa() {

			ParamStampaFoto p = new ParamStampaFoto();

			// Vediamo se esiste il formato
			// TODO : creare un nuovo attributo che identifica il formato carta come chiave naturale (per esempio A4 oppure 6x8)

			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

			FormatoCarta formato = Utilita.ottieniFormatoCarta( dbContext, "A5" );
			formato.prezzo = 5;

			p.formatoCarta = formato;
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
				return contaStampate == QUANTE && this._impl.masterizzaSrv.isCompletato;
			}
		}

	}
}
