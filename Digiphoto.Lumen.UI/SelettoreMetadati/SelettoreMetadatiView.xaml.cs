using System.Windows;
using Digiphoto.Lumen.Core.Collections;
using Digiphoto.Lumen.UI.Mvvm;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for SelettoreMetadatiView.xaml
	/// </summary>
	public partial class SelettoreMetadati : UserControlBase
	{

		public SelettoreMetadati()
        {
            InitializeComponent();

			this.DataContextChanged += SelettoreMetadati_DataContextChanged;
        }

		private void SelettoreMetadati_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {
			associaDialogProvider();
		}

		/// <summary 
		/// Quando spengo la checkbox, spengo la voce selezionata
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void checkBoxEventi_Unchecked( object sender, RoutedEventArgs e ) {
			
			((SelettoreMetadatiViewModel)this.DataContext).selettoreEventoViewModel.eventoSelezionato = null;
			
		}

		private void checkBoxFasidelGiorno_Unchecked( object sender, RoutedEventArgs e ) {
			fasiDelGiorno.SelectedItem = null;
		}

		private void checkDidascalia_Unchecked( object sender, RoutedEventArgs e ) {
			didascalia.Text = null;
		}
	}
}
