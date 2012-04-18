using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Model;
using System.Data.EntityClient;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using System.Transactions;

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

		[TestCleanup]
		public void Cleanup() {
			LumenApplication.Instance.ferma();
		}



		[TestMethod]
		public void carrelloTest() {

			using( LumenEntities dbContext = new LumenEntities() ) {

				Carrello c1 = new Carrello();
				c1.id = Guid.NewGuid();
				c1.giornata = DateTime.Today;
				c1.tempo = DateTime.Now;
				c1.totaleAPagare = 123m;
				c1.righeCarrello = new System.Data.Objects.DataClasses.EntityCollection<RigaCarrello>();
				_carrelloInserito = c1;

				// ---

				RiCaDiscoMasterizzato r1 = new RiCaDiscoMasterizzato();
				r1.id = Guid.NewGuid();
				r1.prezzoLordoUnitario = new Decimal( 20 );
				r1.quantita = 2;
				r1.prezzoNettoTotale = Decimal.Multiply( r1.prezzoLordoUnitario, r1.quantita );
				r1.descrizione = "RiCaDiscoMasterizzato";
				r1.totFotoMasterizzate = 85;
				c1.righeCarrello.Add( r1 );
				_contaMasterizzate++;

				// ---

				RiCaFotoStampata r2 = new RiCaFotoStampata();
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

	
				


				RiCaFotoStampata r3 = new RiCaFotoStampata();
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

				dbContext.SaveChanges();


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

				RiCaFotoStampata rr = new RiCaFotoStampata();
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
				foreach( RiCaFotoStampata riCaFotoStampata in dbContext.RigheCarrelli.OfType<RiCaFotoStampata>() ) {
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
					where (c.id == _carrelloInserito.id && r is RiCaFotoStampata)
					select r;




				// le righe inserite nell'ultimo carrello sono 3 ma soltanto 2 sono di tipo foto stampata
				Assert.IsTrue( esito.Count() == _contaStampate );
				foreach( RiCaFotoStampata riga in esito ) {
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
				                   FROM  OFTYPE(LumenEntities.RigheCarrelli, Digiphoto.Lumen.Model.RiCaFotoStampata) AS fs";

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
				c3.righeCarrello = new System.Data.Objects.DataClasses.EntityCollection<RigaCarrello>();
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

				RiCaFotoStampata r1 = new RiCaFotoStampata();
				r1.id = Guid.NewGuid();
				r1.prezzoLordoUnitario = new Decimal( 5 );
				r1.quantita = 3;
				r1.prezzoNettoTotale = Decimal.Multiply( r1.prezzoLordoUnitario, r1.quantita );
				r1.descrizione = "RicaFotoStampata1";
				r1.totFogliStampati = 11;

				// Riattacco le associazioni
				dbContext.FormatiCarta.Attach( formato );
/*
				dbContext.Fotografi.Attach( fotografo );
				dbContext.Fotografie.Attach( fotografia );
*/
				r1.formatoCarta = formato;

				//r1.fotografo = fotografo;
				//r1.fotografia = fotografia;
				
				c3.righeCarrello.Add( r1 );
			}


			// ----------

			using( LumenEntities dbContext = new LumenEntities() ) {
				
				RiCaFotoStampata r1 = (RiCaFotoStampata) c3.righeCarrello.ElementAt( 0 );
				
				FormatoCarta fc = r1.formatoCarta;
				dbContext.RigheCarrelli.Add( r1 );

//				r1.formatoCartaReference.EntityKey = fc.EntityKey;

//				dbContext.AttachTo( "FormatiCarta", fc );

				//dbContext.FormatiCarta.Attach( r1.formatoCarta );
/*
				Guid pezza = new Guid( "144c0ffe-95ed-4844-8781-70885317535b" );
				r1.formatoCarta = dbContext.FormatiCarta.Single( ff => ff.id == pezza );
*/
				/*
								EntityKey ek1 = dbContext.CreateEntityKey( "RigheCarrelli", r1  );
								r1.EntityKey = ek1;

								EntityKey ek2 = dbContext.CreateEntityKey( "Carrelli", c3 );

								FormatoCarta fc = r1.formatoCarta;
								dbContext.FormatiCarta.Attach( formato );
								dbContext.Fotografi.Attach( r1.fotografo );
								dbContext.Fotografie.Attach( r1.fotografia );

				*/

				
				// The EntityKey property can only be set when the current value of the property is null
				dbContext.Carrelli.Add( c3 );
				dbContext.SaveChanges();
			}

		}



		[TestMethod]
		public void carrelloConPezziStaccati() {

			Carrello c3 = new Carrello { id = Guid.NewGuid(), giornata = DateTime.Today, tempo = DateTime.Now, totaleAPagare = 123m	};
			c3.righeCarrello = new System.Data.Objects.DataClasses.EntityCollection<RigaCarrello>();

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
				RiCaFotoStampata r1 = new RiCaFotoStampata {
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

				RiCaFotoStampata r1 = (RiCaFotoStampata)c3.righeCarrello.ElementAt( 0 );

				dbContext.Fotografie.Attach( r1.fotografia );
				dbContext.FormatiCarta.Attach( r1.formatoCarta );
				dbContext.Fotografi.Attach( r1.fotografo );

				// The EntityKey property can only be set when the current value of the property is null
				dbContext.Carrelli.Add( c3 );
				dbContext.SaveChanges();
			}
		}
	}
}
