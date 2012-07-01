﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.UI.Pubblico;
using log4net;
using Digiphoto.Lumen.Config;
using System.Threading;
using Digiphoto.Lumen.Util;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Digiphoto.Lumen.UI {



	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {


		private static readonly ILog _giornale = LogManager.GetLogger( typeof( App ) );

		private static Mutex mutex;

		private static Mutex mutexSingle;

		private SlideShowWindow _slideShowWindow;
		private MainWindow _mainWindow;


		protected override void OnStartup( StartupEventArgs e ) {

			// Faccio partire il log
			log4net.Config.XmlConfigurator.Configure();

			mutex = new Mutex(true, "Digiphoto.Lumen.UI");
			if (mutex.WaitOne(0, false)) 
			{

				mutexSingle = new Mutex(true, "Digiphoto.Lumen.Single");
				if (mutexSingle.WaitOne(0, false))
				{
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

					} catch( Exception ee ) {
						
						_giornale.Error( "Impossibile avviare applicazione", ee );

						// Metto due message box perché la prima non si ferma !
						MessageBox.Show( "ATTENZIONE" );
						MessageBox.Show( "Impossibile avviare l'applicazione!\nOccorre creare la configurazione iniziale.\nLanciare prima il gestore della configurazione!", "ERRORE avvio", MessageBoxButton.OK, MessageBoxImage.Error );
						Environment.Exit( 2 );
					}

					#if (! DEBUG)
						// Chiudo lo splash
						splashScreen.Close( new TimeSpan() );
					#endif

					_giornale.Info("Applicazione avviata. Apro il form principale.");

					_mainWindow = new MainWindow();
					_mainWindow.Show();

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
				MessageBox.Show( "L'applicazione è già in esecuzione" );
				Environment.Exit(3);
			}
			
		}

		protected override void OnExit( ExitEventArgs e ) {

			_giornale.Info( "Uscita dall'applicazione" );

			if( _slideShowWindow != null ) {
				_slideShowWindow.Close();
				_slideShowWindow = null;
			}

			if( _mainWindow != null ) {
				_mainWindow.Close();
				_mainWindow = null;
			}
			
			LumenApplication.Instance.ferma();
			
			base.OnExit( e );
		}

		/// <summary>
		/// Se la finestra del pubblico non è aperta (o non è istanziata)
		/// la creo sul momento
		/// </summary>
		public void forseApriWindowPubblica() {

			// Se è già aperta, non faccio niente
			if( _slideShowWindow != null )
				return;  

			// Apro la finestra modeless
			// Create a window and make this window its owner
			_slideShowWindow = new SlideShowWindow();
			_slideShowWindow.Closed += chiusoSlideShowWindow;
			_slideShowWindow.Show();
		}

		public SlideShowViewModel slideShowViewModel {
			get {
				return _slideShowWindow == null ? null : (SlideShowViewModel)_slideShowWindow.DataContext;
			}
		}

		public void chiusoSlideShowWindow( object sender, EventArgs e ) {
			_slideShowWindow.Closed -= chiusoSlideShowWindow;
			_slideShowWindow = null;
		}

	}
}
