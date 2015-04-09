using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Core.VsTest.Util;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.BarCode;
using Digiphoto.Lumen.Servizi.Explorer;
using System.IO;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Core.Database;

namespace Digiphoto.Lumen.Core.VsTest
{
	[TestClass]
	public class BarCodeSrvImplTest 
	{

		private BarCodeSrvImpl _impl;

		[TestInitialize]
		public void Init()
		{

			LumenApplication app = LumenApplication.Instance;
			app.avvia();

			this._impl = new BarCodeSrvImpl();

			IRicercatoreSrv srv2 = app.creaServizio<IRicercatoreSrv>();

		}

		[TestMethod]
		public void searchBarCode()
		{
			int trovati = 0;
			using (new UnitOfWorkScope(false))
			{
				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				List<Fotografia> fotos = dbContext.Fotografie.ToList<Fotografia>();

				trovati = _impl.applicaBarCodeDidascalia(fotos);
			}

			Assert.IsTrue(trovati >= 1);
			
		}

		[TestCleanup]
		public void Cleanup()
		{
			_impl.Dispose();
		}

		
	}
}
