using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Database;
using System.Linq;
using System.Transactions;
using System.Data.Objects;
using Digiphoto.Lumen.Model;
using System.Reflection;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Core.VsTest.Servizi.Scaricatore {
	
	[TestClass]
	public class ScaricatoreFotoImplTest : IObserver<ScaricoFotoMsg> {

		private ScaricatoreFotoSrvImpl _impl;
		private bool _puoiTogliereLaFlashCard;

		Fotografo _mario = null;
		Fotografo _artista = null;
		Evento _ballo = null;
		Evento _briscola = null;
		bool _rimaniQui = true;

		

		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void MyClassInitialize( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}

		[TestInitialize]
		public void Init() {

			LumenApplication app = LumenApplication.Instance;
			IObservable<ScaricoFotoMsg> observable = app.bus.Observe<ScaricoFotoMsg>();
			observable.Subscribe( this );

			_impl = new ScaricatoreFotoSrvImpl();
			_impl.start();

			

			// -------
			using( LumenEntities dbContext = new LumenEntities() ) {

				InfoFissa i = dbContext.InfosFisse.Single<InfoFissa>( f => f.id == "K" );

				_mario = Utilita.ottieniFotografoMario( dbContext );

				// Se hai fatto bene la configurazione, il fotografo artista deve sempre esistere
				_artista = dbContext.Fotografi.Single( f => f.id == Configurazione.ID_FOTOGRAFO_ARTISTA );

					// cerco l'evento con la descrizione
				_ballo = (from e in dbContext.Eventi
						  where e.descrizione == "BALLO"
						  select e).FirstOrDefault();

				if( _ballo == null ) {
					_ballo = new Evento();
						
					_ballo.descrizione = "BALLO";
					_ballo.id = Guid.NewGuid();
					dbContext.Eventi.Add( _ballo );
				}

				_briscola = (from e in dbContext.Eventi
							 where e.descrizione == "BRISCOLA"
								 select e).FirstOrDefault();

				if( _briscola == null ) {
					_briscola = new Evento();
					_briscola.id = Guid.NewGuid();
					_briscola.descrizione = "BRISCOLA";
					dbContext.Eventi.Add( _briscola );
				}

				dbContext.SaveChanges();
			}
		}

		[TestCleanup]
		public void Cleanup() {
			_impl.Dispose();
		}


		[TestMethod]
		public void scaricaCartellaTest() {

			Guid guid = Guid.NewGuid();

			String doveSono = Assembly.GetExecutingAssembly().Location;

			string appPath = Path.GetDirectoryName( doveSono );
			string cartella = Path.Combine( appPath, "images" );
			string [] nomiFiles = Directory.GetFiles( cartella , "*.jpg" );


			string dir = PathUtil.createTempDirectory();

			foreach( string nomeSrc in nomiFiles ) {

				FileInfo fiInfo = new FileInfo( nomeSrc );
				string nomeDest = Path.Combine( dir, fiInfo.Name );

				File.Copy( nomeSrc, nomeDest );
			}

			ParamScarica param = new ParamScarica();
			param.cartellaSorgente = dir;
			param.eliminaFilesSorgenti = true;
			param.faseDelGiorno = FaseDelGiorno.Pomeriggio;

			

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

		[TestMethod]
		public void scaricaFileTest() {



			Guid guid = Guid.NewGuid();

			String doveSono = Assembly.GetExecutingAssembly().Location;

			string appPath = Path.GetDirectoryName( doveSono );
			string cartella = Path.Combine( appPath, "images" );
			string nomeSrc = Directory.GetFiles( cartella, "*.jpg" ).ElementAt( 0 );

			FileInfo fiInfo = new FileInfo( nomeSrc );

			ParamScarica param = new ParamScarica();
			param.nomeFileSingolo = nomeSrc;
			param.cartellaSorgente = null;
			param.eliminaFilesSorgenti = false;


			param.flashCardConfig = new Config.FlashCardConfig( _artista );
			_impl.scarica( param );

			while( !_puoiTogliereLaFlashCard ) {
				Thread.Sleep( 10000 );
			}

			Console.Write( "ok puoi togliere la flash card. Attendere elaborazione in corso ..." );

			while( !_elaborazioneTerminata ) {
				Thread.Sleep( 10000 );
			}

			Console.WriteLine( "Ecco finito" );
		}

		/// <summary>
		/// Provo ad aprire e chiudere il servizio dicendo di scaricare da una cartella vuota
		/// </summary>
		[TestMethod]
		public void scaricatoreApriChiudi() {

			for( int ii = 0; ii < 10; ii++ ) {

				using( IScaricatoreFotoSrv srv = LumenApplication.Instance.creaServizio<IScaricatoreFotoSrv>() ) {

					srv.start();

					// creo una cartella vuota temporanea che poi andrò a buttare
					string cartella = PathUtil.createTempDirectory();

					ParamScarica param = new ParamScarica();
					param.cartellaSorgente = cartella;
					param.flashCardConfig = new Config.FlashCardConfig {
						idFotografo = Configurazione.ID_FOTOGRAFO_ARTISTA
					};

					srv.scarica( param );

					System.Diagnostics.Trace.WriteLine( "Attendo un pò di secondi" );
					Thread.Sleep( 3000 );

					srv.stop();

					Directory.Delete( cartella );
				}
			}
		}

		[TestMethod]
		public void workerApriChiudi() {

			for( int ii = 0; ii < 20; ii++ ) {

				// creo una cartella vuota temporanea che poi andrò a buttare
				string cartella = PathUtil.createTempDirectory();

				ParamScarica param = new ParamScarica();
				param.cartellaSorgente = cartella;
				param.flashCardConfig = new Config.FlashCardConfig {
					idFotografo = Configurazione.ID_FOTOGRAFO_ARTISTA
				};

				_rimaniQui = true;
				using( CopiaImmaginiWorker wkr = new CopiaImmaginiWorker( param, elaboraNessunaImmagine ) ) {

					wkr.Start();
					do {
						Thread.Sleep( 500 );
					} while ( _rimaniQui );

					Directory.Delete( cartella );
					wkr.Stop();
				}
			}
		}

		void elaboraNessunaImmagine( EsitoScarico esitoScarico ) {
			_rimaniQui = false;
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
			if( msg.fase == FaseScaricoFoto.FineScarico )
				_puoiTogliereLaFlashCard = true;

			if( msg.fase == FaseScaricoFoto.FineLavora ) {
				_puoiTogliereLaFlashCard = true;
				_elaborazioneTerminata = true;
			}

		}

		public bool _elaborazioneTerminata {
			get;
			set;
		}


	}
}

