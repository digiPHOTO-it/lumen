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
using Digiphoto.Lumen.Servizi.Reports;
using System.Diagnostics;
using Digiphoto.Lumen.Database;

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

		}

		[ClassCleanup()]
		public static void MyClassCleanUp() {

		}


		IDisposable ascoltami;

		[TestInitialize()]
		public void MyTestInitialize() {
			LumenApplication.Instance.avvia();
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

			// Istanzio una coda di stampa e la chiudo
			CodaDiStampe c1 = new CodaDiStampe( "coda1" );
			c1.Start();
			c1.Stop();
			c1.Dispose();

			CodaDiStampe c2 = new CodaDiStampe( "coda2" );
			c2.Dispose();

			CodaDiStampe c3 = new CodaDiStampe( "coda3" );
			c3.Stop();
			c3.Stop();
			c3.Dispose();




		}

		[TestMethod]
		public void codaDiStampeConAbort() {

			CodaDiStampe c3 = new CodaDiStampe( "doPDF v7" );
			c3.Stop();

			c3.EnqueueItem( new LavoroDiStampa( new Fotografia(), new ParamStampaFoto() ) );
			// Accodo una stampa in modo da testare l'abort

			c3.Stop( Threading.PendingItemAction.AbortPendingItems );
			c3.Dispose();
		}



		[TestMethod]
		public void vendiFotoTest() {


			using( new UnitOfWorkScope( false ) ) {

				_impl.creaNuovoCarrello();


				ParamStampaFoto p = ricavaParamStampa();

				LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;
				List<Fotografia> fotos = (from f in dbContext.Fotografie.Include( "fotografo" )
										  select f).Take( QUANTE ).ToList();

				if( fotos.Count == QUANTE ) {

					contaStampate = 0;

					_impl.aggiungiStampe( fotos, p );

					_impl.aggiungiMasterizzate( fotos );
					_impl.masterizzaSrv.impostaDestinazione( TipoDestinazione.CARTELLA, Path.GetTempPath() );
					_impl.masterizzaSrv.prezzoForfaittario = 7;

					Assert.IsFalse( _impl.carrello.venduto );

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


		private ParamStampaFoto ricavaParamStampa() {

			ParamStampaFoto p = new ParamStampaFoto();

			// Vediamo se esiste il formato
			// TODO : creare un nuovo attributo che identifica il formato carta come chiave naturale (per esempio A4 oppure 6x8)

			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

			FormatoCarta formato = Utilita.ottieniFormatoCarta( dbContext, "A5" );
			formato.prezzo = 5;
			p.formatoCarta = formato;

			// Qui non si deve spaccare
			Digiphoto.Lumen.Database.OrmUtil.forseAttacca<FormatoCarta>( dbContext.ObjectContext, "LumenEntities.FormatiCarta", ref formato );

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
				return this._impl.masterizzaSrv.isCompletato;
				// return contaStampate == QUANTE && this._impl.masterizzaSrv.isCompletato;
			}
		}


		[TestMethod]
		public void queryVenditeConTotali() {

			using( new UnitOfWorkScope() ) {
			
				LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;
				DateTime dataIniz = new DateTime( 2012, 04, 01 );
				DateTime dataFine = new DateTime( 2012, 04, 30 );


				var porc = from c in dbContext.Carrelli.Include( "righeCarrello" )
						   from r in c.righeCarrello.OfType<RiCaFotoStampata>()
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

						if( rc is RiCaFotoStampata ) {
							RiCaFotoStampata rfs = (RiCaFotoStampata)rc;

						}

						if( rc is RiCaDiscoMasterizzato ) {

						}

					}

					var qq = carrello.righeCarrello.OfType<RiCaFotoStampata>()
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



	}
}
