using System;
using System.Windows.Controls;
using System.Windows;


namespace Digiphoto.Lumen.UI.Mvvm {

	public class UserControlBase : UserControl, IDialogProvider {


		protected ViewModelBase viewModelBase {
			get;
			private set;
		}

		public override void EndInit() {
			
			base.EndInit();


			// Siccome io sono la base di tutti gli user control...
			// Siccome tutti gli user control devono fornire supporto per l'implementazione dei dialoghi
			// Allora provo a farlo qui una volta per tutti
			// Inietto me stesso come fornitore di dialoghi, nel viewModel che così mi può chiamare
			if( this.DataContext != null && this.DataContext is ViewModelBase ) {
				viewModelBase = (ViewModelBase) this.DataContext;
				viewModelBase.dialogProvider = this;
			}

		}


		/// <summary>
		/// Visualizza un messaggio
		/// </summary>
		/// <param name="message"></param>
		/// <param name="title"></param>
		/// <param name="afterHideCallback"></param>
		public void ShowError( string message, string title, Action afterHideCallback ) {

			var risultato = MessageBox.Show( message, title, MessageBoxButton.OK, MessageBoxImage.Error );
			if( afterHideCallback != null )
				afterHideCallback();
		}

		public void ShowMessage( string message, string title ) {
			MessageBox.Show( message, title, MessageBoxButton.OK, MessageBoxImage.Information );
		}

		
		/// <summary>
		/// Chiedo conferma SI/NO.
		/// Chiamo la callback passando TRUE se l'utente ha scelto SI.
		/// </summary>
		public void ShowConfirmation( string message, string title, Action<bool> afterHideCallback ) {
			var tastoPremuto = MessageBox.Show( message, title, MessageBoxButton.YesNo, MessageBoxImage.Question );
			afterHideCallback( tastoPremuto == MessageBoxResult.Yes );
		}
	}
}
