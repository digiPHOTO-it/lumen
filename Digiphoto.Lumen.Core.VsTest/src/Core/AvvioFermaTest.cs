using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Servizi.Masterizzare;

namespace Digiphoto.Lumen.Core.VsTest.Core {

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


	}
}
