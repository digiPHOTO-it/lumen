﻿using System;
using System.Windows;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Config;
using System.Configuration;
using Digiphoto.Lumen.UI.Main;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.UI.Reports;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.UI.TrayIcon;
using Digiphoto.Lumen.UI.About;

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
			}


			// When the ViewModel asks to be closed, 
			// close the window.
			EventHandler handler = null;
			handler = delegate {
				_mainWindowViewModel.RequestClose -= handler;
				this.Close();
				Application.Current.Shutdown();
			};

			_mainWindowViewModel.RequestClose += handler;

			// il mio ViewModel
			DataContext = _mainWindowViewModel;

			// Mi sottoscrivo per ascoltare i messaggi di richiesta di cambio pagina.
			IObservable<CambioPaginaMsg> observable = LumenApplication.Instance.bus.Observe<CambioPaginaMsg>();
			observable.Subscribe( this );
		}

		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( CambioPaginaMsg cambioPaginaMsg ) {

			
			if( cambioPaginaMsg.nuovaPag == "FotoRitoccoPag" )
				tabControlPagine.SelectedItem = tabControlPagine.FindName( "tabItemAggiusta" );
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

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			AboutBox about = new AboutBox(this);
			about.ShowDialog();

		}
	}
}
