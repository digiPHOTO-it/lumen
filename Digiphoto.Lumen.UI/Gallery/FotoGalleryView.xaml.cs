﻿using System;
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
using Digiphoto.Lumen.UI.Pubblico;
using Digiphoto.Lumen.UI.Gallery;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Digiphoto.Lumen.UI.Gallery {


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

			// Ascolto l'associazione del viewmodel
			DataContextChanged += new DependencyPropertyChangedEventHandler( fotoGallery_DataContextChanged );

			// Carico lo stato della checkbox di collasso filtri, prendendolo dal file di last-used
			checkBoxCollassaFiltri.IsChecked = Configurazione.LastUsedConfigLumen.collassaFiltriInRicercaGallery;
		}

		GalleryUIRispetto galleryUIRispetto;
		void fotoGallery_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {

			associaDialogProvider();

			// Associo i ViewModel dei componenti interni
			selettoreMetadati.DataContext = fotoGalleryViewModel.selettoreMetadatiViewModel;

			selettoreAzioniAutomatiche.DataContext = fotoGalleryViewModel.selettoreAzioniAutomaticheViewModel;

			galleryUIRispetto = new GalleryUIRispetto( LsImageGallery, this );

			// Ascolto gli eventi di richiesta apertura popup da parte del viewmodel
			fotoGalleryViewModel.openPopupDialogRequest += viewModel_openPopupDialogRequest;
		}

		private void viewModel_openPopupDialogRequest( object sender, EventArgs e ) {

			if( e is CercaFotoPopupRequestEventArgs ) {

				CercaFotoPopupRequestEventArgs popEventArgs = (CercaFotoPopupRequestEventArgs)e;

				CercaFotoPopup win = new CercaFotoPopup();

				// Imposto la finestra contenitore per poter centrare
				win.Owner = this.parentWindow;

				// Questo è il viewmodel della finestra di popup				
				win.DataContext = fotoGalleryViewModel.cercaFotoPopupViewModel;

				var esito = win.ShowDialog();

				if( esito == true ) {
					// TODO
				}

				Console.WriteLine( esito );

				win.Close();
			}
		}





		#region Proprietà

		private FotoGalleryViewModel fotoGalleryViewModel
		{
			get
			{
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
				DateTime aa = (DateTime)giorni [0];
				DateTime bb = (DateTime)giorni [giorni.Count - 1];

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
		/// Originariamente, tramite il doppio click sulla foto, mandavo direttamente in modifica quella immagine.
		/// Ora invece mi posiziono la paginazione ad 1 HQ su quella foto
		/// </summary>
		private void listBoxItemImageGallery_MouseDoubleClick( object sender, RoutedEventArgs e ) {

			ListBoxItem lbItem = (ListBoxItem)sender; 
			var fotoCliccata = lbItem.Content as Fotografia;

			if( fotoGalleryViewModel.isAltaQualita )
				fotoGalleryViewModel.mandareInModificaImmediata( lbItem.Content as Fotografia );
			else
				fotoGalleryViewModel.cambiarePaginazioneCommand.Execute( fotoCliccata );

		}

		/// <summary>
		/// Devo gestire la selezione consecutiva quando avviene l'evento SHIFT + CLICK
		/// Problema da risolvere, è che adesso la selezione può avvenire anche a cavallo di pagine diverse
		/// Devo spostare tutta questa logica, lato viewModel, non è più compito di questo metodo
		/// </summary>
		/// <param name="sender">la ListBox che ha generato l'evento</param>
		/// <param name="e">evento del bottone del mouse</param>
		private void LsImageGallery_PreviewMouseLeftButtonDown( object sender, MouseButtonEventArgs e ) {

			// Se non ho il tasto SHIFT premuto, allora non devo fare niente.
			// Faccio subito questo controllo per evitare altre operazioni più lente e costose.
			// La stragrande maggioranza dei click, infatti, avviene senza lo shift premuto.
			// Il limiteA della selezione estesa (quello senza SHIFT), in questo caso, viene gestito dal ViewModel del metodo: fotografie_selezioneCambiata

			if( Keyboard.IsKeyDown( Key.LeftShift ) == false && Keyboard.IsKeyDown( Key.RightShift ) == false )
				return;

			// -- prendo la fotografia su cui ho cliccato
			Fotografia fotoLimiteB = getSelectedFotografiaOnMouseClick( e );
			if( fotoLimiteB == null )
				return;

			// Ok ho cliccato con lo shift, identificando quindi il secondo limite. Lo setto nel viewModel per avare una selezione completa (2 limiti)
			fotoGalleryViewModel.eseguireSelezioneEstesaCommand.Execute( fotoLimiteB );

			// Questo evento non è più da gestire perché ci ho già pensato prima nel ViewModel
			// viceversa, l'ultima foto cliccata riceverebbe un ulteriore click che la spegnerebbe (io invece sto accendendo)
			e.Handled = true;
		}

		private void LsImageGallery_PreviewKeyDown( object sender, KeyEventArgs e ) {
			if( e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control &&
				LsImageGallery.SelectedItems.Count > 0 ) {
				fotoGalleryViewModel.riportaOriginaleFotoSelezionateCommand.Execute( null );
				fotoGalleryViewModel.fotografieCW.deselezionaTutto();
			}
		}

		/// <summary>
		/// Quando viene cliccata la lista delle foto, voglio stabilire quale foto è stata selezionata,
		/// o per meglio dire voglio stabilire l'elemento della ListBox.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		private Fotografia getSelectedFotografiaOnMouseClick( System.Windows.Input.MouseButtonEventArgs e ) {

			// Anche se clicco sulla scrollbar mi solleva l'evento button down.
			if( !(e.OriginalSource is Image) )
				return null;

			var lbi = getSelectedItemOnLeftClick( e );
			return lbi == null ? null : (Fotografia)lbi.Content;
		}

		/// <summary>
		/// L'evento click avviene su tutta la ListBox (comprese aree vuote e anche scrollbar)
		/// Invece voglio isolare soltanto i click sulle immagini delle foto.
		/// Se il click è stato fatto su di una immagine, allora torno l'elemento della lista 
		/// interessato : il ListBoxItem .
		/// In tutti gli altri casi torno NULL
		/// </summary>
		/// <param name="e">l'evento click del mouse</param>
		/// <returns></returns>
		private ListBoxItem getSelectedItemOnLeftClick( System.Windows.Input.MouseButtonEventArgs e ) {
			Point clickPoint = e.GetPosition( LsImageGallery );
			object element = LsImageGallery.InputHitTest( clickPoint );
			ListBoxItem clickedListBoxItem = null;
			if( element != null )
				clickedListBoxItem = GetVisualParent<ListBoxItem>( element );

			return clickedListBoxItem;
		}

		private void LsImageGallery_PreviewMouseRightButtonDown( object sender, MouseButtonEventArgs e ) {

			if( !(e.OriginalSource is Image) )
				return;

			var foto = SelectItemOnRightClick( e );
			fotoGalleryViewModel.setRapideTargetSingolaFoto( foto );

			e.Handled = true;
		}

		/// <summary>
		/// Quando clicco con il destro, prima di aprire il menu contestuale,
		/// seleziono di giallo la foto che sta sotto il puntatore del mouse.
		/// Mi servirà poi per eseguire le azioni rapide, per esempio per stampare.
		/// </summary>
		/// <param name="e">evento click destro del mouse</param>
		/// <returns>la Fotografia che è stata cliccata e selezionata</returns>
		private Fotografia SelectItemOnRightClick( System.Windows.Input.MouseButtonEventArgs e ) {

			Fotografia foto = getSelectedFotografiaOnMouseClick( e );

			fotoGalleryViewModel.selezionareSingola( foto );

			return foto;
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





		private void checkBoxAreaRispetto_Click( object sender, RoutedEventArgs e ) {

			bool spegniForzatamente = checkBoxAreaRispetto.IsChecked == false;

			galleryUIRispetto.gestioneAreaStampabileHQ( spegniForzatamente );

			galleryUIRispetto.ascolta( checkBoxAreaRispetto.IsChecked == true );

		}

		#region Gestione Slider Spostamento

		private bool _azioneManuale = false;
		private bool _dragIniziato = false;

		// Questo binding mi serve per disabilitare il binding
		private Binding pauseBinding = new Binding( "FooInutile" ) {
			Mode = BindingMode.OneTime
		};

		private BindingExpression _saveSliderValueBinding;

		/// <summary>
		/// Mentre eseguo il drag della maniglietta dello slider, devo disabilitare il binding perché agisco sullo stesso
		/// controllo in cui ho il "Value" bindato in Get. (readonly)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void slider_DragStarted( object sender, DragStartedEventArgs e ) {

			_dragIniziato = true;

			fotoGalleryViewModel.startUnitOkWork();

			inizioSpostamentoSlider();
		}

		private void inizioSpostamentoSlider() {

			// Se già iniziato, non faccio niente
			if( _azioneManuale == true )
				return;

			_azioneManuale = true;

			// Mi salvo il binding e poi lo imposto su di uno che non fa niente. In pratica lo metto in pausa
			_saveSliderValueBinding = slider.GetBindingExpression( Slider.ValueProperty );
			slider.SetBinding( Slider.ValueProperty, pauseBinding );
			slider.Minimum = fotoGalleryViewModel.minPagineRicerca;
			slider.Maximum = fotoGalleryViewModel.totPagineRicerca;
			slider.Value = fotoGalleryViewModel.paginaAttualeRicerca;

			// Apro la tooltip
			sliderTooltipPopup.IsOpen = true;
		}

		void fineSpostamentoSlider() {

			if( _azioneManuale == false )
				return;

			_azioneManuale = false;
			sliderTooltipPopup.IsOpen = false;

			int paginaDesiderata = (int)slider.Value;
			int deltaPagine = (int)(paginaDesiderata - fotoGalleryViewModel.paginaAttualeRicerca);

			fotoGalleryViewModel.commandSpostarePaginazione.Execute( deltaPagine );

			// Rimetto a posto il binding che avevo messo in pausa
			BindingOperations.SetBinding( slider, Slider.ValueProperty, _saveSliderValueBinding.ParentBinding );
		}

		private void slider_DragCompleted( object sender, DragCompletedEventArgs e ) {
			
			_dragIniziato = false;

			fineSpostamentoSlider();

			fotoGalleryViewModel.stopUnitOkWork();
		}

		#endregion Gestione Slider Spostamento

		private void slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {

			fineSpostamentoSlider();

			if( _azioneManuale ) {

				if( _dragIniziato == true ) {
					// Ancora non devo fare nulla perché sto facendo il drag. 
					// L'azione la farò al termine del drag
				} else {
					// Se arrivo qui significa che ho cliccato sullo slider (non sulla maniglia)
					// Questo ha provocato lo spostamento secco del valore 
					// fineSpostamentoSlider();
				}

			} else {

				// Il valore dello slider è stato modificato dal binding e non da una mossa dell'utente sul componente.
				// Quindi non devo fare niente
			}
		}

		private void slider_PreviewMouseDown( object sender, MouseButtonEventArgs e ) {
			inizioSpostamentoSlider();
		}

		private void slider_PreviewMouseUp( object sender, MouseButtonEventArgs e ) {
			fineSpostamentoSlider();
		}

		private void slider_DragDelta( object sender, DragDeltaEventArgs e ) {

			int numPagina = Convert.ToInt32( ((Slider)sender).Value );
			int numeroFotogramma = fotoGalleryViewModel.getPrimoFotogrammaPagina( numPagina );
			
			sliderTooltipTextBlock.Text = numeroFotogramma.ToString();
		}
	}
}
