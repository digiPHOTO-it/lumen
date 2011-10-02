using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Servizi.Scaricatore;
using System.IO;
using System.Threading;

namespace Digiphoto.Lumen.Core.NunitTest.Servizi {

	[TestFixture]
	class ScaricatoreFotoSrvImplTest : IObserver<ScaricoFotoMsg> {

		private ScaricatoreFotoSrvImpl _impl;
		private bool _finalmenteArrivato;
		private static readonly int QUANTI_FILES = 10;

		[TestFixtureSetUp]
		public void Init() {
			LumenApplication app = LumenApplication.Instance;
			app.avvia();
			IObservable<ScaricoFotoMsg> observable = app.bus.Observe<ScaricoFotoMsg>();
			observable.Subscribe( this );

			_impl = new ScaricatoreFotoSrvImpl();
			_impl.start();
		}

		[Test]
		public void testScaricaFile() {

			string dir = creaDirTemp();
			for( int ii = 1; ii <= QUANTI_FILES; ii++ ) {

				string nomeFile = Path.Combine( dir, "immagine" + ii + ".jpg" );
				StreamWriter SW = File.CreateText( nomeFile );
				SW.WriteLine( "Questa è l'immagine numero " + ii );
				SW.Close();
			}

			ParamScarica param = new ParamScarica();
			param.cartellaSorgente = dir;
			param.eliminaFilesSorgenti = true;
			param.flashCardConfig = new Config.FlashCardConfig( "CC", "BALLO" );
			_impl.scarica( param );

			while( ! _finalmenteArrivato ) {
				Thread.Sleep( 2000 );
			}

		}

		private string creaDirTemp() {
			string path = Path.GetRandomFileName();
			string tempDir = Path.Combine( Path.GetTempPath(), path );
			Directory.CreateDirectory( tempDir );
			return tempDir;
		}

		public void OnCompleted() {
			throw new NotImplementedException();
		}

		public void OnError( Exception error ) {
			throw new NotImplementedException();
		}

		public void OnNext( ScaricoFotoMsg msg ) {

			// ok è arrivato il messaggio.
			_finalmenteArrivato = true;

			
			// Controllo che i files siano tutti copiati
			Assert.True( msg.totFotoCopiateOk == QUANTI_FILES );
			

			
		}
	}
}
