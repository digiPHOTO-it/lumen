using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Servizi.Masterizzare;
using Digiphoto.Lumen.Model;
using System.IO;
using System.Data.SqlClient;

namespace Digiphoto.Lumen.Core.Test.Core {

	/**
	 * Apro e chiudo l'applicazione avvio e fermo i servizi di base
	 */
	[TestClass]
	public class AvvioFermaTest {

		[TestMethod]
		public void avviaFermaTest() {

			LumenApplication.Instance.avvia();
			
			Assert.IsTrue( LumenApplication.Instance.stato.giornataLavorativa.Equals( DateTime.Today ) );

			IServizio srv = LumenApplication.Instance.getServizioAvviato<IVolumeCambiatoSrv>();
			Assert.IsTrue( srv.isRunning );
			
			srv.stop();
			Assert.IsFalse( srv.isRunning );

			srv.start();
			Assert.IsTrue( srv.isRunning );

			LumenApplication.Instance.ferma();
			Assert.IsFalse( srv.isRunning );
		}

		[TestMethod]
		public void avviaFermaTest2() {

			LumenApplication.Instance.avvia();

			IServizio srv = LumenApplication.Instance.creaServizio<IMasterizzaSrv>();
			srv.start();
			Assert.IsTrue( srv.isRunning );
			srv.Dispose();

			LumenApplication.Instance.ferma();
		}


		[TestMethod]
		public void crudFotografo() {

			using( LumenEntities context = new LumenEntities() ) {

				Fotografo ff = new Fotografo();

				ff.id = "Barna " + DateTime.Now.ToString( "HH-mm-ss" );
				ff.cognomeNome = "Bernardini Luca";
				ff.iniziali = "BL";

				context.Fotografi.Add( ff );

                using( var stream = new MemoryStream() ) {
					Properties.Resources.Image01.Save( stream, System.Drawing.Imaging.ImageFormat.Jpeg );
					ff.immagine = stream.ToArray();
				}

				int test = context.SaveChanges();

				Assert.IsTrue( test > 0 );
			}
		}

		[TestMethod]
		public void crudFormatiCarta() {

			using( LumenEntities context = new LumenEntities() ) {

				Random r = new Random();
				
				FormatoCarta fc = new FormatoCarta();

				fc.id = Guid.NewGuid();
                fc.descrizione = "Test-" + r.Next( 1000, 9999 );
				fc.prezzo = r.Next( 1, 50 );
				fc.attivo = true;
				fc.ordinamento = (short) r.Next( 1, 100 );
                context.FormatiCarta.Add( fc );

				int test = context.SaveChanges();

				Assert.IsTrue( test > 0 );

				var trovato = context.FormatiCarta.Any( c => c.descrizione.StartsWith("Test-") );
				Assert.IsTrue( trovato );

				FormatoCarta carta = context.FormatiCarta.First( c => c.descrizione.StartsWith( "Test-" ) );
				Assert.IsNotNull( carta );				
			}
		}

		[TestMethod]
		public void crudAzioneAuto() {

			using( LumenEntities context = new LumenEntities() ) {

				Random r = new Random();

				AzioneAuto aa = new AzioneAuto();

				aa.id = Guid.NewGuid();
				aa.nome = "Test-" + r.Next( 1000, 9999 );
				aa.attivo = false;
				aa.correzioniXml = "Questa è una prova";
				context.AzioniAutomatiche.Add( aa );

				int test = context.SaveChanges();

				Assert.IsTrue( test > 0 );
			}
		}


		[TestMethod]
		public void crudEvento() {

			using( LumenEntities context = new LumenEntities() ) {

				Random r = new Random();

				Evento ev = new Evento();

				ev.id = Guid.NewGuid();
				ev.descrizione = "Test-" + r.Next( 1000, 9999 );
				ev.attivo = false;

				context.Eventi.Add( ev );

				int test = context.SaveChanges();

				Assert.IsTrue( test > 0 );
			}
		}

		[TestMethod]
		public void crudFotografia2() {

			using( LumenEntities context = new LumenEntities() ) {

				Random r = new Random();

				Fotografia f = new Fotografia();

				f.id = Guid.NewGuid();
				f.didascalia = "Test-" + r.Next( 1000, 9999 );
				f.numero = r.Next( 1, 1000000 );
				f.nomeFile = "Test";
				f.dataOraAcquisizione = DateTime.Now;
				f.giornata = DateTime.Today;

				f.fotografo = context.Fotografi.First();
				f.evento = context.Eventi.FirstOrDefault();

				context.Fotografie.Add( f );


				int test = context.SaveChanges();

				Assert.IsTrue( test > 0 );
			}
		}

		[TestMethod]
		public void crudConsumoCartaGG() {

			using( LumenEntities context = new LumenEntities() ) {

				Random r = new Random();

				ConsumoCartaGiornaliero cc = new ConsumoCartaGiornaliero();

				cc.id = Guid.NewGuid();
				cc.diCuiFoto = (short)r.Next( 1, 999 );
				cc.diCuiProvini = (short)r.Next( 1, 999 );
				cc.totFogli = (short)r.Next( 1, 999 );
				cc.giornata = DateTime.Today;
				cc.formatoCarta = context.FormatiCarta.First();

				context.ConsumiCartaGiornalieri.Add( cc );
				
				int test = context.SaveChanges();

				Assert.IsTrue( test > 0 );
			}
		}

