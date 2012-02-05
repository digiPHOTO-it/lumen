using System;

namespace Digiphoto.Lumen.UI.Mvvm {

	public interface IDialogProvider {

        void ShowError(string message, string title, Action afterHideCallback);

        void ShowMessage( string message, string title );

		void ShowConfirmation( string message, string title, Action<bool> afterHideCallback );
    }
}