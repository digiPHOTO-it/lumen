using System;
using Digiphoto.Lumen.Model;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digiphoto.Lumen.Core.VsTest {

	[TestClass]
	public class CrudTest {

	
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
	}
}
