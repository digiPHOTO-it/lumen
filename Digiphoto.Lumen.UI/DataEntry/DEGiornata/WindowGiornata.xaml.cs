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
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Model;
using System.Windows.Markup;
using System.Globalization;

namespace Digiphoto.Lumen.UI.DataEntry.DEGiornata {
	/// <summary>
	/// Interaction logic for WindowCassa.xaml
	/// </summary>
	public partial class WindowGiornata :  Window, IDialogProvider {

		private DataEntryGiornataViewModel _viewModel;

		public WindowGiornata() {

			InitializeComponent();



			_viewModel = new DataEntryGiornataViewModel();
			_viewModel.dialogProvider = this;
			this.DataContext = _viewModel;


//			_viewModel.collectionView.CurrentChanged += new EventHandler( SelectedItemChanged );
		}


		public void ShowError( string message, string title, Action afterHideCallback ) {
			var risultato = MessageBox.Show( message, title, MessageBoxButton.OK, MessageBoxImage.Error );
			if( afterHideCallback != null )
				afterHideCallback();
		}

		public void ShowMessage( string message, string title ) {
			MessageBox.Show( message, title, MessageBoxButton.OK, MessageBoxImage.Information );
		}

		public void ShowConfirmation( string message, string title, Action<bool> afterHideCallback ) {
			var tastoPremuto = MessageBox.Show( message, title, MessageBoxButton.YesNo, MessageBoxImage.Question );
			afterHideCallback( tastoPremuto == MessageBoxResult.Yes );
		}

		public void ShowConfirmationAnnulla( string message, string title, Action<MessageBoxResult> afterHideCallback ) {
			var tastoPremuto = MessageBox.Show( message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question );
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

		private void textBoxIncassoDichiarato_TextChanged( object sender, TextChangedEventArgs e ) {


			Giornata giornata = (Giornata)_viewModel.collectionView.CurrentItem;
			if( giornata == null ) {
				textBoxSquadratura.Text = null;
				return;
			}

			Decimal incassoDichiarato;

			string appo = textBoxIncassoDichiarato.Text; // .Replace( '.', ',').Replace( "$", "" );
			CultureInfo culture = CultureInfo.CurrentUICulture;
			NumberStyles style;
			style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;

			if( Decimal.TryParse( appo, style, culture, out incassoDichiarato ) ) {

				Decimal _squadratura = incassoDichiarato - giornata.incassoPrevisto;
				textBoxSquadratura.Text = _squadratura.ToString( "C" );
				textBoxSquadratura.Foreground = (_squadratura < 0) ? Brushes.Red : Brushes.Black;
			} else {
				textBoxSquadratura.Text = null;
			}

#if  QQQQ		
		
			Giornata giornata = null;
			if( _viewModel.collectionView.IsEditingItem )
				giornata = (Giornata)_viewModel.collectionView.CurrentEditItem;
			else if( _viewModel.collectionView.IsAddingNew )
				giornata = (Giornata)_viewModel.collectionView.CurrentAddItem;
			else
				giornata = (Giornata)_viewModel.collectionView.CurrentItem;

			if( giornata == null || giornata.incassoDichiarato == null )
				this.textBoxSquadratura.Text = null;
			else {
				Decimal squadratura = giornata.incassoPrevisto - giornata.incassoDichiarato;
				this.textBoxSquadratura.Text = squadratura.ToString();
			}
			
		
#endif
		}

	}
}
