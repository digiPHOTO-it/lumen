using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Servizi.Vendere;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;
using System.Data.SqlClient;
using Digiphoto.Lumen.Database;

namespace Digiphoto.Lumen.Core.VsTest.src.Servizi.Vendere {

	[TestClass]
	public class GestoreCarrelloTest {

		GestoreCarrello gestoreCarrello;

		Carrello carrello {
			get {
				return gestoreCarrello == null ? null : gestoreCarrello.carrello;
			}
		}

		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void classInitialize( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}

		[ClassCleanup()]
		public static void classCleanup() {
			LumenApplication.Instance.ferma();
		}

		[TestInitialize()]
		public void MyTestInitialize() {

			this.gestoreCarrello = new GestoreCarrello();
		}
		[TestCleanup()]
		public void MyTestCleanup() {
			this.gestoreCarrello.Dispose();
			LumenApplication.Instance.ferma();
		}

		/// <summary>
		/// Creo un nuovo carrello poi lo salvo. 
		/// A questo punto lo 
		/// </summary>
		[TestMethod]
		public void CarrelloTest1() {

			using( new UnitOfWorkScope() ) {

				Carrello carrelloTest = recuperaCarrelloPerTest();

				gestoreCarrello.caricaCarrello( carrelloTest.id );

				// Cambio una info nella testata e una descrizione in un riga
				carrelloTest.note = DateTime.Now.ToString();

				// Cambio anche una nota sulle righe per causare
				foreach( var riga in carrello.righeCarrello ) {
					riga.descrizione = DateTime.Now.ToString();
				}

				int numRigheOrig = carrello.righeCarrello.Count;

				// ora aggiungo una riga in fondo. Prendo una foto caso che non sia già nel carrello
				var fotos = carrello.righeCarrello.Select( r => r.fotografia ).Distinct();
				RigaCarrello riga3 = new RigaCarrello();
				riga3.fotografia = UnitOfWorkScope.currentDbContext.Fotografie.AsEnumerable().Except( fotos ).First();
				riga3.quantita = 1;
				riga3.fotografo = riga3.fotografia.fotografo;
				riga3.formatoCarta = UnitOfWorkScope.currentDbContext.FormatiCarta.First();
				riga3.discriminator = Carrello.TIPORIGA_STAMPA;
				riga3.descrizione = "DACANC";
				gestoreCarrello.aggiungiRiga( riga3 );

				// Prima salvo senza cambiare niente.
				gestoreCarrello.salva();

				// -----
				// Eseguo una query sql con un altra connessione, per vedere che la riga sia stata aggiunta
				object[] parametri = { carrello.id };
				var cntQuery = UnitOfWorkScope.currentDbContext.Database.SqlQuery<int>( "select count(*) from RigheCarrelli where carrello_id = {0}", parametri );
				int numRigheA = cntQuery.First<int>();
				Assert.IsTrue( numRigheA == numRigheOrig + 1 );

				// Ora riprendo il carrello ed elimino la riga 3
				gestoreCarrello.caricaCarrello( carrelloTest.id );
				RigaCarrello rigaDacanc = carrello.righeCarrello.Single( r => r.descrizione == "DACANC" );
				gestoreCarrello.removeRiga( rigaDacanc );

				// Eseguo una query sql con un altra connessione, per vedere che ancora non c'è stato il commit
				cntQuery = UnitOfWorkScope.currentDbContext.Database.SqlQuery<int>( "select count(*) from RigheCarrelli where carrello_id = {0}", parametri );
				int numRigheB = cntQuery.First<int>();
				Assert.IsTrue( numRigheB == numRigheA );


				gestoreCarrello.salva();


				// Eseguo una query sql con un altra connessione, per vedere che la riga sia stata eliminata
				cntQuery = UnitOfWorkScope.currentDbContext.Database.SqlQuery<int>( "select count(*) from RigheCarrelli where carrello_id = {0}", parametri );
				int numRigheC = cntQuery.First<int>();
				Assert.IsTrue( numRigheC == numRigheOrig );

			}

		}

		const string Tag = "#TEST!";

		private Carrello recuperaCarrelloPerTest() {

			Carrello carrello = UnitOfWorkScope.currentDbContext.Carrelli.FirstOrDefault( c => c.intestazione.Contains( Tag ) );
			if( carrello == null ) {
				carrello = creaNuovoCarrelloPerTest();
			} else {
				// Stacco il carrello dalla sessione
				UnitOfWorkScope.currentObjectContext.Detach( carrello );
			}

			return carrello;
		}

		private Carrello creaNuovoCarrelloPerTest() {

			var fotografie = UnitOfWorkScope.currentDbContext.Fotografie.Take( 2 ).ToArray();
			var formatoCarta = UnitOfWorkScope.currentDbContext.FormatiCarta.First();

			gestoreCarrello.creaNuovo();

			int i = 0;
			RigaCarrello riga1 = new RigaCarrello();
			riga1.fotografia = fotografie[i];
			riga1.quantita = 2;
			riga1.fotografo = fotografie[i].fotografo;
			riga1.formatoCarta = formatoCarta;
			riga1.discriminator = Carrello.TIPORIGA_STAMPA;
			riga1.descrizione = "da stampare";
			gestoreCarrello.aggiungiRiga( riga1 );

			++i;
			RigaCarrello riga2 = new RigaCarrello();
			riga2.fotografia = fotografie[i];
			riga2.quantita = 1;
			riga2.fotografo = fotografie[i].fotografo;
			riga2.discriminator = Carrello.TIPORIGA_MASTERIZZATA;
			riga2.descrizione = "da masterizzare";
			gestoreCarrello.aggiungiRiga( riga2 );

			gestoreCarrello.carrello.intestazione = Tag;
			gestoreCarrello.carrello.prezzoDischetto = 15.5m;
			gestoreCarrello.salva();

			return gestoreCarrello.carrello;
		}

		/// <summary>
		/// Con questo test verifico che il delete-cascade sulle righe funzioni.
		/// </summary>
		[TestMethod]
		public void CarrelloTestCancella2() {

			using( new UnitOfWorkScope() ) {

				Carrello carrelloTest = recuperaCarrelloPerTest();

				// Eseguo una query sql con un altra connessione, per vedere che ci siano le righe
				object[] parametri = { carrelloTest.id };
				const string sqlQuery = "select count(*) from RigheCarrelli where carrello_id = {0}";

				var cntQuery = UnitOfWorkScope.currentDbContext.Database.SqlQuery<int>( sqlQuery, parametri );
				int numRigheA = cntQuery.First<int>();
				Assert.IsTrue( numRigheA > 0 );

				gestoreCarrello.elimina( carrelloTest );

				cntQuery = UnitOfWorkScope.currentDbContext.Database.SqlQuery<int>( sqlQuery, parametri );
				int numRigheC = cntQuery.First<int>();
				Assert.IsTrue( numRigheC == 0 );
			}
		}
	}
}
