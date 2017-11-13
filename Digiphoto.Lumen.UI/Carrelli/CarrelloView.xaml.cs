using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Digiphoto.Lumen.UI.Mvvm;
using System.Collections;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Carrelli.Masterizzare;
using Digiphoto.Lumen.UI.Mvvm.Event;

namespace Digiphoto.Lumen.UI.Carrelli {
	/// <summary>
	/// Interaction logic for Carrello.xaml
	/// </summary>
	public partial class CarrelloView : UserControlBase
    {
        public CarrelloView() 
        {
            InitializeComponent();

			DataContextChanged += new DependencyPropertyChangedEventHandler(carrelloView_DataContextChanged);

			annullaDragRighe();
        }

		void carrelloView_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {
			associaDialogProvider();

			this.incassiFotografiView.DataContext = carrelloViewModel.incassiFotografiViewModel;

			// Mi posiziono per default sulla data di oggi.
			carrelloViewModel.paramCercaCarrello.giornataIniz = carrelloViewModel.oggi;
			carrelloViewModel.paramCercaCarrello.giornataFine = carrelloViewModel.oggi;

			carrelloViewModel.openPopupDialogRequest += viewModel_openPopupDialogRequest;
		}

		private CarrelloViewModel carrelloViewModel
		{
			get
			{
				return (CarrelloViewModel)base.viewModelBase;
			}
		}

		private void oggiButton_Click(object sender, RoutedEventArgs e)
		{
			datePickerRicercaIniz.SelectedDate = carrelloViewModel.oggi;
			datePickerRicercaFine.SelectedDate = carrelloViewModel.oggi;
		}

		private void ieriButton_Click(object sender, RoutedEventArgs e)
		{
			TimeSpan unGiorno = new TimeSpan( 1, 0, 0, 0 );
			DateTime ieri = carrelloViewModel.oggi.Subtract( unGiorno );
			datePickerRicercaIniz.SelectedDate = ieri;
			datePickerRicercaFine.SelectedDate = ieri;
		}

		private void ieriOggiButton_Click(object sender, RoutedEventArgs e)
		{
			TimeSpan unGiorno = new TimeSpan( 1, 0, 0, 0 );
			DateTime ieri = carrelloViewModel.oggi.Subtract( unGiorno );
			datePickerRicercaIniz.SelectedDate = ieri;
			datePickerRicercaFine.SelectedDate = carrelloViewModel.oggi;
		}

		private void calendario_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
		{
			IList giorni = e.AddedItems;

			if (giorni.Count > 0)
			{

				// A seconda di come si esegue la selezione, il range può essere ascendente o discendente.
				// A me serve sempre prima la più piccola poi la più grande
				DateTime aa = (DateTime)giorni[0];
				DateTime bb = (DateTime)giorni[giorni.Count - 1];

				// Metto sempre per prima la data più piccola
				carrelloViewModel.paramCercaCarrello.giornataIniz = minDate(aa, bb);
				carrelloViewModel.paramCercaCarrello.giornataFine = maxDate(aa, bb);
			}
		}

		public static DateTime minDate(DateTime aa, DateTime bb)
		{
			return aa > bb ? bb : aa;
		}
		public static DateTime maxDate(DateTime aa, DateTime bb)
		{
			return aa > bb ? aa : bb;
		}

		private void listRighe_Drop( object sender, DragEventArgs e ) {

			if( sender == this.listRigheStampate ) {
				// Ho fatto il drop sulle righe stampate
			} else if( sender == this.listRigheMasterizzate ) {
				// ho fatto il drop sulle righe masterizzate
			}

			var miaVar = e.Data.GetData( "pollo" );
			RigaCarrello rigaDroppata = (RigaCarrello)miaVar;

			if( carrelloViewModel.SpostaFotoRigaDxSxCommand.CanExecute( rigaDroppata ) ) {
				carrelloViewModel.SpostaFotoRigaDxSxCommand.Execute( rigaDroppata );
            }


			listRigheMasterizzate.AllowDrop = false;
			listRigheMasterizzate.AllowDrop = false;
		}

