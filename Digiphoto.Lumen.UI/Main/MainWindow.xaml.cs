using System;
using System.Windows;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Config;
using System.Configuration;
using Digiphoto.Lumen.UI.Main;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IObserver<CambioPaginaMsg> {

        MainWindowViewModel _mainWindowViewModel = null;

		public MainWindow() {

			using( new UnitOfWorkScope() ) {

				InitializeComponent();
				giorniDeleteFoto.Text = ""+UserConfigLumen.GiorniDeleteFoto;
                _mainWindowViewModel = new MainWindowViewModel();
			}


			// When the ViewModel asks to be closed, 
			// close the window.
			EventHandler handler = null;
			handler = delegate {
				_mainWindowViewModel.RequestClose -= handler;
				this.Close();
				Application.Current.Shutdown();
			};

			_mainWindowViewModel.RequestClose += handler;

			// il mio ViewModel
			DataContext = _mainWindowViewModel;

			// Mi sottoscrivo per ascoltare i messaggi di richiesta di cambio pagina.
			IObservable<CambioPaginaMsg> observable = LumenApplication.Instance.bus.Observe<CambioPaginaMsg>();
			observable.Subscribe( this );
		}

        private void button1_Click(object sender, RoutedEventArgs e)
        {
			UserConfigLumen.GiorniDeleteFoto = short.Parse(this.giorniDeleteFoto.Text);
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            System.Diagnostics.Trace.WriteLine(config.FilePath);
        }

		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( CambioPaginaMsg cambioPaginaMsg ) {

			
			if( cambioPaginaMsg.nuovaPag == "FotoRitoccoPag" )
				tabControlPagine.SelectedItem = tabControlPagine.FindName( "tabItemAggiusta" );
		}
	}
}
