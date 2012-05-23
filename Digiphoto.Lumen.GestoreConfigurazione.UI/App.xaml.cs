﻿using System;
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

        protected override void OnStartup(StartupEventArgs e)
        {
			mutex = new Mutex( true, Application.ResourceAssembly.FullName ); 
			if (mutex.WaitOne(0, false)) {

            base.OnStartup(e);

            // Inizializzo l'applicazione
            //LumenApplication.Instance.avvia();
            _giornale.Info("Applicazione avviata");

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
