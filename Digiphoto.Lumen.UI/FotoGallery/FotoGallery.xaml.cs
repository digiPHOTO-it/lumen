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

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for FotoGallery.xaml
	/// </summary>
	public partial class FotoGallery : UserControlBase {
		
		public FotoGallery() {
			InitializeComponent();

			DataContextChanged += new DependencyPropertyChangedEventHandler(fotoGallery_DataContextChanged);

        }

/*
		void fotoGalleryViewModel_riposizionaFocusEvent( object sender, EventArgs args ) {
			this.LsImageGallery.Focus();
		}
*/
		void fotoGallery_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {
			associaDialogProvider();

//			fotoGalleryViewModel.riposizionaFocusEvent += new FotoGalleryViewModel.RiposizionaFocusEventHandler( fotoGalleryViewModel_riposizionaFocusEvent );
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

			if( param == "+" )
				++quanteRigheVedo;
			else if( param == "-" ) {
				if( quanteRigheVedo > 1 )
					--quanteRigheVedo;
			}  else
				quanteRigheVedo = Convert.ToInt16( param );

			LsImageGallery.Focus();   // mi consente di usare il tasto pg/up pg/down.
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
			Fotografia foto = (Fotografia)SelectItemOnLeftClick(e).Content;
			if(ultimaSelezionata==null)
				ultimaSelezionata = (Fotografia)SelectItemOnLeftClick(e).Content;

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
			LsImageGallery.Focus();
		}
		private void buttonPageDown_Click( object sender, RoutedEventArgs e ) {
			ScrollViewer myScrollviewer = AiutanteUI.FindVisualChild<ScrollViewer>( LsImageGallery );
			myScrollviewer.PageDown();
			LsImageGallery.Focus();
		}


	}
}
