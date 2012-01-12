using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Digiphoto.Lumen.Applicazione;
using log4net;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( App ) );


		protected override void OnStartup( StartupEventArgs e ) {
			
			base.OnStartup( e );

			LumenApplication.Instance.avvia();

			_giornale.Info( "Applicazione avviata" );
		}

		protected override void OnExit( ExitEventArgs e ) {

			_giornale.Info( "Uscita dall'applicazione" );

			LumenApplication.Instance.ferma();
			
			base.OnExit( e );
		}
	}
}
