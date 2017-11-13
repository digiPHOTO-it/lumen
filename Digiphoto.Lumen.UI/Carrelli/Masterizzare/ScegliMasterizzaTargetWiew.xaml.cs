using Digiphoto.Lumen.UI.Mvvm;
using System;
using System.Windows.Input;

namespace Digiphoto.Lumen.UI.Carrelli.Masterizzare {

	/// <summary>
	/// Interaction logic for ScegliMasterizzaTargetWiew.xaml
	/// </summary>
	public partial class ScegliMasterizzaTarget : WindowBase {
		public ScegliMasterizzaTarget() {

			InitializeComponent();

			DataContextChanged += ScegliMasterizzaTarget_DataContextChanged;
		}

		private void ScegliMasterizzaTarget_DataContextChanged( object sender, System.Windows.DependencyPropertyChangedEventArgs e ) {

			// Associo il viewmodel anche al mio componente 
			this.selettoreDisco.DataContext = viewModel.selettoreDiscoViewModel;

			// When the ViewModel asks to be closed, 
			// close the window.
			EventHandler handler = null;
			handler = delegate {
				this.DialogResult = true;
				viewModel.selettoreDiscoViewModel.selezioneCambiata -= SelettoreDiscoViewModel_selezioneCambiata;
				viewModel.RequestClose -= handler;
				this.Close();
			};

			viewModel.selettoreDiscoViewModel.selezioneCambiata += SelettoreDiscoViewModel_selezioneCambiata;

			viewModel.RequestClose += handler;
		}

		private void SelettoreDiscoViewModel_selezioneCambiata( object sender, EventArgs e ) {
			// devo far rievalutare il Command.CanExecute per abilitare il pulsante
			CommandManager.InvalidateRequerySuggested();
		}

		public ScegliMasterizzaTargetViewModel viewModel {
			get {
				return (ScegliMasterizzaTargetViewModel)this.DataContext;
			}
		}

		private void browseForFolderButton_Click( object sender, System.Windows.RoutedEventArgs e ) {
				
			string appo = Util.UtilWinForm.scegliCartella();

			this.masterizzaSuCartellaButton.Focus();
			if( appo != null )
				viewModel.cartella = appo;
			
			this.Dispatcher.Invoke( () => { CommandManager.InvalidateRequerySuggested(); } );
		}
	}
}
