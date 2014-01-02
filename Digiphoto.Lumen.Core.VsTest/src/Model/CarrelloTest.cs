using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Model;
using System.Data.Entity.Core.EntityClient;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using System.Transactions;
using System.Data.Entity.Core.Objects;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Database;
using System.Data.Entity.Validation;
using Digiphoto.Lumen.Util;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Servizi.Vendere;
using System.Data.Entity;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Servizi.Stampare;
using System.ComponentModel;

namespace Digiphoto.Lumen.Core.VsTest {

	

	[TestClass]
	public class CarrelloTest {

		int _contaStampate = 0;
		int _contaMasterizzate = 0;
		Carrello _carrelloInserito = null;


		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void CarrelloTestInitialize( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}

		[ClassCleanup()]
		public static void classeCleanup() {
			LumenApplication.Instance.ferma();
		}


		[TestCleanup]
		public void Cleanup() {
		}

		/// <summary>
		/// Carico i carrelli con anche le righe
		/// </summary>
		[TestMethod]
		public void eagerLodingTest() {

			using( LumenEntities dbContext = new LumenEntities() ) {

				var carrelli = dbContext.Carrelli.Include( "righeCarrello" ).Where( cc => cc.righeCarrello.Count > 1 ).Take( 5 );

				foreach( Carrello c in carrelli ) {
					Debug.WriteLine( "\n\n*** Carrello = " + c.id + " " + c.giornata );

					foreach( RigaCarrello r in c.righeCarrello ) {

						Debug.WriteLine( "\n\t" + r.GetType().Name + " "  + r.id + " " + r.descrizione  );

						Debug.WriteLine( "\t\tFotografo     = " + r.fotografo );
						Debug.WriteLine( "\t\tFotografia    = " + r.fotografia );
						if( r.fotografia != null )
							Debug.WriteLine( "\t\tDataOra = " + r.fotografia.dataOraAcquisizione );

						if( r.discriminator == Carrello.TIPORIGA_STAMPA ) {
							Debug.WriteLine( "\t\tFormato Carta = " + r.formatoCarta );
						}
						if( r.discriminator == Carrello.TIPORIGA_MASTERIZZATA ) {
							Debug.WriteLine( "\t\tTot. foto masterizzate = " + c.totMasterizzate );
						}
					}
				}

			}

		}

		[TestMethod]
		public void carrelloTest() {

			using( LumenEntities dbContext = new LumenEntities() ) {

				Carrello c1 = new Carrello();
				c1.id = Guid.NewGuid();
				c1.giornata = DateTime.Today;
				c1.tempo = DateTime.Now;
				c1.totaleAPagare = 123m;
				c1.righeCarrello = new EntityCollection<RigaCarrello>();
				_carrelloInserito = c1;

				// ---

				RigaCarrello r1 = new RigaCarrello();
				r1.discriminator = Carrello.TIPORIGA_MASTERIZZATA;
				r1.id = Guid.NewGuid();
				r1.prezzoLordoUnitario = new Decimal( 20 );
				r1.quantita = 2;
				r1.prezzoNettoTotale = Decimal.Multiply( r1.prezzoLordoUnitario, r1.quantita );
				r1.descrizione = "Foto masterizzata";
				c1.righeCarrello.Add( r1 );
				_contaMasterizzate++;

				// ---

				RigaCarrello r2 = new RigaCarrello();
				r2.discriminator = Carrello.TIPORIGA_STAMPA;
				r2.id = Guid.NewGuid();
				r2.prezzoLordoUnitario = new Decimal( 5 );
				r2.quantita = 3;
				r2.prezzoNettoTotale = Decimal.Multiply( r2.prezzoLordoUnitario, r2.quantita );
				r2.descrizione = "RicaFotoStampata1";
				r2.totFogliStampati = 3;
				r2.formatoCarta = Utilita.ottieniFormatoCarta( dbContext, "A4" );
				r2.fotografo = Utilita.ottieniFotografoMario( dbContext );
				c1.righeCarrello.Add( r2 );
				_contaStampate++;

				// ---

	
				RigaCarrello r3 = new RigaCarrello();
				r3.discriminator = Carrello.TIPORIGA_STAMPA;
				r3.id = Guid.NewGuid();
				r3.prezzoLordoUnitario = new Decimal( 5 );
				r3.quantita = 2;
				r3.prezzoNettoTotale = Decimal.Multiply( r3.prezzoLordoUnitario, r3.quantita );
				r3.descrizione = "RicaFotoStampata1";
				r3.totFogliStampati = 3;
				r3.formatoCarta = Utilita.ottieniFormatoCarta( dbContext, "A4" );
				r3.fotografo = Utilita.ottieniFotografoMario( dbContext );
				c1.righeCarrello.Add( r3 );
				_contaStampate++;

				// ---
				
				dbContext.Carrelli.Add( c1 );


				try {
					dbContext.SaveChanges();

				} catch( Exception ee) {

					String msg = ErroriUtil.estraiMessage( ee );
					Console.WriteLine( msg );

					throw ee;
				}


			}

			// Verifico che l'inserimento appena effettuato sia andato bene.
			queryPolimorficaCorrente();


			// provo altre tecniche di query, giusto per sport.
			queryPolimorficaSql();
			queryPolimorfica();
		}