		#region Sposta Copia Righe Drag and Drop

		// Memorizzo il punto di inizio drag
		private Point dragStartPoint;

		// Con questo metodo, mi segno il punto di inizio del drag
		private void listRighe_PreviewMouseLeftButtonDown( object sender, MouseButtonEventArgs e ) {

			ListBox container = sender as ListBox;

			if( sender != null ) {

				var item = ItemsControl.ContainerFromElement( sender as ItemsControl, (DependencyObject)e.OriginalSource ) as ListBoxItem;

				// Se non faccio questo test, mi viene intercettato anche il Click sulla ScrollBar.
				if( item != null ) {
					// Store the mouse position
					dragStartPoint = e.GetPosition( null );
				}
			}
		}

		private void listRighe_MouseMove( object sender, MouseEventArgs e ) {

			// Questo evento viene generato di continuo. A me interessa soltanto quando è stato fissato il punto inizio drag
			if( dragStartPoint.X == 0 && dragStartPoint.Y == 0 )
				return;

			// Get the current mouse position
			Point mousePos = e.GetPosition( null );
			Vector diff = dragStartPoint - mousePos;

			bool annulla = false;

			if( e.LeftButton == MouseButtonState.Pressed ) {

				if( Math.Abs( diff.X ) > SystemParameters.MinimumHorizontalDragDistance ||
					Math.Abs( diff.Y ) > SystemParameters.MinimumVerticalDragDistance ) {

					// Ricavo la fotografia selezionata
					ListBox listBoxSorgente = sender as ListBox;
					RigaCarrello sel = (RigaCarrello)listBoxSorgente.SelectedItem;

					if( sel != null ) {
						if( sender == this.listRigheStampate ) {
							this.listRigheMasterizzate.AllowDrop = true;
							this.listRigheStampate.AllowDrop = false;
						} else if( sender == this.listRigheMasterizzate ) {
							this.listRigheMasterizzate.AllowDrop = false;
							this.listRigheStampate.AllowDrop = true;
						}

						// Initialize the drag & drop operation
						// Creo una chiave con un valore che mi in
						DataObject dragData = new DataObject( "pollo", sel );

						DragDrop.DoDragDrop( listBoxSorgente, dragData, DragDropEffects.Move );
					} else
						annulla = true;
				} 
			} else
				annulla = true;


			if( annulla ) {
				annullaDragRighe();
			}

		}

		void annullaDragRighe() {
			dragStartPoint.X = 0;
			dragStartPoint.Y = 0;
			listRigheStampate.AllowDrop = false;
			listRigheMasterizzate.AllowDrop = false;
		}

		private void listRighe_QueryContinueDrag( object sender, QueryContinueDragEventArgs e ) {

			// Se premo ESC mentre sto facendo il drag, allora annullo tutto.
			if( Keyboard.IsKeyDown( Key.Escape ) ||
				( dragStartPoint.X == 0 && dragStartPoint.Y == 0 ) ) {
				e.Action = DragAction.Cancel;
				annullaDragRighe();
			}
		}

		#endregion Sposta Copia Righe Drag and Drop

		private void viewModel_openPopupDialogRequest( object sender, EventArgs e ) {

			OpenPopupRequestEventArgs eaPop = (OpenPopupRequestEventArgs)e;

			if( eaPop.requestName == "ScegliMasterizzaTargetPopup" ) {

				ScegliMasterizzaTarget win = new ScegliMasterizzaTarget();

				// Imposto la finestra contenitore per poter centrare
				win.Owner = this.parentWindow;

				// Questo è il viewmodel della finestra di popup				
				win.DataContext = eaPop.viewModel;

				var esito = win.ShowDialog();

				eaPop.mioDialogResult = esito;

				win.Close();
			}
		}

	}
}
