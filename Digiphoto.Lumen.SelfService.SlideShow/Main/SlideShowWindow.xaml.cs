using System.Windows;


namespace Digiphoto.Lumen.SelfService.SlideShow.Main {

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class SlideShowWindow : Window {

		public SlideShowWindow() {

			InitializeComponent();

			this.DataContext = new SlideShowWindowViewModel();
		}

		SlideShowWindowViewModel viewModel {
			get {
				return (SlideShowWindowViewModel)this.DataContext;
			}
		}

		private void Window_Loaded( object sender, RoutedEventArgs e ) {
			viewModel.Start();
		}
	}
}
