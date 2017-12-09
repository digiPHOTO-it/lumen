using System;
using System.Windows;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.UI.Main;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.UI.TrayIcon;
using Digiphoto.Lumen.UI.About;
using System.Windows.Input;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.UI.Mvvm.Event;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IObserver<CambioPaginaMsg>, IDialogProvider, ITrayIconProvider {

        MainWindowViewModel _mainWindowViewModel = null;

		public MainWindow() {

			using( new UnitOfWorkScope() ) {

				InitializeComponent();

                _mainWindowViewModel = new MainWindowViewModel();
				_mainWindowViewModel.dialogProvider = this;
				_mainWindowViewModel.trayIconProvider = this;

				carrelloView.DataContext = _mainWindowViewModel.carrelloViewModel;
				fotoGallery.DataContext = _mainWindowViewModel.fotoGalleryViewModel;
				fotoRitoccoUserControl.DataContext = _mainWindowViewModel.fotoRitoccoViewModel;

				_mainWindowViewModel.openPopupDialogRequest += _mainWindowViewModel_openPopupDialogRequest;
			}


			// When the ViewModel asks to be closed, 
			// close the window.
			EventHandler handler = null;
			handler = delegate {
				_mainWindowViewModel.RequestClose -= handler;
				this.DataContext = null;
				this.Close();
			};

			_mainWindowViewModel.RequestClose += handler;

			// il mio ViewModel
			DataContext = _mainWindowViewModel;

			// Mi sottoscrivo per ascoltare i messaggi di richiesta di cambio pagina.
			IObservable<CambioPaginaMsg> observable = LumenApplication.Instance.bus.Observe<CambioPaginaMsg>();
			observable.Subscribe( this );
		}

		private void _mainWindowViewModel_openPopupDialogRequest( object sender, EventArgs e ) {

			if( e is OpenPopupRequestEventArgs ) {

				OpenPopupRequestEventArgs popEventArgs = (OpenPopupRequestEventArgs)e;
				if( popEventArgs.requestName == "RicostruzioneDbPopup" ) {

					DbRebuilderWiew win = new DbRebuilderWiew();

					// Imposto la finestra contenitore per poter centrare
					win.Owner = this;

					// Questo è il viewmodel della finestra di popup				
					win.DataContext = popEventArgs.viewModel;

					var esito = win.ShowDialog();

					if( esito == true ) {
						// TODO
					}

					win.Close();
				}
			}
		}

		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( CambioPaginaMsg cambioPaginaMsg ) {

			if( cambioPaginaMsg.nuovaPag == "FotoRitoccoPag" ) {
				tabControlProspettive.SelectedItem = tabControlProspettive.FindName( "tabItemAggiusta" );

				// Provo a dare il fuoco al mio usercontrol ma nel thread della GUI
				App.Current.Dispatcher.BeginInvoke(
					new Action( () => {
						fotoRitoccoUserControl.Focus();
					}
				) );

			} else if( cambioPaginaMsg.nuovaPag == "GalleryPag" )
				tabControlProspettive.SelectedItem = tabControlProspettive.FindName( "tabItemGallery" );

		}


		/// <summary>
		/// Visualizza un messaggio
		/// </summary>
		/// <param name="message"></param>
		/// <param name="title"></param>
		/// <param name="afterHideCallback"></param>
		public void ShowError( string message, string title, Action afterHideCallback ) {

			var risultato = MessageBox.Show( this, message, title, MessageBoxButton.OK, MessageBoxImage.Error );
			if( afterHideCallback != null )
				afterHideCallback();
		}

		public void ShowMessage( string message, string title ) {
			MessageBox.Show( this, message, title, MessageBoxButton.OK, MessageBoxImage.Information );
		}


		/// <summary>
		/// Chiedo conferma SI/NO.
		/// Chiamo la callback passando TRUE se l'utente ha scelto SI.
		/// </summary>
		public void ShowConfirmation( string message, string title, Action<bool> afterHideCallback ) {
			var tastoPremuto = MessageBox.Show( this, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question );
			afterHideCallback( tastoPremuto == MessageBoxResult.Yes );
		}
		
		/// <summary>
		/// Chiedo conferma SI/NO/ANNULLA.
		/// Chiamo la callback passando TRUE se l'utente ha scelto SI.
		/// </summary>
		public void ShowConfirmationAnnulla(string message, string title, Action<MessageBoxResult> afterHideCallback)
		{
			var tastoPremuto = MessageBox.Show( this, message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
			afterHideCallback(tastoPremuto);
		}

		#region TrayIcon

		public void showAbout(string title, string msg, int? sleep)
		{
			ShowTrayIcon trayIcon = new ShowTrayIcon();
			trayIcon.showAbout(title, msg, sleep);

			Messaggio msgStatusBar = new Messaggio(this);
			msgStatusBar.descrizione = msg;
			msgStatusBar.showInStatusBar = true;
			LumenApplication.Instance.bus.Publish(msgStatusBar);
		}

		public void showAboutCloud(string title, string msg, int? sleep)
		{
			ShowTrayIcon trayIcon = new ShowTrayIcon();
			trayIcon.showAboutCloud(title, msg, sleep);

			Messaggio msgStatusBar = new Messaggio(this);
			msgStatusBar.descrizione = msg;
			msgStatusBar.showInStatusBar = true;
			LumenApplication.Instance.bus.Publish(msgStatusBar);
		}

		public void showError(string title, string msg, int? sleep)
		{
			ShowTrayIcon trayIcon = new ShowTrayIcon();
			trayIcon.showError(title, msg, sleep);
		}

		public void showInfo(string title, string msg, int? sleep)
		{
			ShowTrayIcon trayIcon = new ShowTrayIcon();
			trayIcon.showInfo(title, msg, sleep);
		}

		public void showWarning(string title, string msg, int? sleep)
		{
			ShowTrayIcon trayIcon = new ShowTrayIcon();
			trayIcon.showWarning(title, msg, sleep);
		}

		#endregion;

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			AboutBox about = new AboutBox(this);
			about.ShowDialog();

		}

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

		private void mainWindow_ContentRendered( object sender, EventArgs e ) {

			// Apro il form pubblico, ed associo il datacontext prendendolo da quello della foto.gallery
			((App)App.Current).gestoreFinestrePubbliche.azionePosizionamentoIniziale();

		}



	}
}
