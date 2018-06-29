using System;
using System.Windows;
using System.Windows.Markup;
using System.Globalization;
using System.Reflection;
using log4net;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Licensing;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.OnRideUI {

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( App ) );

		protected override void OnStartup( StartupEventArgs e ) {

			// Faccio partire il log
			log4net.Config.XmlConfigurator.Configure();

			// loggo la versione dell'assebly (la release del software)
			_giornale.Info( "Avvio " + Assembly.GetExecutingAssembly().FullName );

			// Senza di questa istruzione, gli StringFormat usati nei binding, usano sempre la cultura americana.
			FrameworkElement.LanguageProperty.OverrideMetadata( typeof( FrameworkElement ), new FrameworkPropertyMetadata( XmlLanguage.GetLanguage( CultureInfo.CurrentCulture.IetfLanguageTag ) ) );


			base.OnStartup( e );


			try
			{

				// Provo ad iniziare l'applicazione.
				// Se la configurazione è mancante, allora rimando all'apposita gestione
				LumenApplication.Instance.avvia();



			} catch( LicenseNotFoundException ) {

				// Metto due message box perché la prima non si ferma !
				MessageBox.Show( "Errore nella licenza", "ATTENZIONE" );
				MessageBox.Show( "Non è stata rilevata una licenza valida per l'uso del programma.\nContattare il fornitore del software " + Configurazione.applicationName + "\nper ottenere regolare licenza,\noppure una versione demo gratuita", "Licenza non valida", MessageBoxButton.OK, MessageBoxImage.Error );
				Environment.Exit( 7 );

			} catch( ConfigurazioneMancanteException em ) {

				_giornale.Warn( "Configurazione mancante. Occorre prima creare la configurazione", em );
				MessageBox.Show( em.Message, "ATTENZIONE" );
				MessageBox.Show( "Impossibile avviare l'applicazione adesso!\nOccorre prima creare la configurazione iniziale.\nLanciare il gestore della configurazione!", "Dimenticanza", MessageBoxButton.OK, MessageBoxImage.Exclamation );
				Environment.Exit( 2 );

			} catch( ConfigurazioneNonValidaException nve )
			{

				_giornale.Error( "Impossibile avviare applicazione", nve );

				// Metto due message box perché la prima non si ferma !
				MessageBox.Show( nve.Message, "ATTENZIONE" );
				MessageBox.Show( "Impossibile avviare l'applicazione!\nLa configurazione non è valida, oppure\nnon è stata aggiornata dopo un cambio release.\nLanciare apposito programma di gestione configurazione.", "ERRORE non previsto", MessageBoxButton.OK, MessageBoxImage.Error );

				Environment.Exit( 6 );

			} catch( Exception ee )
			{

				_giornale.Error( "Impossibile avviare applicazione", ee );

				// Metto due message box perché la prima non si ferma !
				MessageBox.Show( ee.Message, "ATTENZIONE" );
				MessageBox.Show( "Impossibile avviare l'applicazione " + Configurazione.applicationName + " !\nErrore bloccante!\nVedere il log", "ERRORE non previsto", MessageBoxButton.OK, MessageBoxImage.Error );
				Environment.Exit( 9 );
			}

			_giornale.Info( "Applicazione avviata. Apro il form principale." );

		}

		protected override void OnExit( ExitEventArgs e ) {

			_giornale.Info( "Uscita dall'applicazione" );

			LumenApplication.Instance.ferma();

			base.OnExit( e );
		}




	}
}
