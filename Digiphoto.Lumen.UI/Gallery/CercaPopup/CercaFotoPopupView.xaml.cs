using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Digiphoto.Lumen.UI.Gallery {
	/// <summary>
	/// Interaction logic for CercaFotoPopup.xaml
	/// </summary>
	public partial class CercaFotoPopup : Window {
		public CercaFotoPopup() {
			InitializeComponent();
		}

		/// <summary>
		/// Controllo che l'utente inserisca soltanto numeri
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void numFotoTextBox_PreviewTextInput( object sender, TextCompositionEventArgs e ) {
			if( !char.IsDigit( e.Text, e.Text.Length - 1 ) )
				e.Handled = true;
		}

		private void CercaFotoPopupWindow_Loaded( object sender, RoutedEventArgs e ) {
			// Do il focus al campo con il numero del fotogramma
			numFotoTextBox.Focus();
			numFotoTextBox.SelectAll();

			// When the ViewModel asks to be closed, 
			// close the window.
			EventHandler closingHandle = null;
			closingHandle = delegate {
				viewModel.RequestClose -= closingHandle;
				viewModel.identificatoreImprontaViewModel.PropertyChanged -= IdentificatoreImprontaViewModel_PropertyChanged;
				viewModel.Dispose();
				this.DataContext = null;
				if( ! _stoChiudendo )
					this.Close();
			};
			viewModel.RequestClose += closingHandle;

			identificatoreImpronta.DataContext = viewModel.identificatoreImprontaViewModel;

			viewModel.identificatoreImprontaViewModel.PropertyChanged += IdentificatoreImprontaViewModel_PropertyChanged;
		}

		private void IdentificatoreImprontaViewModel_PropertyChanged( object sender, PropertyChangedEventArgs e ) {
			if( e.PropertyName == "nomeIdentificato" ) {
				if( viewModel.identificatoreImprontaViewModel.nomeIdentificato != null )
					confermareButton_Click( this, null );
			}
		}

		private bool _stoChiudendo = false;
		protected override void OnClosing( CancelEventArgs e ) {
			_stoChiudendo = true;
			base.OnClosing( e );
		}

		private void confermareButton_Click( object sender, RoutedEventArgs e ) {
			this.DialogResult = true;
			this.Hide();
		}

		public CercaFotoPopupViewModel viewModel {
			get {
				return (CercaFotoPopupViewModel)this.DataContext;
			}
		}

		private void CercaFotoPopupWindow_Closing( object sender, System.ComponentModel.CancelEventArgs e ) {

			if( viewModel != null )
				if( viewModel.CloseCommand.CanExecute( null ) )
					viewModel.CloseCommand.Execute( null );
		}
	}
}