		[TestMethod]
		public void rigaCarrelloStaccaFormatoCarta() {

			FormatoCarta formato;
			using( LumenEntities dbContext = new LumenEntities() ) {
				formato = dbContext.FormatiCarta.FirstOrDefault();
			}

			Fotografia fotografia;
			using( LumenEntities dbContext = new LumenEntities() ) {
				fotografia = dbContext.Fotografie.FirstOrDefault();
			}

			Fotografo fotografo;
			using( LumenEntities dbContext = new LumenEntities() ) {
				fotografo = dbContext.Fotografi.FirstOrDefault();
			}

			using( LumenEntities dbContext = new LumenEntities() ) {
				
				dbContext.FormatiCarta.Attach( formato );
				dbContext.Fotografi.Attach( fotografo );
				dbContext.Fotografie.Attach( fotografia );

				RigaCarrello rr = new RigaCarrello();
				rr.discriminator = Carrello.TIPORIGA_STAMPA;
				rr.formatoCarta =  formato;
				rr.fotografo = fotografo;
				rr.fotografia = fotografia;
			}



		}


		/**
		 * Provo a leggere tutte le righe di tutti i carrelli ma solo quelle di tipo
		 * foto stampata.
		 */
		private void queryPolimorfica() {

			using( LumenEntities dbContext = new LumenEntities() ) {
				foreach( RigaCarrello riCaFotoStampata in 
					dbContext.RigheCarrelli.Include( "fotografo" ).Include( "fotografia" ).Where( r => r.discriminator == Carrello.TIPORIGA_STAMPA ) ) {
					Trace.WriteLine ( "Riga Carrello foto stampata: " + riCaFotoStampata.fotografo.id + " totFoto=" + riCaFotoStampata.totFogliStampati );
				}
			}
		}

		private void queryPolimorficaCorrente() {

			using( LumenEntities dbContext = new LumenEntities() ) {

				// Prendo le righe dell'ultimo carrello inserito,
				// ma solo quelle di tipo foto stampata
				IQueryable<RigaCarrello> esito =
					from c in dbContext.Carrelli.Include( "righeCarrelli" )
					from r in c.righeCarrello
					where (c.id == _carrelloInserito.id && r.discriminator == Carrello.TIPORIGA_STAMPA )
					select r;




				// le righe inserite nell'ultimo carrello sono 3 ma soltanto 2 sono di tipo foto stampata
				Assert.IsTrue( esito.Count() == _contaStampate );
				foreach( RigaCarrello riga in esito ) {
					// Trace.WriteLine( "Riga Carrello foto stampata: " + riCaFotoStampata.fotografo.id + " totFoto=" + riCaFotoStampata.totFogliStampati );
					Trace.WriteLine( "Riga Carrello " + riga.ToString() );
				}

			}
		}


