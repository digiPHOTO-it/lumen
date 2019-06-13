using Digiphoto.Lumen.Applicazione;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Digiphoto.Lumen.WcfServices.Host {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

		protected override void OnStartup( StartupEventArgs e ) {

			base.OnStartup( e );

			LumenApplication.Instance.avvia();
		}

		protected override void OnExit( ExitEventArgs e ) {

			LumenApplication.Instance.ferma();

			base.OnExit( e );
		}

	}



}
