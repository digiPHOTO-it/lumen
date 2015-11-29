using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Digiphoto.Lumen.Applicazione;
using log4net;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Util;
using System.Threading;

namespace Digiphoto.Lumen.GestoreConfigurazione.UI {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

        private static readonly ILog _giornale = LogManager.GetLogger(typeof(App));

		private static Mutex mutex;

		private static Mutex mutexSingle;

        protected override void OnStartup(StartupEventArgs e)
        {
			// Facci partire il log
			log4net.Config.XmlConfigurator.Configure();
			_giornale.Debug( "GestoreConfiguratore sta per partire" );


			mutex = new Mutex(true, "Digiphoto.Lumen.GestoreConfigurazione"); 
			if (mutex.WaitOne(0, false))
			{
				mutexSingle = new Mutex(true, "Digiphoto.Lumen.Single");
				if (mutexSingle.WaitOne(0, false))
				{
					// Carico la Configurazione
					base.OnStartup(e);

					_giornale.Info( "ok startup effettuato" );
				}
				else
				{
					_giornale.Info( "Lumen UI già in esecuzione. Uscita forzata" );
					MessageBox.Show("L'applicazione di Digiphoto.Lumen.UI è in esecuzione\nChiudere l'Applicazione e Riavviare il Configuratore");
					Environment.Exit(0);
				} 
			} else 
			{
				_giornale.Info( "Gestore Configurazione già in esecuzione. Uscita forzata" );
				MessageBox.Show("L'applicazione di Configurazione è già in esecuzione");
				Environment.Exit(0);
			} 
        }

        protected override void OnExit(ExitEventArgs e)
        {
			_giornale.Info( "Uscita dall'applicazione" );

			if (LumenApplication.Instance.avviata)
			{
				LumenApplication.Instance.ferma();
			}

			rilascioMutex();

            base.OnExit(e);
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

	}
}
