using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Core.VsTest {
	[TestClass]
	public class CarrelloTest {

		[TestInitialize]
		public void Init() {
			LumenApplication.Instance.avvia();
		}		


		[TestMethod]
		public void TestCarrello() {

			using( LumenEntities dbContext = new LumenEntities() ) {

				Carrello c1 = new Carrello();
				c1.id = Guid.NewGuid();
				c1.giornata = DateTime.Today;
				c1.tempo = DateTime.Now;
				c1.totaleAPagare = 123m;
				c1.righeCarrello = new System.Data.Objects.DataClasses.EntityCollection<RigaCarrello>();
				
				// ---

				RiCaDiscoMasterizzato r1 = new RiCaDiscoMasterizzato();
				r1.id = Guid.NewGuid();
				r1.prezzoLordoUnitario = new Decimal( 20 );
				r1.quantita = 2;
				r1.prezzoNettoTotale = Decimal.Multiply( r1.prezzoLordoUnitario, r1.quantita );
				r1.descrizione = "RiCaDiscoMasterizzato";
				r1.totFotoMasterizzate = 85;
				c1.righeCarrello.Add( r1 );

				// ---

				RiCaFotoStampata r2 = new RiCaFotoStampata();
				r2.id = Guid.NewGuid();
				r2.prezzoLordoUnitario = new Decimal( 5 );
				r2.quantita = 3;
				r2.prezzoNettoTotale = Decimal.Multiply( r2.prezzoLordoUnitario, r2.quantita );
				r2.descrizione = "RicaFotoStampata1";
				r2.totFogliStampati = 3;
				r2.formatoCarta = Utilita.ottieniFormatoCartaA4( dbContext );
				r2.fotografo = Utilita.ottieniFotografoMario( dbContext );
				c1.righeCarrello.Add( r2 );


				// ---

				RiCaFotoStampata r3 = new RiCaFotoStampata();
				r3.id = Guid.NewGuid();
				r3.prezzoLordoUnitario = new Decimal( 5 );
				r3.quantita = 2;
				r3.prezzoNettoTotale = Decimal.Multiply( r3.prezzoLordoUnitario, r3.quantita );
				r3.descrizione = "RicaFotoStampata1";
				r3.totFogliStampati = 3;
				r3.formatoCarta = Utilita.ottieniFormatoCartaA4( dbContext );
				r3.fotografo = Utilita.ottieniFotografoMario( dbContext );
				c1.righeCarrello.Add( r3 );




				// ---
				dbContext.Carrelli.AddObject( c1 );

				dbContext.SaveChanges();


				// Ora faccio una query cerco solo le foto stampate in questa ultima sessione
				decimal somma = dbContext.RigheCarrelli.Sum(p => Math.Abs(p.prezzoNettoTotale));

				IQueryable<RigaCarrello> esito = from c in dbContext.Carrelli.Include( "righeCarrelli" )
													  from r in c.righeCarrello
													  select r;

				foreach( RigaCarrello r in esito ) {
					System.Diagnostics.Trace.WriteLine( r.GetType().Name );
				}

				
			}


		}


		[TestCleanup]
		public void Cleanup() {

			LumenApplication.Instance.ferma();
		}

	}
}
