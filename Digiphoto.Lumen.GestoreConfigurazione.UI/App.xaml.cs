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

namespace Digiphoto.Lumen.GestoreConfigurazione.UI {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

        private static readonly ILog _giornale = LogManager.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Testo se è stato avviato Lumen
            if (UserConfigXML.PathUserConfigLumen.Equals(""))
            {
                MessageBox.Show("Devi eseguire Lumen prima", "Avviso");
                Environment.Exit(0);
            }

            // Inizializzo l'applicazione
            //LumenApplication.Instance.avvia();
            _giornale.Info("Applicazione avviata");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _giornale.Info("Uscita dall'applicazione");

            // LumenApplication.Instance.ferma();

            base.OnExit(e);
        }
	}
}