		/**
		 * Con questa query estraggo solo le righe del carrello di tipo "foto stampata"
		 * ma usando una sintassi stile SQL. Diciamo che questa non mi piace molto.
		 */
		private void queryPolimorficaSql() {

			using( EntityConnection conn = new EntityConnection( "name=LumenEntities" ) ) {

				conn.Open();
				// Create a query that specifies to 
				// get a collection of only Righe Stampate.
				
				string esqlQuery = @"SELECT VALUE fs 
				                   FROM  LumenEntities.RigheCarrelli AS fs";

				using( EntityCommand cmd = new EntityCommand( esqlQuery, conn ) ) {
					// Execute the command.
					using( DbDataReader rdr = cmd.ExecuteReader( CommandBehavior.SequentialAccess ) ) {
						// Start reading.
						while( rdr.Read() ) {
							// Display 
							Console.WriteLine( "id: {0} ", rdr ["id"] );
							Console.WriteLine( "descriz: {0} ", rdr ["descrizione"] );
						}
					}
				}
			}

		}

		[TestMethod]
		public void simulaUiStaccando() {

			Carrello c3;

			using( LumenEntities dbContext = new LumenEntities() ) {

				c3 = new Carrello { id=Guid.NewGuid(), giornata=DateTime.Today, tempo=DateTime.Now, totaleAPagare = 123m };
				c3.righeCarrello = new EntityCollection<RigaCarrello>();
			}

			// ----------
			FormatoCarta formato;
			using( LumenEntities dbContext = new LumenEntities() ) {
				formato = dbContext.FormatiCarta.FirstOrDefault();
			}

			Fotografia fotografia;
			using( LumenEntities dbContext = new LumenEntities() ) {
				fotografia = dbContext.Fotografie.FirstOrDefault();
			}

			Fotografo fotografo;
			using( LumenEntities dbContext = new LumenEntities() ) {
				fotografo = dbContext.Fotografi.FirstOrDefault();
			}

			// ----------

			using( LumenEntities dbContext = new LumenEntities() ) {

				RigaCarrello r1 = new RigaCarrello();
				r1.discriminator = Carrello.TIPORIGA_STAMPA;
				r1.id = Guid.NewGuid();
				r1.prezzoLordoUnitario = new Decimal( 5 );
				r1.quantita = 3;
				r1.prezzoNettoTotale = Decimal.Multiply( r1.prezzoLordoUnitario, r1.quantita );
				r1.descrizione = "RicaFotoStampata1";
				r1.totFogliStampati = 11;

				r1.formatoCarta = formato;
				r1.fotografo = fotografo;
				r1.fotografia = fotografia;
				
				c3.righeCarrello.Add( r1 );
			}


			// ----------

			using( LumenEntities dbContext = new LumenEntities() ) {
				
				RigaCarrello r1 = c3.righeCarrello.ElementAt( 0 );

				// Riattacco le associazioni altrimeti si spacca (sembra)
				dbContext.FormatiCarta.Attach( r1.formatoCarta );
				dbContext.Fotografi.Attach( r1.fotografo );
				dbContext.Fotografie.Attach( r1.fotografia );

				dbContext.Carrelli.Add( c3 );

				try {
					dbContext.SaveChanges();

	
				} catch( DbEntityValidationException ee) {

					string msg = ErroriUtil.estraiMessage( ee );
					Console.WriteLine( msg );

					throw;
				}

			}
		}



