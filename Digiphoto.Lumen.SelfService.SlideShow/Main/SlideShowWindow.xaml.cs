using Digiphoto.Lumen.SelfService.SlideShow.Config;
using Digiphoto.Lumen.SelfService.SlideShow.Preferenze;
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

		private void PreferenzeButton_Click( object sender, RoutedEventArgs e ) {

			UserConfig appoConfig = (UserConfig)viewModel.userConfig.Clone();

			PreferenzeWindow pw = new PreferenzeWindow();
			pw.DataContext = new PreferenzeWindowViewModel( appoConfig );
			pw.ShowDialog();

			// TODO tutto da rivedere tramite viewmodel
			if( pw.confermato ) {

				viewModel.SalvaNuovaConfigurazione( ((PreferenzeWindowViewModel)pw.DataContext).userConfig );
			}
			pw.Close();
		}
	}
}
