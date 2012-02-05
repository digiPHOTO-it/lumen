using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.IO;
using System.Windows;
using Digiphoto.Lumen.UI.Mvvm;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for SelettoreCartellaView.xaml
	/// </summary>
	public partial class SelettoreCartella : UserControlBase {


		public SelettoreCartella() {
			InitializeComponent();
		}

		#region Proprietà

		private SelettoreCartellaViewModel selettoreCartellaViewModel {
			get {
				return (SelettoreCartellaViewModel)base.viewModelBase;
			}
		}

		public string cartellaSelezionata {
			get {
				return selettoreCartellaViewModel.cartellaSelezionata;
			}
		}

		#endregion

		#region Eventi di cartelleRecentiListBox
		private void cartelleRecentiListBox_SelectionChanged( object sender, SelectionChangedEventArgs e ) {
			string nomeCartellaRecente = ((sender as System.Windows.Controls.ListBox).SelectedItem as string);
			selettoreCartellaViewModel.cartellaSelezionata = nomeCartellaRecente;
		}

		private void cartelleRecentiListBox_MouseDoubleClick( object sender, System.Windows.Input.MouseButtonEventArgs e ) {
			string nomeCartellaRecente = ((sender as System.Windows.Controls.ListBox).SelectedItem as string);
			if( nomeCartellaRecente != null )
				selettoreCartellaViewModel.cartellaSelezionata = nomeCartellaRecente;
		}
		#endregion

		#region Eventi di dischiRimovibiliListBox
		private void dischiRimovibiliListBox_MouseDoubleClick( object sender, System.Windows.RoutedEventArgs e ) {
		DriveInfo di = ((sender as System.Windows.Controls.ListBox).SelectedItem as DriveInfo);
			if( di != null )
				selettoreCartellaViewModel.cartellaSelezionata = di.RootDirectory.Name;
		}

		private void dischiRimovibiliListBox_SelectionChanged( object sender, SelectionChangedEventArgs e ) {
			DriveInfo di = ((sender as System.Windows.Controls.ListBox).SelectedItem as DriveInfo);
			selettoreCartellaViewModel.cartellaSelezionata = (di != null ? di.Name : null);
		}

		#endregion

	}
}
