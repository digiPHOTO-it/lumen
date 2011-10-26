using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Database;
using System.Linq;
using System.Transactions;
using System.Data.Objects;
using Digiphoto.Lumen.Model;
using System.Reflection;

namespace Digiphoto.Lumen.Core.VsTest {
	
	[TestClass]
	public class ScaricatoreFotoImplTest : IObserver<ScaricoFotoMsg> {

		private ScaricatoreFotoSrvImpl _impl;
		private bool _puoiTogliereLaFlashCard;

		Fotografo _mario = null;
		Evento _ballo = null;
		Evento _briscola = null;

		

		[TestInitialize]
		public void Init() {

			LumenApplication app = LumenApplication.Instance;
			app.avvia();
			IObservable<ScaricoFotoMsg> observable = app.bus.Observe<ScaricoFotoMsg>();
			observable.Subscribe( this );

			_impl = new ScaricatoreFotoSrvImpl();
			_impl.start();

			

			// -------

			using( LumenEntities dbContext = new LumenEntities() ) {
				// using( TransactionScope transaction = new TransactionScope() ) {

				InfoFissa i = dbContext.InfosFisse.Single<InfoFissa>( f => f.id == "K" );

				_mario = Utilita.ottieniFotografoMario( dbContext );

					// cerco l'evento con la descrizione
					_ballo = dbContext.Eventi.Where
						( "it.descrizione = @descriz", new ObjectParameter(
					 "descriz", "BALLO" ) ).FirstOrDefault<Evento>();


					//_ballo = (from ev in dbContext.Eventi 
					//           where ev.descrizione.Equals("BALLO")
					//           select ev).FirstOrDefault();

					if( _ballo == null ) {
						_ballo = new Evento();
						
						_ballo.descrizione = "BALLO";
						_ballo.id = Guid.NewGuid();
						dbContext.Eventi.AddObject( _ballo );
					}

					_briscola = dbContext.Eventi.Where
						( "it.descrizione = @descriz", new ObjectParameter(
					 "descriz", "BRISCOLA" ) ).FirstOrDefault<Evento>();

					//_briscola = (from ev in dbContext.Eventi
					//          where ev.descrizione.Equals( "BRISCOLA" )
					//          select ev).FirstOrDefault();
					if( _briscola == null ) {
						_briscola = new Evento();
						_briscola.id = Guid.NewGuid();
						_briscola.descrizione = "BRISCOLA";
						dbContext.Eventi.AddObject( _briscola );
					}

					dbContext.SaveChanges();
					// transaction.Complete();
				// }
			}
		}

		[TestCleanup]
		public void Cleanup() {
			_impl.Dispose();
		}


		[TestMethod]
		public void testScaricaFile() {

			Guid guid = Guid.NewGuid();

			String doveSono = Assembly.GetExecutingAssembly().Location;

			string appPath = Path.GetDirectoryName( doveSono );
			string cartella = Path.Combine( appPath, "images" );
			string [] nomiFiles = Directory.GetFiles( cartella , "*.jpg" );


			string dir = creaDirTemp();

			foreach( string nomeSrc in nomiFiles ) {

				FileInfo fiInfo = new FileInfo( nomeSrc );
				string nomeDest = Path.Combine( dir, fiInfo.Name );

				File.Copy( nomeSrc, nomeDest );
			}

			ParamScarica param = new ParamScarica();
			param.cartellaSorgente = dir;
			param.eliminaFilesSorgenti = true;

			

			param.flashCardConfig = new Config.FlashCardConfig( _mario, _ballo );
			_impl.scarica( param );

			while( ! _puoiTogliereLaFlashCard ) {
				Thread.Sleep( 10000 );
			}

			Console.Write( "ok puoi togliere la flash card. Attendere elaborazione in corso ..." );

			while( !_elaborazioneTerminata ) {
				Thread.Sleep( 10000 );
			}
			
	
			Console.WriteLine( "Ecco finito" );
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

	//		Assert.IsFalse( msg.riscontratiErrori );

			// Controllo che i files siano tutti copiati
//			Assert.IsTrue( msg.totFotoCopiateOk == QUANTI_FILES );

			// ok è arrivato il messaggio.
			if( msg.fase == Fase.FineScarico )
				_puoiTogliereLaFlashCard = true;

			if( msg.fase == Fase.FineLavora )
				_elaborazioneTerminata = true;

		}

		public bool _elaborazioneTerminata {
			get;
			set;
		}
	}
}

