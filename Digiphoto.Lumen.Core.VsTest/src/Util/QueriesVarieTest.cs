using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Core.VsTest.Util {

	[TestClass]

	public class QueriesVarieTest {
	
		[TestMethod]
		public void TestQuery1() {

			LumenApplication.Instance.avvia();

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
				} catch( Exception ee ) {
					// deve dare errore perchè nelle query si possono usare solo tipi primitivi per l'uguaglianza
					//    :-((   bleah!!!
				}
			}

		}
	}
}
