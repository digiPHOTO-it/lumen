using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Digiphoto.Lumen.UI;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.UI.TrayIcon;
using Digiphoto.Lumen.GestoreConfigurazione.UI.About;
using System.Windows.Threading;

namespace Digiphoto.Lumen.GestoreConfigurazione.UI {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IDialogProvider, ITrayIconProvider {

        MainWindowViewModel _mainWindowViewModel;

		public MainWindow() {
            InitializeComponent();

            _mainWindowViewModel = new MainWindowViewModel();

			// When the ViewModel asks to be closed, 
			// close the window.
			EventHandler handler = null;
			handler = delegate {
				_mainWindowViewModel.RequestClose -= handler;
				this.DataContext = null;
				this.Close();
				Application.Current.Shutdown();
			};

			_mainWindowViewModel.RequestClose += handler;


            DataContext = _mainWindowViewModel;
			_mainWindowViewModel.dialogProvider = this;
			_mainWindowViewModel.trayIconProvider = this;

			licenseEditor1.DataContext = _mainWindowViewModel.licenseEditorViewModel;

			
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

		/// <summary>
		/// Chiedo conferma SI/NO/ANNULLA.
		/// Chiamo la callback passando TRUE se l'utente ha scelto SI.
		/// </summary>
		public void ShowConfirmationAnnulla(string message, string title, Action<MessageBoxResult> afterHideCallback)
		{
			var tastoPremuto = MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
			afterHideCallback(tastoPremuto);
		}
		
		#region TrayIcon

		public void showAbout(string title, string msg, int? sleep)
		{
			ShowTrayIcon trayIcon = new ShowTrayIcon();
			trayIcon.showAbout(title, msg, sleep);
		}

		public void showAboutCloud(string title, string msg, int? sleep)
		{
			ShowTrayIcon trayIcon = new ShowTrayIcon();
			trayIcon.showAboutCloud(title, msg, sleep);
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

		private void comboBoxMotoreDatabase_SelectionChanged( object sender, SelectionChangedEventArgs e ) {

			abilitaGiustaTabMotore();
	
		}

		void abilitaGiustaTabMotore() {

			// carico immagine del logo
			string nomeLogo = null;

				
			if( _mainWindowViewModel.cfg.motoreDatabase == Config.MotoreDatabase.SqLite ) {
				sqliteTab.IsEnabled = sqliteTab.IsSelected = true;
				mysqlTab.IsEnabled = false;
				sqliteTab.Focus();
				nomeLogo = "SQLite-Logo.png";
			}

			if( _mainWindowViewModel.cfg.motoreDatabase == Config.MotoreDatabase.MySQL ) {
				mysqlTab.IsEnabled = mysqlTab.IsSelected = true;
				sqliteTab.IsEnabled = false;
				mysqlTab.Focus();
				nomeLogo = "MySQL-Logo.png";
			}

			Uri uriLogo = new Uri( "/Resources/" + nomeLogo, UriKind.Relative );

			logoMotoreDbImage.Source = new BitmapImage( uriLogo );

		}

		private void about_Click(object sender, RoutedEventArgs e)
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
						this.Cursor = Cursors.Arrow;  // normale
				}
			}
		}

		private void Window_ContentRendered( object sender, EventArgs e ) {
			abilitaGiustaTabMotore();
		}
	}
}
