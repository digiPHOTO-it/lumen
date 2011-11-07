using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Model;
using System.Data.EntityClient;
using System.Data.Common;
using System.Data;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Data.Linq;

namespace Digiphoto.Lumen.Core.VsTest.Util {

	[TestClass]
	public class QueriesVarieTest {

		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void MyClassInitialize( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}

		[TestMethod]
		public void TestQuery1() {


			using( LumenEntities dbContext = new LumenEntities() ) {
				
				// modalità di query di tipo LINQ 
				var chiavi =
					from f
					in dbContext.Fotografie
					where 1==1
					select f.id;


				// -----

				// carico una lista di ID per provare una query con il Contains
				Guid [] chiaviArray = chiavi.ToArray();

				var fotos = from f2
								in dbContext.Fotografie
							where chiaviArray.Contains( f2.id )
							select f2;

				IEnumerator<Fotografia> itera = fotos.GetEnumerator();
				itera.MoveNext();
				var campione = itera.Current;

				// -- provo a fare una query con tutta l'entità

				try {
					var foto3 = from f3
								in dbContext.Fotografie
								where f3.Equals( campione )
								select f3;
					Assert.Fail();
				} catch( Exception ) {
					// deve dare errore perchè nelle query si possono usare solo tipi primitivi per l'uguaglianza
					//    :-((   bleah!!!
					// Microsoft promette che nelle prossime versioni sarà implementato. A quel punto si potrà usare.
				}

			}
		}


		[TestMethod]
		public void testQueryEsql() {

			using( LumenEntities dbContext = new LumenEntities() ) {
				
				dbContext.Connection.Open();

				// ----- Ora provo in eSql
				string esql = @"SELECT VALUE f
                              FROM LumenEntities.Fotografie
                              as f
                              WHERE f.numero < @quanti";

				DbCommand comando = dbContext.Connection.CreateCommand();
				comando.CommandText = esql;
				EntityParameter pquanti = new EntityParameter( "quanti", DbType.Int16 );
				pquanti.Value = 3;

				comando.Parameters.Add( pquanti );
				using( DbDataReader rdr = comando.ExecuteReader( CommandBehavior.SequentialAccess ) ) {
					// Iterate through the collection of Contact items.
					while( rdr.Read() ) {
						Console.WriteLine( rdr ["nomefile"] );
					}
				}

				// -----

				// The following query returns a collection of Fotografia objects.
				int [] vettore = { 2, 4, 6 };
				string listaNum = string.Join( ", ", vettore.Select( r => r.ToString()  ).ToArray() );
				string esql2 = @"SELECT VALUE f
                              FROM LumenEntities.Fotografie
                              as f
                              WHERE f.numero in {" + listaNum + "}";

				ObjectQuery<Fotografia> query2 = new ObjectQuery<Fotografia>( esql2, dbContext, MergeOption.NoTracking );
				foreach( Fotografia f in query2 ) {
					Console.WriteLine( f.nomeFile );
				}
			}
		}

		[TestMethod]
		public void testNonScalar() {
			using( LumenEntities dbContext = new LumenEntities() ) {

				Evento evento = dbContext.Eventi.First();
				Fotografia foto = dbContext.Fotografie.First();
				foto.evento = evento;

				// ----- Ora provo in eSql
				string esql = @"SELECT VALUE f
                              FROM LumenEntities.Fotografie as f
                              WHERE f.evento = @evento";

				ObjectQuery<Fotografia> query = new ObjectQuery<Fotografia>( esql, dbContext );
				try {
					query.Parameters.Add( new ObjectParameter( "evento", evento ) );

					IList<Fotografia> ris = query.ToList();
					int quanti = ris.Count();
					Assert.Fail();  // purtroppo entity framework ancora non gestisce questa cosa.
				} catch( Exception ) {
					// purtroppo entity framework ancora non gestisce questa cosa.
				}
			}
		}


		[TestMethod]
		public void testRuntimeLinqQuery() {

			using( LumenEntities dbContext = new LumenEntities() ) {

				Evento ev = dbContext.Eventi.First();

				Fotografia foto = dbContext.Fotografie.First();
				foto.evento = ev;

				// ---
				try {
					var query = from f in dbContext.Fotografie
					            where ev.Equals( f.evento )
					            select f;

					IList<Fotografia> ris = query.ToList();
					int quanti = ris.Count();
					Assert.Fail();  // purtroppo entity framework ancora non gestisce questa cosa.
				} catch( Exception ) {
					// purtroppo entity framework ancora non gestisce questa cosa.
				}
			}
		}


		[TestMethod]
		public void soloLinqToEntities() {

			using( LumenEntities dbContext = new LumenEntities() ) {

				IQueryable<Fotografia> query = from ff in dbContext.Fotografie 
											   select ff;

				var evento = dbContext.Eventi.First();

				int [] numeri = { 1, 2, 3, 4, 6 };

				if( 1 == 1 )
					query = query.Where( ff => numeri.Contains( ff.numero ) );

				if( 1 == 1 )
					query = query.Where( ff => evento.id == ff.evento.id );

				
				var risultato = query.Select( ff => ff );
				ObjectQuery<Fotografia> oq = (ObjectQuery<Fotografia>)query;
				string s = oq.ToTraceString();
				var lista = risultato.ToList();
			}
		}

	}
}

