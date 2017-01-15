using System;
using Digiphoto.Lumen.Model;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Digiphoto.Lumen.Applicazione;
using System.Data.Entity.Validation;

namespace Digiphoto.Lumen.Core.Test.Model {

	[TestClass]
	public class CrudTest {

		#region inizializzazioni
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void myClassInitialize( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}

		[ClassCleanup]
		public static void myClassCleanup() {
			LumenApplication.Instance.ferma();
		}
		#endregion


		[TestMethod]
		public void crudFotografia() {

			using( LumenEntities context = new LumenEntities() ) {

				Fotografo ff = context.Fotografi.First();
				Evento ee = context.Eventi.FirstOrDefault();

				Fotografia foto = new Fotografia();
				foto.id = Guid.NewGuid();
				foto.dataOraAcquisizione = DateTime.Now;
				foto.fotografo = ff;
				foto.evento = ee;
				foto.didascalia = "TEST";
				foto.nomeFile = "nontelodico.jpg";

				context.Fotografie.Add( foto );

				int test = context.SaveChanges();

				Assert.IsTrue( test > 0 );
			}
		}

		[TestMethod]
		public void crudCarrello() {

			Guid guid = Guid.NewGuid();

			using( LumenEntities context = new LumenEntities() ) {

				// Prima scrivo un carrello da solo.
				Carrello c = new Carrello();
				c.id = guid;
				c.giornata = DateTime.Today;
				c.intestazione = "test1";
				c.prezzoDischetto = 123;
				c.tempo = DateTime.Now;
				c.venduto = false;
				c.totaleAPagare = 234;

				// Creo anche una riga
				RigaCarrello r = new RigaCarrello();
				r.id = Guid.NewGuid();
				r.prezzoLordoUnitario = 12.34m;
				r.quantita = 1;
				r.descrizione = "test case";
				r.prezzoNettoTotale = r.prezzoLordoUnitario;
				r.fotografia = context.Fotografie.FirstOrDefault();
				r.fotografo = r.fotografia.fotografo;
				r.discriminator = RigaCarrello.TIPORIGA_MASTERIZZATA;

				c.righeCarrello = new List<RigaCarrello>();
				c.righeCarrello.Add( r );

				// Creo anche un incasso fotografo
				IncassoFotografo i = new IncassoFotografo();
				i.id = Guid.NewGuid();
				i.fotografo = r.fotografo;
				i.incasso = r.prezzoNettoTotale;
				i.incassoMasterizzate = i.incasso;
				i.contaMasterizzate = 1;

				c.incassiFotografi = new List<IncassoFotografo>();
				c.incassiFotografi.Add( i );

				context.Carrelli.Add( c );

				try {
					context.SaveChanges();
				} catch( DbEntityValidationException qq ) {
					foreach( var item in qq.EntityValidationErrors ) {
						foreach( var item2 in item.ValidationErrors ) {
							String errore = item2.ErrorMessage;
							Console.WriteLine( errore );
						}
					}
					throw;
				} catch( Exception ) {
					throw;
				}
			}

			using( LumenEntities context = new LumenEntities() ) {
				Carrello ct = context.Carrelli.Single( c => c.id == guid );
				Assert.IsTrue( ct.intestazione == "test1" );
				Assert.IsTrue( ct.righeCarrello.Count == 1 );
				ct.intestazione = "test2";
				context.SaveChanges();
			}

			using( LumenEntities context = new LumenEntities() ) {
				Carrello ct = context.Carrelli.Single( c => c.id == guid );
				Assert.IsTrue( ct.intestazione == "test2" );
				context.Carrelli.Remove( ct );
				// Se tutto va bene, mi deve buttare via con il trigger di cascade, anche le RigheCarrello e gli IncassiFotografi
				context.SaveChanges();
			}

			using( LumenEntities context = new LumenEntities() ) {
				Carrello ct = context.Carrelli.SingleOrDefault( c => c.id == guid );
				Assert.IsNull( ct );
			}

		}

	}
}
