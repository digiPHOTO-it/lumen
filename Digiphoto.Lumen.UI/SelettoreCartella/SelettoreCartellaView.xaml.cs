using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.IO;
using System.Windows;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for SelettoreCartellaView.xaml
	/// </summary>
	public partial class SelettoreCartella : System.Windows.Controls.UserControl {

		private SelettoreCartellaViewModel _selettoreCartellaViewModel;


		public SelettoreCartella() {

			InitializeComponent();

			_selettoreCartellaViewModel = (SelettoreCartellaViewModel)this.DataContext;
		}

		public string cartellaSelezionata {
			get {
				return _selettoreCartellaViewModel.cartellaSelezionata;
			}
		}

		#region Eventi di cartelleRecentiListBox
		private void cartelleRecentiListBox_SelectionChanged( object sender, SelectionChangedEventArgs e ) {
			string nomeCartellaRecente = ((sender as System.Windows.Controls.ListBox).SelectedItem as string);
			_selettoreCartellaViewModel.cartellaSelezionata = nomeCartellaRecente;
		}

		private void cartelleRecentiListBox_MouseDoubleClick( object sender, System.Windows.Input.MouseButtonEventArgs e ) {
			string nomeCartellaRecente = ((sender as System.Windows.Controls.ListBox).SelectedItem as string);
			if( nomeCartellaRecente != null )
				_selettoreCartellaViewModel.cartellaSelezionata = nomeCartellaRecente;
		}
		#endregion

		#region Eventi di dischiRimovibiliListBox
		private void dischiRimovibiliListBox_MouseDoubleClick( object sender, System.Windows.RoutedEventArgs e ) {
		DriveInfo di = ((sender as System.Windows.Controls.ListBox).SelectedItem as DriveInfo);
			if( di != null )
				_selettoreCartellaViewModel.cartellaSelezionata = di.RootDirectory.Name;
		}

		private void dischiRimovibiliListBox_SelectionChanged( object sender, SelectionChangedEventArgs e ) {
			DriveInfo di = ((sender as System.Windows.Controls.ListBox).SelectedItem as DriveInfo);
			_selettoreCartellaViewModel.cartellaSelezionata = (di != null ? di.Name : null);
		}

		#endregion

	}
}
