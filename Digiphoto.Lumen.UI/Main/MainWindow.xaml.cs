using System;
using System.Windows;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Config;
using System.Configuration;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

        MainWindowViewModel _mainWindowViewModel = null;

		public MainWindow() {

			using( new UnitOfWorkScope() ) {

				InitializeComponent();
				 giorniDeleteFoto.Text = ""+Configurazione.GiorniDeleteFotoProperties;
                 _mainWindowViewModel = new MainWindowViewModel();
			}


			// When the ViewModel asks to be closed, 
			// close the window.
			EventHandler handler = null;
			handler = delegate {
				_mainWindowViewModel.RequestClose -= handler;
				this.Close();
			};
			_mainWindowViewModel.RequestClose += handler;


			DataContext = _mainWindowViewModel;
		}

        private void button1_Click(object sender, RoutedEventArgs e)
        {
			Configurazione.GiorniDeleteFotoProperties = short.Parse(this.giorniDeleteFoto.Text);
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            System.Diagnostics.Trace.WriteLine(config.FilePath);
        }
	}
}
