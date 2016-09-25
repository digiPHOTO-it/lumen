using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Util;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.UI {


	/// <summary>
	/// Interaction logic for FotoGallery.xaml
	/// </summary>
	public partial class FotoGallery : UserControlBase {


		private enum TestVisibilita {
			Piena,
			Parziale
		};

		private enum QuanteVisibilita {
			SoloPrima,
			Tutte
		};

		private const string GOTO_VUOTO = "GOTO_VUOTO";
		private const string GOTO_ERRATO = "GOTO_ERRATO";
		private const string GOTO_EDITING = "GOTO_EDITING";

		public FotoGallery() {
			InitializeComponent();

			textBoxGotoNumFoto.Tag = GOTO_VUOTO;

			DataContextChanged += new DependencyPropertyChangedEventHandler(fotoGallery_DataContextChanged);

			// Carico lo stato della checkbox di collasso filtri, prendendolo dal file di last-used
			checkBoxCollassaFiltri.IsChecked = Configurazione.LastUsedConfigLumen.collassaFiltriInRicercaGallery;
        }

		void fotoGallery_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {
			associaDialogProvider();
		}

		

		#region Proprietà
		private FotoGalleryViewModel fotoGalleryViewModel {
			get {
				return (FotoGalleryViewModel)base.viewModelBase;
			}
		}
		#endregion

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

		private void LsImageGallery_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			// Anche se clicco sulla scrollbar mi solleva l'evento button down.
			if( ! (e.OriginalSource is Image) )	
				return;

			//
			ListBoxItem lbi = SelectItemOnLeftClick( e );
			if (lbi == null)
				return;

			Fotografia foto = (Fotografia)lbi.Content;
			
			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
			{
				if (fotoGalleryViewModel.fotografieCW.SelectedItems.Count > 0)
				{
					Fotografia lastSelectedFoto = fotoGalleryViewModel.fotografieCW.SelectedItems.Last<Fotografia>();
					int firstIndex = fotoGalleryViewModel.fotografieCW.IndexOf(lastSelectedFoto);
					int lastIndex = fotoGalleryViewModel.fotografieCW.IndexOf(foto);

					//se ho selezionato dal più alto al più basso inverto gli indici
					if (firstIndex > lastIndex)
					{
						int appoggio = firstIndex;
						//faccio +1 perche se no non riesco a selezionare l'ultima foto
						firstIndex = lastIndex + 1;
						lastIndex = appoggio;
					}

					for (int i = firstIndex; i < lastIndex; i++)
					{
						Fotografia f = (Fotografia)fotoGalleryViewModel.fotografieCW.GetItemAt(i);
						if (!fotoGalleryViewModel.fotografieCW.SelectedItems.Contains(f))
						{
							fotoGalleryViewModel.fotografieCW.SelectedItems.Add(f);
							fotoGalleryViewModel.fotografieCW.RefreshSelectedItemWithMemory();
						}
					}
				}
			}
		}

		private void LsImageGallery_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control && 
				LsImageGallery.SelectedItems.Count > 0)
			{
				fotoGalleryViewModel.riportaOriginaleFotoSelezionateCommand.Execute(null);
				fotoGalleryViewModel.fotografieCW.deselezionaTutto();
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

			Fotografia foto = (Fotografia)SelectItemOnRightClick( e ).Content;
			fotoGalleryViewModel.setModalitaSingolaFoto( foto );
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
			// TODO da fare
		}

		void selezionareTutteLeFoto_Click( object sender, RoutedEventArgs e ) {
			if( fotoGalleryViewModel.selezionareTuttoCommand.CanExecute( "true" ) ) 
				fotoGalleryViewModel.selezionareTuttoCommand.Execute( "true" );
		}
		void deselezionareTutteLeFoto_Click( object sender, RoutedEventArgs e ) {
			if( fotoGalleryViewModel.selezionareTuttoCommand.CanExecute( "false" ) )
				fotoGalleryViewModel.selezionareTuttoCommand.Execute( "false" );
		}



		private void eseguireRicercaButton_Click( object sender, RoutedEventArgs e ) {

			// Se è cambiato lo stato del flag di ricerca, lo memorizzo nei last-used
			if( checkBoxCollassaFiltri.IsChecked != Configurazione.LastUsedConfigLumen.collassaFiltriInRicercaGallery ) {
				Configurazione.LastUsedConfigLumen.collassaFiltriInRicercaGallery = (bool) checkBoxCollassaFiltri.IsChecked;
				try {
					Configurazione.SalvaLastUsedConfig();
				} catch( Exception ) {

				}
			}

			if( checkBoxCollassaFiltri.IsChecked == true )
				expanderFiltriRicerca.IsExpanded = false;
		}


		/// <summary>
		/// cerca nella collezione delle foto filtrate,
		/// se ne esiste una con il numero indicato.
		/// </summary>
		/// <param name="numero"></param>
		/// <returns></returns>
		private Fotografia ricavaFotoByNumber( int numDaric ) {

			// Devo scorrere la lista
			Fotografia fotoTrovata = null;
			if( fotoGalleryViewModel.fotografieCW != null )
				foreach( var foto in fotoGalleryViewModel.fotografieCW.SourceCollection ) {
					if( ((Fotografia)foto).numero == numDaric ) {
						fotoTrovata = (Fotografia)foto;
						break;
					}
				}

			return fotoTrovata;
		}


		private void textBoxGotoNumFoto_GotFocus( object sender, RoutedEventArgs e ) {
			if( (string)textBoxGotoNumFoto.Tag == GOTO_VUOTO ) {
				textBoxGotoNumFoto.Text = "";
				textBoxGotoNumFoto.Tag = GOTO_EDITING;
			}
		}

		private void textBoxGotoNumFoto_LostFocus( object sender, RoutedEventArgs e ) {

			// Caso particolare: se svuoto la cella, allora rimetto la scritta "goto"
			if( textBoxGotoNumFoto.Text == null || textBoxGotoNumFoto.Text.Trim() == String.Empty ) {
				textBoxGotoNumFoto.Tag = GOTO_VUOTO;
				textBoxGotoNumFoto.Text = "N°foto..."; // occhio che questa scritta c'è anche in FotoGallery.xaml
				return;
			}

			bool posizionato = false;
			int numDaric;
			if( Int32.TryParse( textBoxGotoNumFoto.Text, out numDaric ) )
				posizionato = posizionaListaSulFotogramma( numDaric );

			if( posizionato ) {
				textBoxGotoNumFoto.Tag = GOTO_VUOTO;
				textBoxGotoNumFoto.Text = "N°foto..."; // occhio che questa scritta c'è anche in FotoGallery.xaml
			} else
				textBoxGotoNumFoto.Tag = GOTO_ERRATO;
		}

		private bool posizionaListaSulFotogramma( int numDaric ) { 

			Fotografia daric = ricavaFotoByNumber( numDaric );
			if( daric != null )
				LsImageGallery.ScrollIntoView( daric );

			return daric != null;
		}

		private void posizionaListaSulFotogrammaSS( object sender, RoutedEventArgs e ) {
	
			int nn = fotoGalleryViewModel.numFotoCorrenteInSlideShow;
			if( nn > 0 ) {
				posizionaListaSulFotogramma( nn );
				textBoxGotoNumFoto.Text = nn.ToString();
			} else
				textBoxGotoNumFoto.Text = "";
		}

		private void mostraAreaStampabileButton_Click( object sender, RoutedEventArgs e ) {


			ContentPresenter myContentPresenter = AiutanteUI.FindVisualChild<ContentPresenter>( LsImageGallery );
			// ListBoxItem co = (ListBoxItem)LsImageGallery.ItemContainerGenerator.ContainerFromIndex( 0 );

			iteraAlberoFigli( myContentPresenter );
			// iteraAlberoVisibili();


		}



		void iteraAlberoFigli( FrameworkElement ele ) {

			int quanti = VisualTreeHelper.GetChildrenCount( ele );
			for( int i = 0; i < quanti; i++ ) {
				var child = VisualTreeHelper.GetChild( ele, i ) as FrameworkElement;
				if( child.Name == "fotoCanvas" ) {

					// Per far prima, invece che applicarlo a tutti, la applico soltanto ai componenti visibili
					if( AiutanteUI.IsUserVisible( ele, LsImageGallery )) {

						Canvas canvas = (Canvas)child;

						// prima controllo che non ce l'ho già messo
						if( AiutanteUI.FindVisualChild<Border>( canvas, "borderCopriA" ) == null ) {
							Border borderA = new Border();
							borderA.Name = "borderCopriA";
							borderA.HorizontalAlignment = HorizontalAlignment.Left;
							borderA.VerticalAlignment = VerticalAlignment.Top;
							borderA.Style = (Style)FindResource( "styleBorderCopriA" );
							canvas.Children.Add( borderA );
							borderA.SetValue( Canvas.ZIndexProperty, 99 );
						}

						if( AiutanteUI.FindVisualChild<Border>( canvas, "borderCopriB" ) == null ) {
							Border borderB = new Border();
							borderB.Name = "borderCopriB";
							borderB.HorizontalAlignment = HorizontalAlignment.Left;
							borderB.VerticalAlignment = VerticalAlignment.Top;
							borderB.Style = (Style)FindResource( "styleBorderCopriB" );
							canvas.Children.Add( borderB );
							borderB.SetValue( Canvas.ZIndexProperty, 99 );
						}
					}
				} else {
					iteraAlberoFigli( child );
				}
			}
		}

		private void LsImageGallery_PreviewMouseWheel( object sender, MouseWheelEventArgs e ) {

			// paginazione avanti / indietro
			short direzione = e.Delta < 0 ? (short)+1 : (short)-1;


			if( Keyboard.IsKeyDown( Key.LeftCtrl ) ) {

				// modifico il numero di righe visibili
				if( fotoGalleryViewModel.numRighePag + direzione >= updownNumRighe.Minimum && fotoGalleryViewModel.numRighePag + direzione <= updownNumRighe.Maximum )
					fotoGalleryViewModel.numRighePag += direzione;

				if( fotoGalleryViewModel.numColonnePag + direzione >= updownNumColonne.Minimum && fotoGalleryViewModel.numColonnePag + direzione <= updownNumColonne.Maximum )
					fotoGalleryViewModel.numColonnePag += direzione;

			} else {
				if( fotoGalleryViewModel.commandSpostarePaginazione.CanExecute( direzione ) )
					fotoGalleryViewModel.commandSpostarePaginazione.Execute( direzione );
			}
		}
	}
}
