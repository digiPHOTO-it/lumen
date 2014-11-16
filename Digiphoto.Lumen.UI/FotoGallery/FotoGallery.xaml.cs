using System;
using System.Collections;
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
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.UI.ScreenCapture;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Util;
using System.Windows.Threading;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for FotoGallery.xaml
	/// </summary>
	public partial class FotoGallery : UserControlBase {
		
		public FotoGallery() {
			InitializeComponent();

			DataContextChanged += new DependencyPropertyChangedEventHandler(fotoGallery_DataContextChanged);

        }

		void fotoGallery_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {
			associaDialogProvider();

			fotoGalleryViewModel.snpashotCambiataEventHandler += new FotoGalleryViewModel.SnpashotCambiataEventHandler( onSnapshotCambiata );
		}

		/// <summary>
		///  Questo evento lo ascolto direttamente dal viewmodel perché devo eseguire questa
		///  operazione, dopo aver terminato l'esecuzione dei Command.
		/// </summary>
		/// <param name="sender">il viewmodel</param>
		/// <param name="args">vuoto</param>
		void onSnapshotCambiata( object sender, EventArgs args ) {
			forsePrendoSnapshotPubblico();
		}


		#region Proprietà
		private FotoGalleryViewModel fotoGalleryViewModel {
			get {
				return (FotoGalleryViewModel)base.viewModelBase;
			}
		}
		#endregion


		private void buttonQuanteNeVedo_Click( object sender, RoutedEventArgs e ) {

			String param = (String)((Control)sender).Tag;

			short quante = quanteRigheVedo;

			if( param == "+" )
				++quante;
			else if( param == "-" ) {
				if( quanteRigheVedo > 1 )
					--quante;
			}  else
				quante = Convert.ToInt16( param );

			// prima questo
			sistemaMarginePerVedereDueFotoAffiancate( quante );

			// poi questo
			quanteRigheVedo = quante;

			forsePrendoSnapshotPubblico();

			LsImageGallery.Focus();   // mi consente di usare il tasto pg/up pg/down.
		}

		/// <summary>
		/// Se scelgo di vedere una sola "riga" di foto, allora cerco di farci stare 
		/// due foto affiancate
		/// </summary>
		private void sistemaMarginePerVedereDueFotoAffiancate( short quante ) {

			int quanto = 0;
			Thickness margin = LsImageGallery.Margin;
			if( quante == 1 && expanderFiltriRicerca.IsExpanded == false )
				quanto = Configurazione.UserConfigLumen.correzioneAltezzaGalleryDueFoto;
			margin.Bottom = quanto;
			LsImageGallery.Margin = margin;

			LsImageGallery.UpdateLayout();
		}


		private void oggiButton_Click( object sender, RoutedEventArgs e ) {
			datePickerRicercaIniz.SelectedDate = fotoGalleryViewModel.oggi;
			datePickerRicercaFine.SelectedDate = fotoGalleryViewModel.oggi;
		}

		private void ieriButton_Click( object sender, RoutedEventArgs e ) {
			TimeSpan unGiorno = new TimeSpan(1,0,0,0);
			DateTime ieri = fotoGalleryViewModel.oggi.Subtract( unGiorno );
			datePickerRicercaIniz.SelectedDate = ieri;
			datePickerRicercaFine.SelectedDate = ieri;
		}

		private void ieriOggiButton_Click( object sender, RoutedEventArgs e ) {

			TimeSpan unGiorno = new TimeSpan( 1, 0, 0, 0 );
			DateTime ieri = fotoGalleryViewModel.oggi.Subtract( unGiorno );
			datePickerRicercaIniz.SelectedDate = ieri;
			datePickerRicercaFine.SelectedDate = fotoGalleryViewModel.oggi;
		}

		private void calendario_SelectedDatesChanged( object sender, SelectionChangedEventArgs e ) {
			IList giorni = e.AddedItems;
			
			if( giorni.Count > 0 ) {

				// A seconda di come si esegue la selezione, il range può essere ascendente o discendente.
				// A me serve sempre prima la più piccola poi la più grande
				DateTime aa = (DateTime) giorni[0];
				DateTime bb = (DateTime) giorni[giorni.Count-1];

				// Metto sempre per prima la data più piccola
				fotoGalleryViewModel.paramCercaFoto.giornataIniz = minDate( aa, bb );
				fotoGalleryViewModel.paramCercaFoto.giornataFine = maxDate( aa, bb );
			}
		}

		public static DateTime minDate( DateTime aa, DateTime bb ) {
			return aa > bb ? bb : aa;
		}
		public static DateTime maxDate( DateTime aa, DateTime bb ) {
			return aa > bb ? aa : bb;
		}

		/// <summary>
		/// Tramite il doppio click sulla foto, mando direttamente in modifica quella immagine.
		/// </summary>
		private void listBoxItemImageGallery_MouseDoubleClick( object sender, RoutedEventArgs e ) {

			ListBoxItem lbItem = (ListBoxItem) sender;
			fotoGalleryViewModel.mandareInModificaImmediata( lbItem.Content as Fotografia );
		}

		private Fotografia ultimaSelezionata = null;
		private void LsImageGallery_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			// Anche se clicco sulla scrollbar mi solleva l'evento button down.
			if( ! (e.OriginalSource is Image) )
				return;

			//
			ListBoxItem lbi = SelectItemOnLeftClick( e );
			if( lbi == null )
				return;

			Fotografia foto = (Fotografia)lbi.Content;
			if(ultimaSelezionata==null)
				ultimaSelezionata = (Fotografia)lbi.Content;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
			{
				if(ultimaSelezionata!=null)
				{
					int firstIndex = fotoGalleryViewModel.fotografieCW.IndexOf(ultimaSelezionata);
					int lastIndex = fotoGalleryViewModel.fotografieCW.IndexOf(foto);
					//se ho selezionato dal più alto al più basso inverto gli indici
					if (firstIndex > lastIndex)
					{
						int appoggio = firstIndex;
						//faccio +1 perche se no non riesco a selezionare l'ultima foto
						firstIndex = lastIndex+1;
						lastIndex = appoggio;
					}

					for (int i = firstIndex; i < lastIndex; i++)
					{
						Fotografia f = (Fotografia)fotoGalleryViewModel.fotografieCW.GetItemAt(i);
						if (!fotoGalleryViewModel.fotografieCW.SelectedItems.Contains(f))
						{
							fotoGalleryViewModel.fotografieCW.SelectedItems.Add(f);
							fotoGalleryViewModel.fotografieCW.RefreshSelectedItemWithMemory();
							ultimaSelezionata = null;
						}
					}
				}
			}
		}

		private ListBoxItem SelectItemOnLeftClick(System.Windows.Input.MouseButtonEventArgs e)
		{
			Point clickPoint = e.GetPosition(LsImageGallery);
			object element = LsImageGallery.InputHitTest(clickPoint);
			ListBoxItem clickedListBoxItem = null;
			if (element != null)
			{
				clickedListBoxItem = GetVisualParent<ListBoxItem>(element);
				if (clickedListBoxItem != null)
				{
					Fotografia f = (Fotografia)clickedListBoxItem.Content;
				}
			}
			return clickedListBoxItem;
		}

		private void LsImageGallery_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			if( !(e.OriginalSource is Image) )
				return;

			Fotografia foto = (Fotografia)SelectItemOnRightClick(e).Content;
			((FotoGalleryViewModel)viewModelBase).selettoreAzioniRapideViewModel.ultimaFotoSelezionata = foto;
			e.Handled = true;
		}

		private ListBoxItem SelectItemOnRightClick(System.Windows.Input.MouseButtonEventArgs e)
		{
			Point clickPoint = e.GetPosition(LsImageGallery);
			object element = LsImageGallery.InputHitTest(clickPoint);
			ListBoxItem clickedListBoxItem = null;
			if (element != null)
			{
				clickedListBoxItem = GetVisualParent<ListBoxItem>(element);
				if( clickedListBoxItem != null ) 
				{
					Fotografia f = (Fotografia)clickedListBoxItem.Content;
					if (!fotoGalleryViewModel.fotografieCW.SelectedItems.Contains(f))
					{
						fotoGalleryViewModel.fotografieCW.SelectedItems.Add(f);
						fotoGalleryViewModel.fotografieCW.RefreshSelectedItemWithMemory();
					}
				}
			}
			return clickedListBoxItem;
		}

		public T GetVisualParent<T>(object childObject) where T : Visual
		{
			DependencyObject child = childObject as DependencyObject;
			while ((child != null) && !(child is T))
			{
				child = VisualTreeHelper.GetParent(child);
			}
			return child as T;
		}

		private void buttonScorriFotoSelez_Click( object sender, RoutedEventArgs e ) {

			int direzione = Int32.Parse( ((FrameworkElement)sender).Tag.ToString() );

			fotoGalleryViewModel.calcolaFotoCorrenteSelezionataScorrimento( direzione );

			if( fotoGalleryViewModel.fotoCorrenteSelezionataScorrimento != null )
				LsImageGallery.ScrollIntoView( fotoGalleryViewModel.fotoCorrenteSelezionataScorrimento );

			forsePrendoSnapshotPubblico();
		}


		private short _quanteRigheVedo;
		public short quanteRigheVedo {
			get {
				return _quanteRigheVedo;
			}
			set {
				if( _quanteRigheVedo != value ) {

					_quanteRigheVedo = value;

					double dimensione = (LsImageGallery.ActualHeight / _quanteRigheVedo) - 6;
					fotoGalleryViewModel.dimensioneIconaFoto = dimensione;
				}
			}
		}


		private void buttonPageUp_Click( object sender, RoutedEventArgs e ) {
			ScrollViewer myScrollviewer = AiutanteUI.FindVisualChild<ScrollViewer>( LsImageGallery );
			myScrollviewer.PageUp();

			forsePrendoSnapshotPubblico();

			LsImageGallery.Focus();
		}
		private void buttonPageDown_Click( object sender, RoutedEventArgs e ) {
			ScrollViewer myScrollviewer = AiutanteUI.FindVisualChild<ScrollViewer>( LsImageGallery );
			myScrollviewer.PageDown();

			forsePrendoSnapshotPubblico();

			LsImageGallery.Focus();
		}

		private void buttonTakeSnapshotPubblico_Click( object sender, RoutedEventArgs e ) {
			((App)Application.Current).gestoreFinestrePubbliche.eseguiSnapshotSuFinestraPubblica( this, this.LsImageGallery );
		}

		private void closeSnapshotPubblico_Click( object sender, RoutedEventArgs e ) {
			((App)Application.Current).gestoreFinestrePubbliche.chiudiSnapshotPubblicoWindow();
		}

		void selezionareTutteLeFoto_Click( object sender, RoutedEventArgs e ) {
			if( fotoGalleryViewModel.selezionareTuttoCommand.CanExecute( "true" ) ) 
				fotoGalleryViewModel.selezionareTuttoCommand.Execute( "true" );
		}
		void deselezionareTutteLeFoto_Click( object sender, RoutedEventArgs e ) {
			if( fotoGalleryViewModel.selezionareTuttoCommand.CanExecute( "false" ) )
				fotoGalleryViewModel.selezionareTuttoCommand.Execute( "false" );
		}


		/// <summary>
		/// Solo se la finestra dello snapshot pubblico è già apera, allora prendo la foto.
		/// Se invece è chiusa non faccio niente.
		/// </summary>
		void forsePrendoSnapshotPubblico() {

			// Siccome la UI non è ancora stata ridisegnata, se faccio la foto adesso, vedo 
			// la situazione precedente.
			// Eseguo il comando quindi dopo che la UI si è ridisegnata
			this.Dispatcher.Invoke(
				DispatcherPriority.ApplicationIdle,
				new Action( () => {
					((App)Application.Current).gestoreFinestrePubbliche.eseguiSnapshotSuFinestraPubblica( this, this.LsImageGallery, false );
				} ) );

		}

		private void eseguireRicercaButton_Click( object sender, RoutedEventArgs e ) {

			if( panelFiltriHeight == 0 )
				panelFiltriHeight = expanderFiltriRicerca.ActualHeight;

			if( checkBoxCollassaFiltri.IsChecked == true )
				expanderFiltriRicerca.IsExpanded = false;
		}

	}
}
