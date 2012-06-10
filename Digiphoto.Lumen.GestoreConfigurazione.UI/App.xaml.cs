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
			mutex = new Mutex(true, "Digiphoto.Lumen.GestoreConfigurazione"); 
			if (mutex.WaitOne(0, false))
			{
				mutexSingle = new Mutex(true, "Digiphoto.Lumen.Single");
				if (mutexSingle.WaitOne(0, false))
				{
					// Carico la Configurazione
					base.OnStartup(e);
					// Inizializzo l'applicazione
					LumenApplication.Instance.avvia();
					_giornale.Info("Applicazione avviata");
				}
				else
				{
					MessageBox.Show("L'applicazione di Digiphoto.Lumen.UI è in esecuzione\nChiudere l'Applicazione e Riavviare il Configuratore");
					Environment.Exit(0);
				} 
			} else 
			{
				MessageBox.Show("L'applicazione di Configurazione è già in esecuzione");
				Environment.Exit(0);
			} 
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _giornale.Info("Uscita dall'applicazione");

			if (LumenApplication.Instance.avviata)
			{
				LumenApplication.Instance.ferma();
			}

            base.OnExit(e);
        }
	}
}