		[TestMethod]
		public void carrelloConPezziStaccati() {

			Carrello c3 = new Carrello { id = Guid.NewGuid(), giornata = DateTime.Today, tempo = DateTime.Now, totaleAPagare = 123m	};
			c3.righeCarrello = new EntityCollection<RigaCarrello>();

			// ----------
			FormatoCarta formato;
			using( LumenEntities dbContext = new LumenEntities() ) {
				formato = dbContext.FormatiCarta.FirstOrDefault();
			}

			Fotografia fotografia;
			using( LumenEntities dbContext = new LumenEntities() ) {
				fotografia = dbContext.Fotografie.FirstOrDefault();
			}

			Fotografo fotografo;
			using( LumenEntities dbContext = new LumenEntities() ) {
				fotografo = dbContext.Fotografi.FirstOrDefault();
			}

			// ----------

			using( LumenEntities dbContext = new LumenEntities() ) {

				// Creo la riga con gli attributi scalari
				RigaCarrello r1 = new RigaCarrello {
					discriminator = Carrello.TIPORIGA_STAMPA,
					id = Guid.NewGuid(),
					prezzoLordoUnitario = 5,
					quantita = 3,
					prezzoNettoTotale = 15,
					totFogliStampati = 11,
					descrizione = "RicaFotoStampata1",
				};

				// Aggiungo le associazioni
				r1.formatoCarta = formato;
				r1.fotografo = fotografo;
				r1.fotografia = fotografia;

				// Aggiungo la riga al carrello
				c3.righeCarrello.Add( r1 );
			}


			// ----------

			using( LumenEntities dbContext = new LumenEntities() ) {

				RigaCarrello r1 = c3.righeCarrello.ElementAt( 0 );

				dbContext.Fotografie.Attach( r1.fotografia );
				dbContext.FormatiCarta.Attach( r1.formatoCarta );
				dbContext.Fotografi.Attach( r1.fotografo );

				// The EntityKey property can only be set when the current value of the property is null
				dbContext.Carrelli.Add( c3 );
				dbContext.SaveChanges();
			}
		}


		[TestMethod]
		public void aggiungiUnaRigaAdUnCarrelloEsistente() {

			int countRigheCarrello = 0;
			Carrello carrelloCorrente;

			using( LumenEntities dbContext = new LumenEntities() ) {
				carrelloCorrente = dbContext.Carrelli.Include( "righeCarrello" ).Take( 1 ).Single();
				countRigheCarrello = carrelloCorrente.righeCarrello.Count();
			}

			FormatoCarta formato;
			using( LumenEntities dbContext = new LumenEntities() ) {
				formato = dbContext.FormatiCarta.First();
			}

			Fotografia fotografia;
			using( LumenEntities dbContext = new LumenEntities() ) {
				fotografia = dbContext.Fotografie.Include("fotografo").Where( f => f.fotografo != null ).First();
			}

			Fotografo fotografo;
			using( LumenEntities dbContext = new LumenEntities() ) {
				fotografo = dbContext.Fotografi.First();
			}

			using( LumenEntities dbContext = new LumenEntities() ) {

				var objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;
				objectContext.AttachTo( "Carrelli", carrelloCorrente );
				ObjectStateEntry s1 = objectContext.ObjectStateManager.GetObjectStateEntry( carrelloCorrente );

				objectContext.AttachTo( "FormatiCarta", formato );
				ObjectStateEntry s2 = objectContext.ObjectStateManager.GetObjectStateEntry( formato );

				objectContext.AttachTo( "Fotografie", fotografia );
				ObjectStateEntry s3 = objectContext.ObjectStateManager.GetObjectStateEntry( fotografia );



				// ======= Occhio qui !!!! poi ti spiego =====
 				if( fotografo.id.Equals( fotografia.fotografo.id ) )
					fotografo = fotografia.fotografo;
				else
					objectContext.AttachTo( "Fotografi", fotografo );
				ObjectStateEntry s4 = objectContext.ObjectStateManager.GetObjectStateEntry( fotografo );
				// ======= Occhio qui !!!! poi ti spiego =====



				RigaCarrello riga = objectContext.CreateObject<RigaCarrello>();
				riga.id = Guid.NewGuid();
				riga.discriminator = Carrello.TIPORIGA_STAMPA;
				riga.prezzoLordoUnitario = new Decimal( 5 );
				riga.quantita = 3;
				riga.prezzoNettoTotale = Decimal.Multiply( riga.prezzoLordoUnitario, riga.quantita );
				riga.descrizione = "SaveCarrelloLodingTest";
				riga.totFogliStampati = 3;
				riga.formatoCarta = formato;
				riga.fotografo = fotografo;
				riga.fotografia = fotografia;

				carrelloCorrente.righeCarrello.Add( riga );
				s1 = objectContext.ObjectStateManager.GetObjectStateEntry( carrelloCorrente );

				int quanti = dbContext.SaveChanges();
			}

			// Controllo che le righe siano aumentate di uno.
			using( LumenEntities dbContext = new LumenEntities() ) {
				var testCarrello2 = dbContext.Carrelli.Include( "righeCarrello" ).Where( c => c.id == carrelloCorrente.id ).Single();
				Assert.IsTrue( countRigheCarrello + 1 == testCarrello2.righeCarrello.Count );
			}
		}



