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
using System.Windows.Shapes;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Util;

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

			DataContextChanged += new DependencyPropertyChangedEventHandler(fotoGallery_DataContextChanged);

			// Carico lo stato della checkbox di collasso filtri, prendendolo dal file di last-used
			checkBoxCollassaFiltri.IsChecked = Configurazione.LastUsedConfigLumen.collassaFiltriInRicercaGallery;
		}

		void fotoGallery_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {

			associaDialogProvider();

			// Associo i ViewModel dei componenti interni
			selettoreMetadati.DataContext = fotoGalleryViewModel.selettoreMetadatiViewModel;

			selettoreAzioniAutomatiche.DataContext = fotoGalleryViewModel.selettoreAzioniAutomaticheViewModel;

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
			TimeSpan unGiorno = new TimeSpan( 1, 0, 0, 0 );
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
				DateTime aa = (DateTime)giorni[0];
				DateTime bb = (DateTime)giorni[giorni.Count - 1];

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

			ListBoxItem lbItem = (ListBoxItem)sender;
			fotoGalleryViewModel.mandareInModificaImmediata( lbItem.Content as Fotografia );
		}

		private void LsImageGallery_PreviewMouseLeftButtonDown( object sender, MouseButtonEventArgs e ) {
			// Anche se clicco sulla scrollbar mi solleva l'evento button down.
			if( !(e.OriginalSource is Image) )
				return;

			//
			ListBoxItem lbi = SelectItemOnLeftClick( e );
			if( lbi == null )
				return;

			Fotografia foto = (Fotografia)lbi.Content;

			if( Keyboard.IsKeyDown( Key.LeftShift ) || Keyboard.IsKeyDown( Key.RightShift ) ) {
				if( fotoGalleryViewModel.fotografieCW.SelectedItems.Count > 0 ) {
					Fotografia lastSelectedFoto = fotoGalleryViewModel.fotografieCW.SelectedItems.Last<Fotografia>();
					int firstIndex = fotoGalleryViewModel.fotografieCW.IndexOf( lastSelectedFoto );
					int lastIndex = fotoGalleryViewModel.fotografieCW.IndexOf( foto );

					//se ho selezionato dal più alto al più basso inverto gli indici
					if( firstIndex > lastIndex ) {
						int appoggio = firstIndex;
						//faccio +1 perche se no non riesco a selezionare l'ultima foto
						firstIndex = lastIndex + 1;
						lastIndex = appoggio;
					}

					for( int i = firstIndex; i < lastIndex; i++ ) {
						Fotografia f = (Fotografia)fotoGalleryViewModel.fotografieCW.GetItemAt( i );
						if( !fotoGalleryViewModel.fotografieCW.SelectedItems.Contains( f ) ) {
							fotoGalleryViewModel.fotografieCW.SelectedItems.Add( f );
							fotoGalleryViewModel.fotografieCW.RefreshSelectedItemWithMemory();
						}
					}
				}
			}
		}

		private void LsImageGallery_PreviewKeyDown( object sender, KeyEventArgs e ) {
			if( e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control &&
				LsImageGallery.SelectedItems.Count > 0 ) {
				fotoGalleryViewModel.riportaOriginaleFotoSelezionateCommand.Execute( null );
				fotoGalleryViewModel.fotografieCW.deselezionaTutto();
			}
		}

		private ListBoxItem SelectItemOnLeftClick( System.Windows.Input.MouseButtonEventArgs e ) {
			Point clickPoint = e.GetPosition( LsImageGallery );
			object element = LsImageGallery.InputHitTest( clickPoint );
			ListBoxItem clickedListBoxItem = null;
			if( element != null ) {
				clickedListBoxItem = GetVisualParent<ListBoxItem>( element );
				if( clickedListBoxItem != null ) {
					Fotografia f = (Fotografia)clickedListBoxItem.Content;
				}
			}
			return clickedListBoxItem;
		}

		private void LsImageGallery_PreviewMouseRightButtonDown( object sender, MouseButtonEventArgs e ) {

			if( !(e.OriginalSource is Image) )
				return;

			Fotografia foto = (Fotografia)SelectItemOnRightClick( e ).Content;
			fotoGalleryViewModel.setModalitaSingolaFoto( foto );
			e.Handled = true;
		}

		private ListBoxItem SelectItemOnRightClick( System.Windows.Input.MouseButtonEventArgs e ) {
			Point clickPoint = e.GetPosition( LsImageGallery );
			object element = LsImageGallery.InputHitTest( clickPoint );
			ListBoxItem clickedListBoxItem = null;
			if( element != null ) {
				clickedListBoxItem = GetVisualParent<ListBoxItem>( element );
				if( clickedListBoxItem != null ) {
					Fotografia f = (Fotografia)clickedListBoxItem.Content;
					if( !fotoGalleryViewModel.fotografieCW.SelectedItems.Contains( f ) ) {
						fotoGalleryViewModel.fotografieCW.SelectedItems.Add( f );
						fotoGalleryViewModel.fotografieCW.RefreshSelectedItemWithMemory();
					}
				}
			}
			return clickedListBoxItem;
		}

		public T GetVisualParent<T>( object childObject ) where T : Visual {
			DependencyObject child = childObject as DependencyObject;
			while( (child != null) && !(child is T) ) {
				child = VisualTreeHelper.GetParent( child );
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
				Configurazione.LastUsedConfigLumen.collassaFiltriInRicercaGallery = (bool)checkBoxCollassaFiltri.IsChecked;
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


		private bool posizionaListaSulFotogramma( int numDaric ) {

			Fotografia daric = ricavaFotoByNumber( numDaric );
			if( daric != null )
				LsImageGallery.ScrollIntoView( daric );

			return daric != null;
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
					if( AiutanteUI.IsUserVisible( ele, LsImageGallery ) ) {

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


		private void FotoGalleryViewModel_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e ) {

			bool cambiaAreaStampabile = false;

			// Significa che ho cambiato le impostazioni di visualizzazione
			if( e.PropertyName == "devoVisualizzareAreaDiRispettoHQ" )
				cambiaAreaStampabile = true;

			if( fotoGalleryViewModel.isAltaQualita ) {
				// Significa che mi sono spostato alla foto successiva in HQ
				if( e.PropertyName == "fotografieCW" || 
				    e.PropertyName == "numRighe" || e.PropertyName == "numColonne" ||
					e.PropertyName == "numRighePag" || e.PropertyName == "numColonnePag" )
						cambiaAreaStampabile = true;
			}

			if( cambiaAreaStampabile ) {
				// Devo farlo nel thread della UI altrimenti non si sono ancora disposti i componenti grafici correttamente
				this.Dispatcher.BeginInvoke( gestioneAreaStampabileHQAction );
			}

		}


		Action _gestioneAreaStampabileHQAction;
		Action gestioneAreaStampabileHQAction
		{
			get
			{
				if( _gestioneAreaStampabileHQAction == null )
					_gestioneAreaStampabileHQAction = new Action( gestioneAreaStampabileHQ );
				return _gestioneAreaStampabileHQAction;
			}
		}

		
		private void gestioneAreaStampabileHQ() {
			gestioneAreaStampabileHQ( false );
		}

		// Questo è lo style per decorare i rettangoli
		Style coperturaRispettoStyle;

		private void gestioneAreaStampabileHQ( bool spegniForzatamente ) {

			// Se non so il ratio dell'area stampabile, esco subito
			if( fotoGalleryViewModel.ratioAreaStampabile == 0f )
				return;

			if( spegniForzatamente == false && checkBoxAreaRispetto.IsChecked == false )
				return;
				
			if( spegniForzatamente == false && fotoGalleryViewModel.isAltaQualita ) {

				if( fotoGalleryViewModel.fotografieCW.Count > 2 )
					return;
				
				LsImageGallery.UpdateLayout();
				

				if( coperturaRispettoStyle == null )
					coperturaRispettoStyle = this.FindResource( "coperturaRispettoStyle" ) as Style;
					
				// Le foto in alta qualità possono essere 1 oppure 2 affiancate
				foreach( Fotografia foto in fotoGalleryViewModel.fotografieCW ) { 

					Rectangle[] rettangoli = new Rectangle[2];
					bool esiste = false;

					Grid fotoGrid = findFotoGrid( foto );

					for( char ab = 'A'; ab <= 'B'; ab++ ) {

						// -- areaStampabileA - areaStampabileB
						int pos = ab - 'A';
						string nome = string.Format( "areaStampabile{0}", ab );
						
						rettangoli[pos] = (Rectangle)AiutanteUI.FindChild( fotoGrid, nome, typeof( Rectangle ) );
						
						esiste = rettangoli[pos] != null;
						if( !esiste ) {
							rettangoli[pos] = new Rectangle();
							rettangoli[pos].Name = nome;
							rettangoli[pos].Style = coperturaRispettoStyle;
						}

						Panel.SetZIndex( rettangoli[pos], 25 );
					}

					dimensionaRettangoloPerAreaDiRispetto( rettangoli, fotoGrid );


					if( ! esiste ) {
						fotoGrid.Children.Add( rettangoli[0] );
						fotoGrid.Children.Add( rettangoli[1] );
					}
				}

			} else {

				// Elimino tutti i rettangoli. Possono essere 2 o 4
				for( int rr = 1; rr <= 2; rr++ )
					for( char ab = 'A'; ab <= 'B'; ab++ ) {
						string nome = string.Format( "areaStampabile{0}", ab );
						Rectangle rettangolo = (Rectangle)AiutanteUI.FindChild( gridImges, nome, typeof( Rectangle ) );
						if( rettangolo != null )
							gridImges.Children.Remove( rettangolo );
					}
			}
		}

		static float ratioRispetto = (float)CoreUtil.evaluateExpression( Configurazione.UserConfigLumen.expRatioAreaDiRispetto );

		/// <summary>
		/// Questa grid viene creata a runtime per ogni foto che viene iterata dalla listbox.
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		Grid findFotoGrid( Fotografia f ) {
			return findComponentFromTemplate<Grid>( f, "fotoGrid" );
		}

		Image findFotoImage( Fotografia f ) {
			return findComponentFromTemplate<Image>( f, "fotoImage" );
		}

		T findComponentFromTemplate<T>( Fotografia f, string nomeComponente ) {

			// Per ricavare il componente desiderato, devo fare diversi passaggi

			// 2. dalla foto ricavo il ListBoxItem che la contiene
			ListBoxItem listBoxItem = (ListBoxItem)(LsImageGallery.ItemContainerGenerator.ContainerFromItem( f ));
			// 3. dal ListBoxItem ricavo il suo ContentPresenter
			ContentPresenter contentPresenter = AiutanteUI.FindVisualChild<ContentPresenter>( listBoxItem );
			// 4. con il ContentPresenter ricavo il DataTemplate (del singolo elemento)
			DataTemplate dataTemplate = contentPresenter.ContentTemplate;
			// 5. con il DataTemplate ricavo l'Image contenuta

			return (T) dataTemplate.FindName( nomeComponente, contentPresenter );
		}

		void dimensionaRettangoloPerAreaDiRispetto( Rectangle [] rettangoli, Grid fotoGrid ) {

			try {
				Rectangle rectA = rettangoli[0];
				Rectangle rectB = rettangoli[1];

				// Ora ricalcolo la dimensione dell'area di rispetto
				// float ratio = fotoGalleryViewModel.ratioAreaStampabile;
				if( fotoGalleryViewModel.ratioAreaStampabile == 0f )
					return;

				

				Image fotoImage = (Image) fotoGrid.FindName( "fotoImage" );
				
				CalcolatoreAreeRispetto.Geo imageGeo = new CalcolatoreAreeRispetto.Geo();

				imageGeo.w = fotoImage.ActualWidth;
				imageGeo.h = fotoImage.ActualHeight;

				// Calcolo la fascia A
				Rect rettangoloA = CalcolatoreAreeRispetto.calcolaDimensioni( CalcolatoreAreeRispetto.Fascia.FasciaA, ratioRispetto, imageGeo );
				CalcolatoreAreeRispetto.Bordi bordiA = CalcolatoreAreeRispetto.calcolcaLatiBordo( CalcolatoreAreeRispetto.Fascia.FasciaA, ratioRispetto, imageGeo, imageGeo );

				// Calcolo la fascia B
				Rect rettangoloB = CalcolatoreAreeRispetto.calcolaDimensioni( CalcolatoreAreeRispetto.Fascia.FasciaB, ratioRispetto, imageGeo );
				CalcolatoreAreeRispetto.Bordi bordiB = CalcolatoreAreeRispetto.calcolcaLatiBordo( CalcolatoreAreeRispetto.Fascia.FasciaB, ratioRispetto, imageGeo, imageGeo );

				// Calcolo left e top in base alla posizione della immagine rispetto alla grid che la contiene
				var qq = fotoImage.TransformToAncestor( fotoGrid );
				Point relativePoint = qq.Transform( new Point( 0, 0 ) );
				double currentLeft = relativePoint.X;
				double currentTop = relativePoint.Y;

				// Setto fascia A
				rectA.Width = rettangoloA.Width;
				rectA.Height = rettangoloA.Height;
				var left = currentLeft + rettangoloA.Left;
				var top = currentTop + rettangoloA.Top;
				var right = 0;
				var bottom = 0;

				Thickness ticA = new Thickness( left, top, right, bottom );
				rectA.Margin = ticA;

				// ---

				// Setto fascia B
				rectB.Width = rettangoloB.Width;
				rectB.Height = rettangoloB.Height;
				left = currentLeft + rettangoloB.Left;
				top = currentTop + rettangoloB.Top;
				right = 0;
				bottom = 0;

				Thickness ticB = new Thickness( left, top, right, bottom );
				rectB.Margin = ticB;

			} catch( Exception ee ) {
				// pazienza : dovrei loggare l'errore
				int a = 3;
			}
		}

		private void checkBoxAreaRispetto_Click( object sender, RoutedEventArgs e ) {

			bool spegniForzatamente = checkBoxAreaRispetto.IsChecked == false;

			gestioneAreaStampabileHQ( spegniForzatamente );

			// Abilito / Disabilito l'ascolto dei cambi di property
			if( checkBoxAreaRispetto.IsChecked == true ) {
				// Metto un ascoltatore su tutte le property. perché devo sentire i cambi della AltaQualità
				fotoGalleryViewModel.PropertyChanged += FotoGalleryViewModel_PropertyChanged;
			} else {
				fotoGalleryViewModel.PropertyChanged -= FotoGalleryViewModel_PropertyChanged;
			}
		}
	}
}