		[TestMethod]
		public void crudScaricoCard() {

			using( LumenEntities context = new LumenEntities() ) {

				Random r = new Random();

				ScaricoCard sc = new ScaricoCard();

				sc.id = Guid.NewGuid();
				sc.totFoto = (short)r.Next( 1, 999 );
				sc.giornata = DateTime.Today;
				sc.fotografo = context.Fotografi.First();
				sc.tempo = DateTime.Now;

				context.ScarichiCards.Add( sc );

				int test = context.SaveChanges();

				Assert.IsTrue( test > 0 );
			}
		}

		[TestMethod]
		public void crudGiornata() {

			using( LumenEntities context = new LumenEntities() ) {

				Random r = new Random();

				Giornata gg = new Giornata();

				gg.id = DateTime.Today;
				gg.firma = "il Barna";
				gg.incassoDichiarato = decimal.Parse( "12345,67" );
				gg.incassoPrevisto = decimal.Parse( "12345,67" );
				gg.orologio = DateTime.Now;
				gg.totScarti = (short)r.Next( 1, 50 );

				context.Giornate.Add( gg );

				int test = context.SaveChanges();

				Assert.IsTrue( test > 0 );
			}
		}

		[TestMethod]
		public void crudCarrello2() {

			using( LumenEntities context = new LumenEntities() ) {

				Random r = new Random();

				Carrello cc = new Carrello();

				cc.id = Guid.NewGuid();
				cc.giornata = DateTime.Today;
				cc.tempo = DateTime.Today;
				cc.intestazione = "Test-" + r.Next( 1000, 9999 );
				cc.note = "Prova";
				cc.prezzoDischetto = r.Next( 1, 90 );
				cc.totaleAPagare = r.Next( 10, 80 );
				cc.totMasterizzate = (short) r.Next( 1, 100 );


				RigaCarrello rc = new RigaCarrello();
				rc.id = Guid.NewGuid();
				rc.carrello = cc;
				rc.descrizione = "Test-" + r.Next( 1000, 9999 );
				rc.discriminator = "S";
				rc.formatoCarta = context.FormatiCarta.First();
				rc.fotografo = context.Fotografi.First();
				rc.prezzoLordoUnitario = r.Next( 1, 12 );
				rc.quantita = (short)r.Next( 1, 12 );
				rc.prezzoNettoTotale = rc.prezzoLordoUnitario * rc.quantita;
				rc.totFogliStampati = 1;
				rc.sconto = (decimal)r.Next( 1, 12 );
				rc.fotografia = context.Fotografie.First();
				rc.bordiBianchi = true;
                cc.righeCarrello.Add( rc );

				context.Carrelli.Add( cc );

				int test = context.SaveChanges();

				Assert.IsTrue( test > 0 );
			}
		}


		[TestMethod]
		public void paginazioneDirettaSql() {

			int sizePag = 20;	// foto per pagina

			using( LumenEntities context = new LumenEntities() ) {

				SqlParameter p1 = new SqlParameter( "giornata", System.Data.SqlDbType.Date );
				p1.Value =  DateTime.Parse( "2017-02-27" );

				SqlParameter p2 = new SqlParameter( "numero", System.Data.SqlDbType.Int );
				p2.Value = 74;

//				object[] parametri = { p1, p2 };
//				object[] parametri = { DateTime.Parse( "2017-02-27" ), (int)74 };
				object[] sqlParams = { "6f588174-3768-49d7-a096-2a537cc3b5e6" };


				context.Database.BeginTransaction();

				string sql1 = "SELECT count( f.id ) FROM Fotografie f  where 1=1 AND f.evento_id = {0} ";
				var query1 = context.Database.SqlQuery<int>( sql1, sqlParams );
				var quanti = query1.Single();


				//				var isCompatibile = context.Database.CompatibleWithModel( true );
				//				Console.Out.WriteLine( isCompatibile );

				for( int pagina = 1; pagina < 10; pagina++ ) {

// where f.giornata = @giornata and f.numero = @numero

					string sql = @"SELECT f.id
				             FROM `fotografie` as f 
                             INNER JOIN `Fotografi` AS f2 ON f.`fotografo_id` = `f2`.`id` 
                             LEFT OUTER JOIN `Eventi` AS `ev` 
							              ON `f`.`evento_id` = `ev`.`id` 
							 where f.evento_id = {0}
                             order by `dataOraAcquisizione` desc, `numero` DESC 
                             LIMIT " + ((pagina - 1) * sizePag) + ", " + sizePag;

					System.Diagnostics.Debug.WriteLine( "\n------\nTempo1: " + DateTime.Now.ToString( "mm:ss:fff" ) );

					var ris = context.Database.SqlQuery<Guid>( sql, sqlParams );

					System.Diagnostics.Debug.WriteLine( "Tempo2: " + DateTime.Now.ToString( "mm:ss:fff" ) );

					var lista = ris.ToList();

					System.Diagnostics.Debug.WriteLine( "Tempo3: " + DateTime.Now.ToString( "mm:ss:fff" ) );

					System.Diagnostics.Debug.WriteLine( lista.Count );
				}

				System.Diagnostics.Debug.WriteLine( "finito" );
			}
		}

}
}
