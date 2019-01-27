using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using System.Data.Entity.Core.EntityClient;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using System.Data.Entity.Core.Objects;
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
using Digiphoto.Lumen.Core.Test.Util;

namespace Digiphoto.Lumen.Core.Test.Model {



	[TestClass]
	public class PromoPolimorfTest {

		int _contaStampate = 0;
		int _contaMasterizzate = 0;
		Carrello _carrelloInserito = null;


		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void PromoPolimorfTestInitialize( TestContext testContext ) {
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
		public void DoPromoPolimorfTest() {

			using( LumenEntities dbContext = new LumenEntities() ) {

				PromoStessaFotoSuFile p1 = (PromoStessaFotoSuFile)dbContext.Promozioni.SingleOrDefault( p => p.id == 1 );

				bool nuovo1 = (p1 == null);
				if( nuovo1 ) {
					p1 = new PromoStessaFotoSuFile();
				}
				p1.id = 1;
				p1.descrizione = "prova1";
				p1.prezzoFile = 1;

				if( nuovo1 )
					dbContext.Promozioni.Add( p1 );

				//


				PromoPrendiNPaghiM p2 = (PromoPrendiNPaghiM)dbContext.Promozioni.SingleOrDefault( p => p.id == 2 );

				bool nuovo2 = (p2 == null);
				if( nuovo2 ) {
					p2 = new PromoPrendiNPaghiM();
				}
				p2.id = 2;
				p2.descrizione = "prova2";
				p2.qtaDaPrendere = 6;
				p2.qtaDaPagare = 5;

				if( nuovo2 )
					dbContext.Promozioni.Add( p2 );


				dbContext.SaveChanges();

			}

		}


		[TestMethod]
		public void listaProdottiESurrogatiTest() {

			using( LumenEntities dbContext = new LumenEntities() ) {

				int qTutti = dbContext.Prodotti.Count();
				Console.WriteLine( "tot prodotti : " + qTutti );
				foreach( Prodotto prod in dbContext.Prodotti ) {
					Console.WriteLine( "Prodotto: " + prod.descrizione );
				}

				int qStampe = dbContext.FormatiCarta.Count();
				Console.WriteLine( "tot stampe : " + qStampe );
				foreach( Prodotto prod in dbContext.FormatiCarta ) {
					Console.WriteLine( "Carta: " + prod.descrizione );
				}

				int qFile = dbContext.ProdottiFile.Count();
				Console.WriteLine( "tot file : " + qFile );
				foreach( Prodotto prod in dbContext.ProdottiFile ) {
					Console.WriteLine( "File: " + prod.descrizione );
				}

			}

		}
	}
}
