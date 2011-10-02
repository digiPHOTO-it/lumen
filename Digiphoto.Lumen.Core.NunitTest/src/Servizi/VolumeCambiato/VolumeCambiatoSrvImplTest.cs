using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.VolumeCambiato;

namespace Digiphoto.Lumen.Core.Servizi.VolumeCambiato {

	[TestFixture]
	public class VolumeCambiatoSrvImplTest {

		private VolumeCambiatoSrvImpl _impl = null;

		[TestFixtureSetUp]
		public void Init() {
			LumenApplication l = Lumen.Applicazione.LumenApplication.Instance;
			_impl = new VolumeCambiatoSrvImpl();
			_impl.start();
			Assert.IsTrue( _impl.isRunning );
		}

		[Test]
		public void testAttesaEventi() {
			
			Console.Write( "Togliere la chiavetta e premi INVIO" );
			Console.ReadLine();

			Console.Write( "Ora inserisci la chiavetta e premi INVIO" );

			_impl.attesaBloccante = true;

			_impl.attesaEventi();

			Assert.IsNotNull( _impl.ultimoDriveMontato );
		}

		[TestFixtureTearDown]
		public void TearDown() {
			Assert.IsTrue( _impl.isRunning );
			_impl.Dispose();
			Assert.IsFalse( _impl.isRunning );
		}


	}
}