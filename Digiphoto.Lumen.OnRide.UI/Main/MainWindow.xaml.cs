using Digiphoto.Lumen.OnRide.UI.Model;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.UI.Util;
using Digiphoto.Lumen.Util;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Digiphoto.Lumen.OnRide.UI {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IDialogProvider {

		public MainWindow() {

			InitializeComponent();

			this.DataContext = new MainWindowViewModel( this );



			if( viewModel.userConfigOnRide.runningMode == Config.RunningMode.Automatico )
				((INotifyCollectionChanged)onrideListView.Items).CollectionChanged += ListView_CollectionChanged;
		}

		private void ListView_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e ) {

			if( viewModel.userConfigOnRide.runningMode == Config.RunningMode.Automatico ) {

				foreach( FotoItem fotoItem in viewModel.fotoItemsCW )
					fotoItem.daTaggare = false;

				viewModel.acquisireFotoCommand.Execute( null );
			}

		}

		MainWindowViewModel viewModel {
			get {
				return (MainWindowViewModel)this.DataContext;
			}
		}


		private void boxHeaderEliminare_Click( object sender, RoutedEventArgs e ) {

			// In base al mio valore, valorizzo tutte le righe eventuali
			for( int ii = 0; ii < viewModel.fotoItemsCW.Count; ii++ ) {
				FotoItem item = (FotoItem) viewModel.fotoItemsCW.GetItemAt( ii );
				item.daEliminare = (boxHeaderEliminare.IsChecked == true);
			}
		}

		private void boxHeaderTaggare_Click( object sender, RoutedEventArgs e ) {

			// In base al mio valore, valorizzo tutte le righe eventuali
			for( int ii = 0; ii < viewModel.fotoItemsCW.Count; ii++ ) {
				FotoItem item = (FotoItem)viewModel.fotoItemsCW.GetItemAt( ii );
				item.daTaggare = (boxHeaderTaggare.IsChecked == true);
			}
		}

		#region IDIalogProvider

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
		public void ShowConfirmationAnnulla( string message, string title, Action<MessageBoxResult> afterHideCallback ) {
			var tastoPremuto = MessageBox.Show( this, message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question );
			afterHideCallback( tastoPremuto );
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

		#endregion IDIalogProvider

		private void TagTextBox_GotFocus( object sender, RoutedEventArgs e ) {
			
			// Quando clicco nella textbox, non si aggiorna la foto (perché l'evento viene consumato)
			// devo selezionare la riga io a mano

			ListViewItem lvi = AiutanteUI.GetAncestorByType( e.OriginalSource as DependencyObject, typeof( ListViewItem ) ) as ListViewItem;

			if( lvi != null ) {

				bool front = true;

				if( front ) {

					// provoco lo spostamento sulla ListView del frontend
					var idx = onrideListView.ItemContainerGenerator.IndexFromContainer( lvi );
					onrideListView.SelectedIndex = idx;
					lvi.IsSelected = true;
				} else {
					// cosi funziona ma agisto sul viemwodel
					FotoItem item = (FotoItem)lvi.Content;
					viewModel.fotoItemsCW.MoveCurrentTo( item );
				}

			}

		}

		private void cambiareMascheraButton_Click( object sender, RoutedEventArgs e ) {

			// Create OpenFileDialog
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			// Set filter for file extension and default file extension
			dlg.DefaultExt = ".png";
			dlg.Filter = "Portable Network Graphics (.png)|*.png";
			dlg.InitialDirectory = PathUtil.getCartellaMaschera( Lumen.Model.FiltroMask.MskSingole );

			// Display OpenFileDialog by calling ShowDialog method
			Nullable<bool> result = dlg.ShowDialog();

			// Get the selected file name and display in a TextBox
			if( result == true ) {
				FileInfo fi = new FileInfo( dlg.FileName );
				viewModel.CaricareMaschera( fi.Name );		// solo nome, senza path
			}

		}
	}
}