		[TestMethod]
		public void simulaUiStaccando2() {

			Carrello carrello;
			ObjectStateEntry stato;


			using( LumenEntities dbContext = new LumenEntities() ) {

				ObjectContext objContext = ((IObjectContextAdapter)dbContext).ObjectContext;
				carrello = dbContext.Carrelli.FirstOrDefault( c => c.venduto == false );

				bool trovato = objContext.ObjectStateManager.TryGetObjectStateEntry( carrello, out stato );
				Assert.IsTrue( trovato );
				Assert.AreEqual( stato.State, EntityState.Unchanged );
			}

			using( LumenEntities dbContext = new LumenEntities() ) {
				ObjectContext objContext = ((IObjectContextAdapter)dbContext).ObjectContext;
				bool isDetached = dbContext.Entry( carrello ).State == EntityState.Detached;
				Assert.IsTrue( isDetached );

				// Modifico l'oggetto da staccato
				Random rnd = new Random();
				int randomNumber = rnd.Next( 0, 9 );
				carrello.note = carrello.note + randomNumber.ToString();
			}

			using( LumenEntities dbContext = new LumenEntities() ) {
				ObjectContext objContext = ((IObjectContextAdapter)dbContext).ObjectContext;
				bool isDetached = dbContext.Entry( carrello ).State == EntityState.Detached;
				Assert.IsTrue( isDetached );
				var poldo = dbContext.Carrelli.Attach( carrello );
				isDetached = dbContext.Entry( carrello ).State == EntityState.Detached;
				Assert.IsFalse( isDetached );

				// Qui non ci devono essere modifiche, perché prima ho modificato l'oggetto quando era staccato.
				int quanti = dbContext.SaveChanges();
				Assert.IsTrue( quanti == 0 );
			}


			// -- Ora provo a modificare l'oggetto da attaccato.
			using( LumenEntities dbContext = new LumenEntities() ) {
				ObjectContext objContext = ((IObjectContextAdapter)dbContext).ObjectContext;
				bool isDetached = dbContext.Entry( carrello ).State == EntityState.Detached;
				Assert.IsTrue( isDetached );
				var poldo = dbContext.Carrelli.Attach( carrello );
				isDetached = dbContext.Entry( carrello ).State == EntityState.Detached;
				Assert.IsFalse( isDetached );

				// Modifico l'oggetto da attaccato
				Random rnd = new Random();
				int randomNumber = rnd.Next( 0, 9 );
				carrello.note = carrello.note + randomNumber.ToString();

				var tt = dbContext.Entry( carrello );
				Assert.AreEqual( tt.State, EntityState.Modified );
			}


			using( LumenEntities dbContext = new LumenEntities() ) {
				ObjectContext objContext = ((IObjectContextAdapter)dbContext).ObjectContext;

				// Provo a salvare l'oggetto staccato e mi deve dare errore.
				int quantiQ = dbContext.SaveChanges();
				Assert.IsTrue( quantiQ == 0 );

				// Ora forzo lo stato della entità a modificato

				var poldo = dbContext.Carrelli.Attach( carrello );
				var test2 = dbContext.Entry( carrello );
				Assert.AreEqual( test2.State, EntityState.Unchanged );

				objContext.DetectChanges();
				var test3 = dbContext.Entry( carrello );
				Assert.AreEqual( test3.State, EntityState.Unchanged );

				test3.State = EntityState.Modified;
				int quanti = dbContext.SaveChanges();
				Assert.IsTrue( quanti > 0 );
			}
		}

