using System;
using System.Windows;

namespace Digiphoto.Lumen.UI.Mvvm {

	public interface IDialogProvider {

        void ShowError(string message, string title, Action afterHideCallback);

        void ShowMessage( string message, string title );

		void ShowConfirmation( string message, string title, Action<bool> afterHideCallback );
		
		void ShowConfirmationAnnulla(string message, string title, Action<MessageBoxResult> afterHideCallback);

		bool attenderePrego {
			get;
			set;
		}
    }
}