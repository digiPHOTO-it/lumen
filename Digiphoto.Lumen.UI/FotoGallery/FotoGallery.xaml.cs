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
using System.Collections.Generic;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.UI {

	
	/// <summary>
	/// Interaction logic for FotoGallery.xaml
	/// </summary>
	public partial class FotoGallery : UserControlBase {

		private const string GOTO_VUOTO = "GOTO_VUOTO";
		private const string GOTO_ERRATO = "GOTO_ERRATO";
		private const string GOTO_EDITING = "GOTO_EDITING";

		public FotoGallery() {
			InitializeComponent();

			textBoxGotoNumFoto.Tag = GOTO_VUOTO;

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
			//sistemaMarginePerVedereDueFotoAffiancate( quante );

			/*
			ContentPresenter myContentPresenter = AiutanteUI.FindVisualChild<ContentPresenter>(LsImageGallery);
			
			ScrollViewer myScrollviewer = AiutanteUI.FindVisualChild<ScrollViewer>(LsImageGallery);
			
			List<Fotografia> viewFotos = GetVisibleItemsFromListbox(LsImageGallery, myScrollviewer);
			Fotografia viewFoto = null;
			if (viewFotos != null && viewFotos.Count > 0)
				viewFoto = viewFotos.First<Fotografia>();

			List<Fotografia> viewFotos2 = GetVisibleItemsFromListbox(LsImageGallery, myContentPresenter);
			Fotografia viewFoto2 = null;
			if (viewFotos2 != null && viewFotos2.Count > 0)
				viewFoto2 = viewFotos2.First<Fotografia>();
			*/

			List<Fotografia> viewFotos3 = GetVisibleItemsFromListbox(LsImageGallery, LsImageGallery);
			Fotografia viewFoto3 = null;
			if (viewFotos3 != null && viewFotos3.Count > 0){
				if (LsImageGallery.SelectedItems.Count > 0)
				{
					viewFoto3 = viewFotos3.FirstOrDefault<Fotografia>(element => LsImageGallery.SelectedItems.Contains(element));
					if (viewFoto3 == null)
					{
						viewFoto3 = viewFotos3.First<Fotografia>();
					}
				}
				else
				{
					viewFoto3 = viewFotos3.First<Fotografia>();
				}
			}

			// poi questo
			quanteRigheVedo = quante;

			//Mi posiziono sulla foto su cui avevo il focus
			if (viewFoto3!=null)
				posizionaListaSulFotogramma(viewFoto3.numero);
		}

		private List<Fotografia> GetVisibleItemsFromListbox(ListBox listBox, FrameworkElement parentToTestVisibility)
		{
			var items = new List<Fotografia>();

			foreach (var item in LsImageGallery.Items)
			{
				if (IsUserVisible((ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(item), parentToTestVisibility))
				{
					items.Add((Fotografia)item);
				}
				else if (items.Any())
				{
					break;
		}
			}

			return items;
		}

		private static bool IsUserVisible(FrameworkElement element, FrameworkElement container)
		{
			if (!element.IsVisible)
				return false;

			Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
			var rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
			return rect.Contains(bounds.TopLeft) && rect.Contains(bounds.BottomRight);
		}

		protected bool IsFullyOrPartiallyVisible(FrameworkElement child, FrameworkElement scrollViewer)
		{
			if (!child.IsVisible)
				return false;

			var childTransform = child.TransformToAncestor(scrollViewer);
			var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
			var ownerRectangle = new Rect(new Point(0, 0), scrollViewer.RenderSize);
			return ownerRectangle.IntersectsWith(childRectangle);
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

		private void LsImageGallery_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control && 
				LsImageGallery.SelectedItems.Count > 0)
			{
				fotoGalleryViewModel.riportaOriginaleFotoSelezionateCommand.Execute(null);
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
			spostamentoScrollerSoloSuSelezionate( direzione );
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


		// TODO DACANC
		private void buttonPageUp_Click___DACANC( object sender, RoutedEventArgs e ) {

			ContentPresenter myContentPresenter = AiutanteUI.FindVisualChild<ContentPresenter>( LsImageGallery );
			ListBoxItem co = (ListBoxItem)LsImageGallery.ItemContainerGenerator.ContainerFromIndex( 0 );

			ScrollViewer myScrollviewer = AiutanteUI.FindVisualChild<ScrollViewer>( LsImageGallery );

			double newOffset = 0;
			if( myScrollviewer.VerticalOffset % co.ActualHeight == 0 )
				newOffset = myScrollviewer.VerticalOffset - co.ActualHeight;	
			else
				newOffset = (int)(myScrollviewer.VerticalOffset / co.ActualHeight) - co.ActualHeight;

			myScrollviewer.ScrollToVerticalOffset( newOffset );

			forsePrendoSnapshotPubblico();

			LsImageGallery.Focus();

		}

		private void buttonMovePage_Click( object sender, RoutedEventArgs e ) {

			int direzione = Convert.ToInt32( ((Button)sender).CommandParameter );

			if( fotoGalleryViewModel.flagPosizionaSuSelezionate ) {

				// Spostamento solo sulle selezionate ..
				spostamentoScrollerSoloSuSelezionate( direzione );

			} else {
				// Spostamento normale mi sposto di una pagina
				spostamentoScrollerNormaleSuGiu( direzione );
			}

		}

		/// <summary>
		/// Posiziono lo scroller in modo da avere sempre una fila di foto "piena" 
		/// e non posizionata a metà nel video.
		/// </summary>
		/// <param name="direzione">
		///   -1 = una pagina indietro (Pg Up)
		///    0 = risposiziono su stessa pagina
		///   +1 = una pagina avanti (Pg Down)
		/// </param>
		void spostamentoScrollerNormaleSuGiu( int direzione ) {

			ContentPresenter myContentPresenter = AiutanteUI.FindVisualChild<ContentPresenter>( LsImageGallery );
			ListBoxItem co = (ListBoxItem)LsImageGallery.ItemContainerGenerator.ContainerFromIndex( 0 );
			if( co == null )
				return;  // Non dovrebbe succedere ma per sicurezza...
			double elemH = co.ActualHeight;

			ScrollViewer myScrollviewer = AiutanteUI.FindVisualChild<ScrollViewer>( LsImageGallery );

			// Prima calcolo quante righe di foto intere ci stanno a video in questo momento
			int quanteRighe = (int)(myContentPresenter.ActualHeight / elemH);
			if( quanteRighe == 0 )
				quanteRighe = 1;   // Questa correzione serve perché ci sono dei pixel del bordo che sfasano di poco il calcolo

			// ora mi sposto alla prossima riga visibile (esempio se vedo adesso 4 righe, mi sposto sulla 5°)
			double incremento = (direzione * co.ActualHeight * quanteRighe);
			double posizioneFutura = myScrollviewer.VerticalOffset + incremento;
			int numrighe = (int)(posizioneFutura / elemH);

			if( posizioneFutura % elemH != 0 ) {
				// Se mi andrò a posizionare in un punto che non è multiplo della altezza foto, 
				// Significa che vedrò la fila di foto tagliata. Cerco di posizionare

				// Questo dovrebbe fare al posto dello switch seguente
				posizioneFutura = (numrighe - direzione) * elemH;
/*
				switch( direzione ) {

					case -1:
						// Indietro
						posizioneFutura = (numrighe + 1) * elemH;
						break;

					case 0:
						// non mi muovo rimango sulla pagina attuale
						posizioneFutura = (numrighe) * elemH;
						break;

					case 1:
						// Avanti: non so quando capita. Vediamo
						var qq = 1;
						posizioneFutura = (numrighe - 1) * elemH;  // TODO da testare non so se funziona
						break;
				}
 */
			}

			if( posizioneFutura < 0 )
				posizioneFutura = 0;
			// TODO : testare anche il massimo ?

			// Sistemo la posizione delle foto
			myScrollviewer.ScrollToVerticalOffset( posizioneFutura );

			// Faccio vedere la schermata al cliente
			forsePrendoSnapshotPubblico();

			// Rimetto il fuoco sul componente gallery, in modo che se premo le freccie su/giu mi sposto correttamente
			LsImageGallery.Focus();
		}

		/// <summary>
		/// Eseguo lo spostamento della finestra dello scroller per andare a posizionarmi
		/// sulla foto selezionata successiva.
		/// </summary>
		/// <param name="direzione"></param>
		void spostamentoScrollerSoloSuSelezionate( int direzione ) {

			fotoGalleryViewModel.calcolaFotoCorrenteSelezionataScorrimento( direzione );

			if( fotoGalleryViewModel.fotoCorrenteSelezionataScorrimento != null )
				LsImageGallery.ScrollIntoView( fotoGalleryViewModel.fotoCorrenteSelezionataScorrimento );

			forsePrendoSnapshotPubblico();
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

			// Aggiorno la vista del cliente
			forsePrendoSnapshotPubblico();
		}

#if false
		private void iteraAlberoVisibili() {

			foreach( var item in LsImageGallery.Items ) {
			
				var listBoxItem = (ListBoxItem)LsImageGallery.ItemContainerGenerator.ContainerFromItem( item );

				if( IsUserVisible( listBoxItem, LsImageGallery ) ) {
					iteraAlberoFigli( listBoxItem );
				}
			}
		}
#endif

		void iteraAlberoFigli( FrameworkElement ele ) {

			int quanti = VisualTreeHelper.GetChildrenCount( ele );
			for( int i = 0; i < quanti; i++ ) {
				var child = VisualTreeHelper.GetChild( ele, i ) as FrameworkElement;
				if( child.Name == "fotoCanvas" ) {

					// Per far prima, invece che applicarlo a tutti, la applico soltanto ai componenti visibili
					if( IsUserVisible( ele, LsImageGallery )) {

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
	}
}
