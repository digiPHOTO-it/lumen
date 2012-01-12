using System;
using System.Windows;
using Digiphoto.Lumen.Core.Database;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		MainWindowViewModel _mainWindowViewModel = new MainWindowViewModel();

		public MainWindow() {

			using( new UnitOfWorkScope() ) {

				InitializeComponent();

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
	}
}