		[TestMethod]
		public void simulaUiStaccando3() {

			Carrello carrello;
			String notePrec;
			String numAggiunto;
			IVenditoreSrv venditoreSrv ;

			using( new UnitOfWorkScope() ) {
				var context = UnitOfWorkScope.currentDbContext;
				carrello = context.Carrelli.Include( "righeCarrello" ).Where( c => c.righeCarrello.Count > 0 &&  c.righeCarrello.Count < 3 && c.venduto == false ).FirstOrDefault();
			}

			using( new UnitOfWorkScope() ) {
				var context = UnitOfWorkScope.currentDbContext;
				venditoreSrv = LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();
				venditoreSrv.caricaCarrello( carrello );
				// Salvo le note precedenti.
				notePrec = venditoreSrv.carrello.note;
				if( notePrec == null )
					notePrec = String.Empty;
			}

			bool aggiungiUnaRiga = true;

			List<Fotografia> fotos = new List<Fotografia>();
			ParamStampaFoto par = null;
			if( aggiungiUnaRiga ) {
				using( new UnitOfWorkScope() ) {

					// Cerco una fotografia a caso
					IFotoExplorerSrv fotoExplorerSrv = LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
					fotoExplorerSrv.cercaFoto( new ParamCercaFoto {
						paginazione = new Paginazione {
							take = 1
						}
					} );

					var dbContext = UnitOfWorkScope.currentDbContext;
					par = new ParamStampaFoto {
						formatoCarta = dbContext.FormatiCarta.First(),
						nomeStampante = "qqq"
					};

					fotos = fotoExplorerSrv.fotografie;
				}
			}

			using( new UnitOfWorkScope() ) {
				// Modifico l'oggetto da staccato
				Random rnd = new Random();
				int randomNumber = rnd.Next( 0, 9 );
				numAggiunto = randomNumber.ToString();

				venditoreSrv.carrello.note = venditoreSrv.carrello.note + numAggiunto;

				if( aggiungiUnaRiga ) {
					venditoreSrv.aggiungereStampe( fotos, par );
				}
			}

			using( new UnitOfWorkScope() ) {
				var context = UnitOfWorkScope.currentDbContext;
				bool esito = venditoreSrv.salvaCarrello();
				Assert.IsTrue( esito );
			}

			using( new UnitOfWorkScope() ) {

				var context = UnitOfWorkScope.currentDbContext;
				Carrello carrello2 = context.Carrelli.Where( c => c.id == carrello.id ).Single();
				Assert.AreEqual( carrello2.note, notePrec + numAggiunto );
			}

		}

		[TestMethod]
		public void rigaCarrelloNotificaPropertyChanged() {
			// Se succede questa eccezione significa che è stato salvato il modello EDMX e quindi 
			// ha rigenerato la classe RigaCarrello.
			// Purtoppo questa classe l'ho dovuta personalizzare a mano.
			// Deve implementare INotifyPropertyChanged e le 2 property
			//  1) quantita
			//  2) prezzoNettoTotale
			//  devono rilanciare l'evento di property modificata.
			// Occorre quindi sistemare a mano il sorgente (magari riprendendolo dal vcs)
			Assert.IsTrue( typeof( INotifyPropertyChanged ).IsAssignableFrom( typeof(RigaCarrello) ) );
		}
	}
}
