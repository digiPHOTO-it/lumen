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
using System.Windows.Shapes;
using System.ComponentModel;
using Digiphoto.Lumen.UI.TrayIcon;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.UI.Dialogs.SelezionaStampante
{
	/// <summary>
	/// Interaction logic for StampaProviniDialog.xaml
	/// </summary>
	public partial class SelezionaStampanteDialog : Window, IDialogProvider, ITrayIconProvider
	{

		SelezionaStampanteDialogViewModel model = null;

		public SelezionaStampanteDialog()
		{
			InitializeComponent();

			model = new SelezionaStampanteDialogViewModel(this);

			DataContext = model;
			model.dialogProvider = this;
			model.trayIconProvider = this;

		}

		public FormatoCarta formatoCarta
		{
			get
			{
				return model.formatoCarta;
			}
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

		#region Dialog

		/// <summary>
		/// Visualizza un messaggio
		/// </summary>
		/// <param name="message"></param>
		/// <param name="title"></param>
		/// <param name="afterHideCallback"></param>
		public void ShowError(string message, string title, Action afterHideCallback)
		{

			var risultato = MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
			if (afterHideCallback != null)
				afterHideCallback();
		}

		public void ShowMessage(string message, string title)
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
		}


		/// <summary>
		/// Chiedo conferma SI/NO.
		/// Chiamo la callback passando TRUE se l'utente ha scelto SI.
		/// </summary>
		public void ShowConfirmation(string message, string title, Action<bool> afterHideCallback)
					{
			var tastoPremuto = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
			afterHideCallback(tastoPremuto == MessageBoxResult.Yes);
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

		#endregion

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

	}
}
