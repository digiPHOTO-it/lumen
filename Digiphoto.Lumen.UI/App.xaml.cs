using System;
using System.Windows;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.UI.Pubblico;
using log4net;
using Digiphoto.Lumen.Config;
using System.Threading;
using System.Windows.Markup;
using System.Globalization;
using Digiphoto.Lumen.Licensing;
using System.Reflection;

namespace Digiphoto.Lumen.UI {



	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {


		private static readonly ILog _giornale = LogManager.GetLogger( typeof( App ) );

		private static Mutex mutex;

		private static Mutex mutexSingle;

//		private GestoreFinestrePubbliche _gestoreFinestrePubbliche;


		protected override void OnStartup( StartupEventArgs e ) {

			// Faccio partire il log
			log4net.Config.XmlConfigurator.Configure();

			// loggo la versione dell'assembly (la release del software)
			_giornale.Info( "Avvio " + Assembly.GetExecutingAssembly().FullName + " (" + Configurazione.releaseNickname + ")" );
				
			// Senza di questa istruzione, gli StringFormat usati nei binding, usano sempre la cultura americana.
			FrameworkElement.LanguageProperty.OverrideMetadata( typeof( FrameworkElement ), new FrameworkPropertyMetadata( XmlLanguage.GetLanguage( CultureInfo.CurrentCulture.IetfLanguageTag ) ) );

			impostaMutex();

			#if (! DEBUG)			
				// Preparo finestra di attesa
				SplashScreen splashScreen = new SplashScreen( "SplashScreen1.png" );
				splashScreen.Show( false, true );
			#endif
			base.OnStartup(e);


			try {

				// Provo ad iniziare l'applicazione.
				// Se la configurazione è mancante, allora rimando all'apposita gestione
				LumenApplication.Instance.avvia();

				avvisoScadenzaLicenza( 2 );
				
			} catch ( LicenseNotFoundException ) {
				
				// Metto due message box perché la prima non si ferma !
				MessageBox.Show( "Errore nella licenza", "ATTENZIONE" );
				MessageBox.Show( "Non è stata rilevata una licenza valida per l'uso del programma.\nContattare il fornitore del software " + Configurazione.applicationName + "\nper ottenere regolare licenza,\noppure una versione demo gratuita", "Licenza non valida", MessageBoxButton.OK, MessageBoxImage.Error );
				Environment.Exit( 7 );

			} catch( ConfigurazioneMancanteException em ) {

				_giornale.Warn( "Configurazione mancante. Occorre prima creare la configurazione", em );
				MessageBox.Show( em.Message, "ATTENZIONE" );
				MessageBox.Show( "Impossibile avviare l'applicazione adesso!\nOccorre prima creare la configurazione iniziale.\nLanciare il gestore della configurazione!", "Dimenticanza", MessageBoxButton.OK, MessageBoxImage.Exclamation );
				Environment.Exit( 2 );
					
			} catch( ConfigurazioneNonValidaException  nve ) {

				_giornale.Error( "Impossibile avviare applicazione", nve );

				// Metto due message box perché la prima non si ferma !
				MessageBox.Show( nve.Message, "ATTENZIONE" );
				MessageBox.Show( "Impossibile avviare l'applicazione!\nLa configurazione non è valida, oppure\nnon è stata aggiornata dopo un cambio release.\nLanciare apposito programma di gestione configurazione.", "ERRORE non previsto", MessageBoxButton.OK, MessageBoxImage.Error );

				Environment.Exit( 6 );

			} catch( Exception ee ) {
						
				_giornale.Error( "Impossibile avviare applicazione", ee );

				// Metto due message box perché la prima non si ferma !
				MessageBox.Show( ee.Message, "ATTENZIONE" );
				MessageBox.Show( "Impossibile avviare l'applicazione " + Configurazione.applicationName + " !\nErrore bloccante!\nVedere il log", "ERRORE non previsto", MessageBoxButton.OK, MessageBoxImage.Error );
				Environment.Exit( 9 );
			}

			#if (! DEBUG)
				// Chiudo lo splash
				splashScreen.Close( new TimeSpan() );
			#endif

			_giornale.Info("Applicazione avviata. Apro il form principale.");

			// Creo il gestore delle finestre pubbliche e quindi apro la main.
			gestoreFinestrePubbliche = new GestoreFinestrePubbliche();
			gestoreFinestrePubbliche.aprireFinestraMain();

        }

		protected override void OnExit( ExitEventArgs e ) {

			_giornale.Info( "Uscita dall'applicazione" );

			gestoreFinestrePubbliche.chiudereTutteLeFinestre();

			avvisoScadenzaLicenza( 1 );

			LumenApplication.Instance.ferma();

			rilascioMutex();
			
			base.OnExit( e );
		}

		/// <summary>
		/// Imposto i semafori per evitare esecuzioni multiple del programma.
		/// </summary>
		void impostaMutex() {

			// In debug (cioè in sviluppo) mi serve lanciare più volte il prorgramma che testare l'utilizzo in concorrenza.
#if (!DEBUG)

			mutex = new Mutex(true, "Digiphoto.Lumen.UI");
			if (mutex.WaitOne(0, false)) 
			{
				mutexSingle = new Mutex(true, "Digiphoto.Lumen.Single");
				if( mutexSingle.WaitOne( 3000, false ) ) 
				{
				}
				else 
				{
					// Metto due message box perché la prima non si ferma !
					MessageBox.Show( "ATTENZIONE" );
					MessageBox.Show( "L'applicazione di Digiphoto.Lumen.Configuratore è in esecuzione\nChiudere l'Applicazione e Riavviare" );
					Environment.Exit(1);
				} 
			}
			else
			{
				// Metto due message box perché la prima non si ferma !
				MessageBox.Show( "ATTENZIONE" );
				MessageBox.Show( "L'applicazione " + Configurazione.applicationName + " è già in esecuzione" );
				Environment.Exit(3);
			}
#endif
		}

		void rilascioMutex() {
			try {
				if( mutex != null ) {
					mutex.ReleaseMutex();
					mutex.Dispose();
					mutex = null;
				}
			} catch( Exception ) {
				_giornale.Error( "Problema 1 nel rilascio del mutex di lock applicazione" );
			}

			try {
				if( mutexSingle != null ) {
					mutexSingle.ReleaseMutex();
					mutexSingle.Dispose();
					mutexSingle = null;
				}
			} catch( Exception ) {
				_giornale.Error( "Problema 2 nel rilascio del mutex di lock applicazione" );
			}

		}



		private void avvisoScadenzaLicenza( int quanti ) {

			if( LumenApplication.Instance.numGiorniScadenzaLicenza <= 30 )
				for( int ii = 1; ii <= quanti; ii++ )
					MessageBox.Show( "ATTENZIONE !\n\nMancano " + LumenApplication.Instance.numGiorniScadenzaLicenza + " giorni allo scadere della licenza.\nContattare il fornitore del software per rinnovare il contratto.", Configurazione.applicationName , MessageBoxButton.OK, MessageBoxImage.Exclamation );
		}

		public GestoreFinestrePubbliche gestoreFinestrePubbliche {
			get;
			private set;
		}

	}
}
