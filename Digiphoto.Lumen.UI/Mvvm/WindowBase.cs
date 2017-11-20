using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.UI.TrayIcon;
using System;
using System.Windows;
using System.Windows.Input;

namespace Digiphoto.Lumen.UI.Mvvm {

	public class WindowBase : System.Windows.Window, IDialogProvider , ITrayIconProvider  {


		protected ViewModelBase viewModelBase {
			get;
			private set;
		}

		public override void EndInit() {
			
			base.EndInit();

			associaDialogProvider();
		}

		protected void associaDialogProvider() {

			// Siccome io sono la base di tutti gli user control...
			// Siccome tutti gli user control devono fornire supporto per l'implementazione dei dialoghi
			// Allora provo a farlo qui una volta per tutti
			// Inietto me stesso come fornitore di dialoghi, nel viewModel che così mi può chiamare
			if( this.DataContext != null && this.DataContext is ViewModelBase ) {
				viewModelBase = (ViewModelBase) this.DataContext;
				viewModelBase.dialogProvider = this;
				viewModelBase.trayIconProvider = this;
			}
		}

		protected Window parentWindow {
			get {
				return this.Owner;
			}
		}

		/// <summary>
		/// Visualizza un messaggio
		/// </summary>
		/// <param name="message"></param>
		/// <param name="title"></param>
		/// <param name="afterHideCallback"></param>
		public void ShowError( string message, string title, Action afterHideCallback ) {
			var risultato = MessageBox.Show( parentWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Error );
			if( afterHideCallback != null )
				afterHideCallback();
		}

		public void ShowMessage( string message, string title ) {
			MessageBox.Show( parentWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information );
		}

		
		/// <summary>
		/// Chiedo conferma SI/NO.
		/// Chiamo la callback passando TRUE se l'utente ha scelto SI.
		/// </summary>
		public void ShowConfirmation( string message, string title, Action<bool> afterHideCallback ) {
			var tastoPremuto = MessageBox.Show( parentWindow, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No );
			afterHideCallback( tastoPremuto == MessageBoxResult.Yes );
		}

		/// <summary>
		/// Chiedo conferma SI/NO/ANNULLA.
		/// Chiamo la callback passando TRUE se l'utente ha scelto SI.
		/// </summary>
		public void ShowConfirmationAnnulla(string message, string title, Action<MessageBoxResult> afterHideCallback)
		{
			var tastoPremuto = MessageBox.Show( parentWindow, message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No );
			afterHideCallback(tastoPremuto);
		}

		#region TrayIcon

		public void showAbout(string title, string msg, int? sleep)
		{
			// Risolto il problema dello STAThread
			App.Current.Dispatcher.BeginInvoke(
				new Action(() =>
				{
					ShowTrayIcon trayIcon = new ShowTrayIcon();
					trayIcon.showAbout(title, msg, sleep);
				}
			));

			Messaggio msgStatusBar = new Messaggio(this);
			msgStatusBar.descrizione = msg;
			msgStatusBar.showInStatusBar = true;
			LumenApplication.Instance.bus.Publish(msgStatusBar);
		}

		public void showAboutCloud(string title, string msg, int? sleep)
		{
			// Risolto il problema dello STAThread
			App.Current.Dispatcher.BeginInvoke(
				new Action(() =>
				{
					ShowTrayIcon trayIcon = new ShowTrayIcon();
					trayIcon.showAboutCloud(title, msg, sleep);
				}
			));

			Messaggio msgStatusBar = new Messaggio(this);
			msgStatusBar.descrizione = msg;
			msgStatusBar.showInStatusBar = true;
			LumenApplication.Instance.bus.Publish(msgStatusBar);
		}

		public void showError(string title, string msg, int? sleep)
		{
			// Risolto il problema dello STAThread
			App.Current.Dispatcher.BeginInvoke(
				new Action(() =>
				{
					ShowTrayIcon trayIcon = new ShowTrayIcon();
					trayIcon.showError(title, msg, sleep);
				}
			));
		}

		public void showInfo(string title, string msg, int? sleep)
		{
			// Risolto il problema dello STAThread
			App.Current.Dispatcher.BeginInvoke(
				new Action(() =>
				{
					ShowTrayIcon trayIcon = new ShowTrayIcon();
					trayIcon.showInfo(title, msg, sleep);
				}
			));
		}

		public void showWarning(string title, string msg, int? sleep)
		{
			// Risolto il problema dello STAThread
			App.Current.Dispatcher.BeginInvoke(
				new Action(() =>
				{
					ShowTrayIcon trayIcon = new ShowTrayIcon();
					trayIcon.showWarning(title, msg, sleep);
				}
			));
		}

		#endregion;

		private bool _attenderePrego;
		public bool attenderePrego {
			get {
				return _attenderePrego;
			}
			set {

				if( _attenderePrego != value ) {
					_attenderePrego = value;

					if( value == true )
						this.Cursor = Cursors.Wait;
					else
						this.Cursor = Cursors.AppStarting;  // normale
				}
			}
		}
	}
}
