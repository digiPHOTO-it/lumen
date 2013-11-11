using System;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Digiphoto.Lumen.Windows.Media.Effects;
using System.Windows.Media;
using Digiphoto.Lumen.UI.Adorners;
using System.Windows.Documents;
using Digiphoto.Lumen.UI.Util;
using System.Collections.Generic;
using System.Windows.Input;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging.Wic;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Imaging;
using System.Diagnostics;
using System.IO;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Core.Database;
using System.ComponentModel;
using Digiphoto.Lumen.UI.Mvvm.Event;
using Digiphoto.Lumen.Config;
using System.Text;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.UI.Main;
using System.Windows.Threading;



namespace Digiphoto.Lumen.UI.FotoRitocco {

	/// <summary>
	/// Interaction logic for FotoRitocco.xaml
	/// </summary>
	public partial class FotoRitocco : UserControlBase, IObserver<RitoccoPuntualeMsg> {

		FotoRitoccoViewModel _viewModel;

		public FotoRitocco() {
			
			InitializeComponent();

			_viewModel = (FotoRitoccoViewModel) this.DataContext;

			_viewModel.editorModeChangedEvent += cambiareModoEditor;


			// Mi sottoscrivo per ascoltare i messaggi di fotoritocco per ribindare i controlli.
			IObservable<RitoccoPuntualeMsg> observable = LumenApplication.Instance.bus.Observe<RitoccoPuntualeMsg>();
			observable.Subscribe( this );

//			_viewModel.PropertyChanged += propertyCambiata;

//			this.KeyDown += new KeyEventHandler( onFotoRitoccoUserControl_KeyDown );			

		}

		private void sliderLuminosita_ValueChanged( object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e ) {

			if( _viewModel != null )
				if( _viewModel.forseCambioEffettoCorrente( typeof( LuminositaContrastoEffect ) ) )
					bindaSliderLuminositaContrasto();
		}

		private void sliderContrasto_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {

			if( _viewModel != null )
				if( _viewModel.forseCambioEffettoCorrente( typeof( LuminositaContrastoEffect ) ) )
					bindaSliderLuminositaContrasto();
		}


		private void sliderRuota_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {

			if( _viewModel != null )
				if( _viewModel.forseCambioTrasformazioneCorrente( FotoRitoccoViewModel.TFXPOS_ROTATE ) )
					bindaSliderRuota();
		}


		private void sliderDominanti_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {

			if( _viewModel != null )
				if( _viewModel.forseCambioEffettoCorrente( typeof( DominantiEffect ) ) )
					bindaSlidersDominanti();
		}

		private void sliderZoom_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {

			if( _viewModel != null )
				if( _viewModel.forseCambioTrasformazioneCorrente( FotoRitoccoViewModel.TFXPOS_ZOOM ) )
					bindaSliderZoom();
		}

		private void sliderTrasla_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {
			if( _viewModel != null )
				if( _viewModel.forseCambioTrasformazioneCorrente( FotoRitoccoViewModel.TFXPOS_TRANSLATE ) ) {
					bindaSliderTrasla();
				}
		}


		private void bindaSlidersDominanti() {
			bindaSlidersDominanti( false );
		}

		/// <summary>
		/// Associo lo slider all'effetto corrente
		/// </summary>
		/// <param name="qualeRGB">Una stringa contenente : "R", "G", "B" </param>
		private void bindaSlidersDominanti( bool mantieniValori ) {

			if( mantieniValori && _viewModel.dominantiEffect == null ) {
				sliderDominanteRed.Value = 0;
				sliderDominanteGreen.Value = 0;
				sliderDominanteBlue.Value = 0;
				return;
			}

			double salvaValoreR = _viewModel.dominantiEffect.Red;
			double salvaValoreG = _viewModel.dominantiEffect.Green;
			double salvaValoreB = _viewModel.dominantiEffect.Blue;

			bindaSliderDominanteRGB( sliderDominanteRed,   DominantiEffect.RedProperty );
			bindaSliderDominanteRGB( sliderDominanteGreen, DominantiEffect.GreenProperty );
			bindaSliderDominanteRGB( sliderDominanteBlue,  DominantiEffect.BlueProperty );

			if( mantieniValori ) {
				_viewModel.dominantiEffect.Red = salvaValoreR;
				_viewModel.dominantiEffect.Green = salvaValoreG;
				_viewModel.dominantiEffect.Blue = salvaValoreB;
			}
		}

		private void bindaSliderDominanteRGB( Slider sliderSorgente, DependencyProperty prop ) {
			Binding binding = new Binding();
			binding.Source = sliderSorgente;
			binding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			binding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( _viewModel.dominantiEffect, prop, binding );
		}

		private void bindaSliderRuota() {
			bindaSliderRuota( false );
		}
		
		private void bindaSliderRuota( bool mantieniValore ) {

			if( FotoRitoccoViewModel.isTrasformazioneNulla( _viewModel.trasformazioneRotate ) ) {
				sliderRuota.Value = 0;  // per sicurezza riporto lo slider nella sua posizione neutra
				return;
			}

			double salvaValore = ((RotateTransform)_viewModel.trasformazioneRotate).Angle;

			Binding binding = new Binding();
			binding.Source = sliderRuota;
			binding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			binding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( _viewModel.trasformazioneRotate, RotateTransform.AngleProperty, binding );

			if( mantieniValore ) {
				((RotateTransform)_viewModel.trasformazioneRotate).Angle = salvaValore;
			}
		}

		private void bindaSliderZoom() {
			bindaSliderZoom( false );
		}

		private void bindaSliderZoom( bool mantieniValore ) {

			if( FotoRitoccoViewModel.isTrasformazioneNulla( _viewModel.trasformazioneZoom ) ) {
				sliderZoom.Value = 1;
				return;
			}

			double scaleX = ((ScaleTransform)_viewModel.trasformazioneZoom).ScaleX;
			double scaleY = ((ScaleTransform)_viewModel.trasformazioneZoom).ScaleY;

			Binding binding = new Binding();
			binding.Source = sliderZoom;
			binding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			binding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( _viewModel.trasformazioneZoom, ScaleTransform.ScaleXProperty, binding );
			BindingOperations.SetBinding( _viewModel.trasformazioneZoom, ScaleTransform.ScaleYProperty, binding );

			if( mantieniValore ) {
				((ScaleTransform)_viewModel.trasformazioneZoom).ScaleX = scaleX;
				((ScaleTransform)_viewModel.trasformazioneZoom).ScaleY = scaleY;
			}
		}

		private void bindaSliderTrasla() {
			bindaSliderTrasla( false );
		}

		private void bindaSliderTrasla( bool mantieniValore ) {
		
			if( FotoRitoccoViewModel.isTrasformazioneNulla( _viewModel.trasformazioneTranslate ) ) {
				sliderTraslaX.Value = 0;
				sliderTraslaY.Value = 0;
				return;
			}

			double salvaX = ((TranslateTransform)_viewModel.trasformazioneTranslate).X;
			double salvaY = ((TranslateTransform)_viewModel.trasformazioneTranslate).Y;

			Binding bindingSx = new Binding();
			bindingSx.Source = sliderTraslaX;
			bindingSx.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			bindingSx.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( _viewModel.trasformazioneTranslate, TranslateTransform.XProperty, bindingSx );

			Binding bindingSy = new Binding();
			bindingSy.Source = sliderTraslaY;
			bindingSy.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			bindingSy.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( _viewModel.trasformazioneTranslate, TranslateTransform.YProperty, bindingSy );

			// Setto anche le dimensioni di riferimento dell'area di modifica. Mi serviranno per riproporzionare sui provini o sulle risultanti

			if( mantieniValore ) {
				((TranslateTransform)_viewModel.trasformazioneTranslate).X = salvaX;
				((TranslateTransform)_viewModel.trasformazioneTranslate).Y = salvaY;
			}
		}

		/// <summary>
		/// Siccome gli slider sono due, ma l'effetto è uno solo, allora bindo sempre entrambi
		/// </summary>
		private void bindaSliderLuminositaContrasto() {
			bindaSliderLuminosita();
			bindaSliderContrasto();
		}

		private void bindaSliderContrasto() {
			bindaSliderContrasto( false );
		}

		/// <summary>
		/// Creo il binding tra la property dell'effetto e lo slider che lo muove
		/// </summary>
		private void bindaSliderContrasto( bool mantieniValore ) {

			if( mantieniValore && _viewModel.luminositaContrastoEffect == null ) {
				sliderContrasto.Value = 1;
				return;
			}

			double salvaValore = _viewModel.luminositaContrastoEffect.Contrast;

			Binding contrBinding = new Binding();
			contrBinding.Source = sliderContrasto;
			contrBinding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			contrBinding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( _viewModel.luminositaContrastoEffect, LuminositaContrastoEffect.ContrastProperty, contrBinding );

			if( mantieniValore )
				_viewModel.luminositaContrastoEffect.Contrast = salvaValore;
		}

		private void bindaSliderLuminosita() {
			bindaSliderLuminosita( false );
		}

		/// <summary>
		/// Creo il binding tra la property dell'effetto e lo slider che lo muove
		/// </summary>
		private void bindaSliderLuminosita( bool mantieniValore ) {

			if( _viewModel.luminositaContrastoEffect == null ) {
				sliderLuminosita.Value = 0;
				return;
			}

			double salvaValore = _viewModel.luminositaContrastoEffect.Brightness;

			Binding lumBinding = new Binding();
			lumBinding.Mode = BindingMode.TwoWay; // .TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			lumBinding.Source = sliderLuminosita;
			lumBinding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( _viewModel.luminositaContrastoEffect, LuminositaContrastoEffect.BrightnessProperty, lumBinding );

			if( mantieniValore )
				_viewModel.luminositaContrastoEffect.Brightness = salvaValore;
		}


/* RRR

				/// <summary>
				/// A discapito del nome, questa rappresenta l'unica immagine selezionata,
				/// su cui ho attivato un Selettore Adorner.
				/// </summary>
				public Image imageToCrop {

					get {

						if( itemsControlImmaginiInModifica == null || itemsControlImmaginiInModifica.Items.Count != 1 ) 
							return null;

						// Veder spiegazione qui:
						// http://msdn.microsoft.com/en-us/library/bb613579.aspx

						// Prendo il primo (e l'unico elemento)
						object myElement = itemsControlImmaginiInModifica.Items.GetItemAt( 0 );

						ContentPresenter contentPresenter = (ContentPresenter)itemsControlImmaginiInModifica.ItemContainerGenerator.ContainerFromItem( myElement );
						if( contentPresenter == null )
							return null;

						// Finding image from the DataTemplate that is set on that ContentPresenter
						DataTemplate myDataTemplate = contentPresenter.ContentTemplate;
						return (Image)myDataTemplate.FindName( "imageModTemplate", contentPresenter );
					}
				}

				private void toggleSelector_Checked( object sender, RoutedEventArgs e ) {

					if( _viewModel.attivareSelectorCommand.CanExecute( null ) )
						_viewModel.attivareSelectorCommand.Execute( imageToCrop );
					else
						toggleSelector.IsChecked = false;  // Rifiuto
				}

				private void toggleSelector_Unchecked( object sender, RoutedEventArgs e ) {
					_viewModel.attivareSelectorCommand.Execute( null );  // Qui vorrei spegnere
				}
		*/


		void cambiareModoEditor( object sender, EditorModeEventArgs args ) {

			switch( args.modalitaEdit ) {
				
				case ModalitaEdit.GestioneMaschere:

					primoPianoCanvasMask( false );
					initGestioneMaschere();
					break;

				case ModalitaEdit.FotoRitocco:

					listBoxImmaginiDaModificare.SetValue( MultiSelect.IsEnabledProperty, true );
					listBoxImmaginiDaModificare.SetValue( MultiSelect.MaxNumSelectedItemProperty, 3 );

					primoPianoCanvasMask( true );
					azzeraGestioneMaschere();
					break;
			}
		}


		private Fotografia firstFotoInCanvas = null;
		private void canvasMsk_Drop( object sender, DragEventArgs e ) {

			Fotografia foto = e.Data.GetData( typeof( Fotografia ) ) as Fotografia;
			if( foto != null ) {
				//Mi serve sapere quale è la prima foto nel canvas la uso per il clone.
				if (firstFotoInCanvas == null)
					firstFotoInCanvas = foto;	
				
				// Devo creare una image con la foto grande (il provino non basta più).
				Image imageFotina = new Image();

				// Metto un nome univoco al componente
				imageFotina.Name = "imageFotina" + Guid.NewGuid().ToString().Replace( '-', '_' );

				IImmagine immagine = AiutanteFoto.idrataImmagineGrande( foto );

				// per evitare che l'immagine sfori le dimensioni del video, la faccio sempre grande la metà della zona visibile.
				if( immagine.orientamento == Orientamento.Orizzontale )
					imageFotina.Width = imageMask.Width / 2;
				else
					imageFotina.Height = imageMask.Height / 2;

				BitmapSource bmpSrc = ((ImmagineWic)immagine).bitmapSource;

				imageFotina.BeginInit();
				imageFotina.Source = bmpSrc;
				imageFotina.EndInit();
				
				Point position = e.GetPosition( canvasMsk );
				Canvas.SetLeft( imageFotina, position.X );
				Canvas.SetTop( imageFotina, position.Y );
				canvasMsk.Children.Add( imageFotina );
				AddAdorner( imageFotina );

/* TODO : verificare se serve. Ho messo un breakpoint nell'event handler e non viene mai chiamato
				imageFotina.PreviewMouseDown += new MouseButtonEventHandler( imageFotina_PreviewMouseDown );

*/
				imageFotina.ContextMenu = (ContextMenu )this.Resources ["contextMenuImageFotina"];
				foreach( MenuItem item in imageFotina.ContextMenu.Items ) {
					if( item.Name == "menuItemBringToFront" ) {
						item.Click += menuItemBringToFront_Click;
					}
					// Console.Write( item );
				}

				portaInPrimoPianoFotina( imageFotina );

				primoPianoCanvasMask( true );
			}
		}

#if false
		private System.Windows.Controls.ContextMenu creaContextMenuFotina() {

			MenuItem m1, m2, m3, m4;

			ContextMenu _contextMenu = new ContextMenu();

			m1 = new MenuItem();

			m1.Header = "File";

			m2 = new MenuItem();

			m2.Header = "Save";

			m3 = new MenuItem();

			m3.Header = "SaveAs";

			m4 = new MenuItem();

			m4.Header = "Recent Files";

			_contextMenu.Items.Add( m1 );

			_contextMenu.Items.Add( m2 );

			_contextMenu.Items.Add( m3 );

			_contextMenu.Items.Add( m4 );

			return _contextMenu;
		}
#endif
		
		/// <summary>
		/// Elimina tutti gli Adornes da tutte le immagini che sono state aggiunte
		/// </summary>
		void rimuoviTutteLeManigliette() {

			// Rimuove tutti gli adorner (toglie le manigliette)
			foreach( UIElement element in canvasMsk.Children ) {
				AdornerLayer adornerlayer = AdornerLayer.GetAdornerLayer( element );
				var adorners = adornerlayer.GetAdorners( element );
				if( adorners != null ) {
					for( int i = adorners.Length - 1; i >= 0; i-- ) {
						adornerlayer.Remove( adorners [i] );
					}
				}
			}
		}

		/// <summary>
		///   true  = Porto avanti il canvas con la maschera in modo che si sovrapponga alla foto.
		///   
		///   false = Porto avanti il canvas che riceverà la foto (per permetter l'operazione di drop)
		/// </summary>
		/// <param name="avanti"></param>
		void primoPianoCanvasMask( bool avanti ) {

			if( avanti ) {

				// Porto davanti il canvas con la maschera in modo che si sovrapponga alla foto (tanto ha il buco trasparente)
				Grid.SetZIndex( canvasMskCopertura, 90 );
				Canvas.SetZIndex( canvasMsk, 10 );


			} else {
				// Porto davanti il canvas che riceverà la fotina (per permettere il drop)
				Canvas.SetZIndex( canvasMsk, 50 );
				Grid.SetZIndex( canvasMskCopertura, 10 );
			}

			Grid.SetZIndex( gridRitocco, 2 );

		}

		private void AddAdorner( UIElement element ) {
			AdornerLayer adornerlayer = AdornerLayer.GetAdornerLayer( element );
			if( adornerlayer.GetAdorners( element ) == null || adornerlayer.GetAdorners( element ).Length == 0 ) {
				ComposingAdorner adorner = new ComposingAdorner( element );
				adornerlayer.Add( adorner );
			}
		}

		private void menuItemBringToFront_Click( object sender, RoutedEventArgs e ) {

			MenuItem menuItem = sender as MenuItem;

			ContextMenu contextMenu = menuItem.Parent as ContextMenu;

			Image imageTarget = contextMenu.PlacementTarget as Image;

			if( imageTarget != null )
				portaInPrimoPianoFotina( imageTarget );
		}


		void imageFotina_PreviewMouseDown( object sender, MouseButtonEventArgs e ) {
			
			AddAdorner( (Image)sender );

			portaInPrimoPianoFotina( (Image)sender );

			e.Handled = true;
		}

		//Memorizzo il punto di ingresso
		private Point startPoint;

		private void listBoxImmaginiDaModificare_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			ListBoxItem container = sender as ListBoxItem;

			if (container != null)
			{
				// Store the mouse position
				startPoint = e.GetPosition(null);
			}

		}

		private void listBoxImmaginiDaModificare_MouseMove(object sender, MouseEventArgs e)
		{
			ListBoxItem container = sender as ListBoxItem;

			if( container != null ) {
				// Get the current mouse position
				Point mousePos = e.GetPosition(container);
				Vector diff = startPoint - mousePos;
 
				if (e.LeftButton == MouseButtonState.Pressed &&
					(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
					Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance) )
				{
					primoPianoCanvasMask( false );
					DragDrop.DoDragDrop( container, container.DataContext, DragDropEffects.Copy );
				}
			}
		}

/* RRR
		/// <summary>
		/// Questo mi serve per togliere dalla modifica una foto.
		/// </summary>
		private void itemsControlImmaginiInModifica_PreviewMouseDown( object sender, MouseButtonEventArgs e ) {

			if( e.OriginalSource is Image ) {
				Image image = e.OriginalSource as Image;
				DragDrop.DoDragDrop( image, image.DataContext, DragDropEffects.Move );
			}

		}
*/

		/// <summary>
		/// siccome a video devo lavorare con un canvas più piccolo che contenga la foto in modo 
		/// visibile all'utente,
		/// ora sono costretto a ricrearne un altro con le dimensioni reali della foto che voglio
		/// andare a comporre.
		/// 
		/// </summary>
		/// <param name="workCanvas">Il canvas usato nella schermata</param>
		/// <returns>il canvas con le dimensioni reali della foto grande da generare</returns>
		private Canvas trasformaCanvasDefinitivo() {

			BitmapSource bmpSource = (BitmapSource)imageMask.Source;

			double factorX = 96d / bmpSource.DpiX;
			double factorY = 96d / bmpSource.DpiY;
			//factorX = factorY = 1;
			Int32 newWidth = (int)((double)bmpSource.PixelWidth * factorX );
			Int32 newHeight = (int)((double)bmpSource.PixelHeight * factorY);

			Canvas c = new Canvas();
			c.Background = new SolidColorBrush( Colors.Transparent );
			c.Width = newWidth;
			c.Height = newHeight;
			c.HorizontalAlignment = HorizontalAlignment.Left;
			c.VerticalAlignment = VerticalAlignment.Top;
			
			SortedList<int,UIElement> listaOrdinata = new SortedList<int,UIElement>();
			foreach( UIElement uiElement in canvasMsk.Children ) {
				listaOrdinata.Add( Canvas.GetZIndex( uiElement ), uiElement );
			}

			var qq = listaOrdinata.OrderBy( f => f.Key ).Select( v => v.Value );

			foreach( Visual visual in qq ) {

				Image fotina = (Image)visual;

				WriteableBitmap wb = new WriteableBitmap( (BitmapSource)fotina.Source );

				BitmapSource bs = (BitmapSource)fotina.Source;
				Image fotona = new Image();
				fotona.HorizontalAlignment = HorizontalAlignment.Left;
				fotona.VerticalAlignment = VerticalAlignment.Top;
	
				fotona.BeginInit();

				fotona.Source = wb;

				fotona.EndInit();

				double fotinaFactorX = 96d / bs.DpiX;

				Rect rectFotina = new Rect( Canvas.GetLeft( fotina ), Canvas.GetTop( fotina ), fotina.ActualWidth, fotina.ActualHeight );
				
				Vector offset = VisualTreeHelper.GetOffset( imageMask );
				Rect rectFondo = new Rect( offset.X, offset.Y, imageMask.ActualWidth, imageMask.ActualHeight );
				
				Size sizeMaschera = new Size( bmpSource.PixelWidth, bmpSource.PixelHeight );


				Rect newRect = Geometrie.proporziona( rectFotina, rectFondo, sizeMaschera );
				
				fotona.Width = newRect.Width * factorX;
				fotona.Height = newRect.Height * factorY;
				fotona.Stretch = Stretch.Uniform;
				

				c.Children.Add( fotona );

				double test = Canvas.GetLeft( fotona );
				int appo = Canvas.GetZIndex( fotina );
				Canvas.SetZIndex( fotona, appo );

				// Imposto la posizione della foto all'interno del canvas della cornice.
				Canvas.SetLeft( fotona, newRect.Left * factorX );
				Canvas.SetTop( fotona, newRect.Top * factorY );
//				Canvas.SetLeft( fotona, 0 );
//				Canvas.SetTop( fotona, 0 );



				// ----------------------------------------------
				// ---   ora mi occupo delle trasformazioni   ---
				// ----------------------------------------------
				#region trasformazioni
				if( fotina.RenderTransform is TransformGroup ) {

					TransformGroup newTg = new TransformGroup();

					TransformGroup tg = (TransformGroup)fotina.RenderTransform;
					foreach( Transform tx in tg.Children ) {

						Debug.WriteLine( "Trasformazione = " + tx.ToString() );

						if( tx is RotateTransform ) {
							RotateTransform rtx = (RotateTransform)tx;
							// L'angolo rimane uguale che tanto è in gradi
							RotateTransform newTx = rtx.Clone();
							// ricalcolo solo il punto centrale
							newTx.CenterX = rtx.CenterX * fotona.Width / fotina.ActualWidth;
							newTx.CenterY = rtx.CenterY * fotona.Height / fotina.ActualHeight;
							newTg.Children.Add( newTx );
						} else if( tx is TranslateTransform ) {
							TranslateTransform ttx = (TranslateTransform)tx;
							// Devo riproporzionare le misure.
							//  x1 : w1 = ? : w2
							//  ? = x1 * w2 / w1
							double newX = ttx.X * fotona.Width / fotina.ActualWidth;
							double newY = ttx.Y * fotona.Height / fotina.ActualHeight;
							TranslateTransform newTx = new TranslateTransform( newX, newY );
							newTg.Children.Add( newTx );
						} else if( tx is MatrixTransform ) {
							// Questo è il Flip
							MatrixTransform mtx = (MatrixTransform)tx;
							MatrixTransform newTx = (MatrixTransform)tx.Clone();

							Matrix mMatrix = new Matrix();
							mMatrix.Scale( -1.0, 1.0 );
							mMatrix.OffsetX = mtx.Value.OffsetX * fotona.Width / fotina.ActualWidth;
							newTx.Matrix = mMatrix;

							newTg.Children.Add( newTx );
						} else if( tx is ScaleTransform ) {
							ScaleTransform stx = (ScaleTransform)tx;

							// La scala rimane uguale perché è un fattore moltiplicativo.
							ScaleTransform newTx = stx.Clone();
							// ricalcolo solo il punto centrale
							newTx.CenterX = stx.CenterX * fotona.Width / fotina.ActualWidth;
							newTx.CenterY = stx.CenterY * fotona.Height / fotina.ActualHeight;
							newTg.Children.Add( newTx );
						}
					}

					fotona.RenderTransform = newTg;
				}
				#endregion trasformazioni
			}

			// per concludere, aggiungo anche la maschera che deve ricoprire tutto.
			Image maschera = new Image();
			maschera.BeginInit();
			maschera.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			maschera.VerticalAlignment = System.Windows.VerticalAlignment.Top;
			maschera.Source = imageMask.Source.Clone();
			maschera.EndInit();
			maschera.Width = c.Width;
			maschera.Height = c.Height;
			c.Children.Add( maschera );

			// Non so se serve, ma dovrebbe provocare un ricalcolo delle posizioni dei componenti all'interno del canvas.
			c.InvalidateMeasure();
			c.InvalidateArrange();
			return c;
		}

		public void salvaCanvasSuFile( Canvas canvas, string nomeFile ) {

			BitmapSource bmpSource = (BitmapSource)imageMask.Source;

			Int32 newWidth = (int)((double)bmpSource.PixelWidth );
			Int32 newHeight = (int)((double)bmpSource.PixelHeight );

			RenderTargetBitmap rtb = new RenderTargetBitmap( newWidth, newHeight, bmpSource.DpiX, bmpSource.DpiY, PixelFormats.Pbgra32 );

			// Prima renderizzo le fotine ...
			foreach( Visual visual in canvas.Children ) {
				Image fotina = (Image)visual;
				rtb.Render( visual );
			}

			// ... poi la maschera per ultima che copre tutto.
			//rtb.Render( imgMaschera );

			if( rtb.CanFreeze )
				rtb.Freeze();

			BitmapFrame frame = BitmapFrame.Create( rtb );
			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add( frame );

			// ----- scrivo su disco
			using( FileStream fs = new FileStream( nomeFile, FileMode.Create ) ) {
				encoder.Save( fs );
				fs.Flush();
			}
		}

		private void salvareMascheraButton_Click( object sender, RoutedEventArgs e ) {


			// Questo controllo purtroppo non posso farlo a monte. 
			// Sarebbe meglio evitare di accendere il pulsante, ma non riesco a farlo facilmente perché la property "firstFotoInCanvas" non è nel ViewModel ma è qui.
			if( firstFotoInCanvas == null ) {
				this.ShowError( "Trascinare almeno una foto nella cornice\nprima di salvare", "Errore", null );
				return;
			}

			Canvas canvasDefinitivo = trasformaCanvasDefinitivo();

			// salvaCanvasSuFile( canvasDefinitivo, @"c:\temp\definitivo.jpg" );

			// Non ho capito perchè, ma se non assegno questo canvas ad una finestra, 
			// allora quando lo andrò a salvare su disco, l'immagine apparirà tutta nera.
			// boh!...   Con questo trucco tutto si sistema.
			Window w = new Window();
			try {

				w.Content = canvasDefinitivo;
				bool voglioDebuggare = false;
				if( ! voglioDebuggare ) {
					w.Visibility = Visibility.Hidden;
					w.Show();
				} else {
					// per debug si può anche visualizzare il risultato
					w.Visibility = Visibility.Visible;
					w.ShowDialog();
				}

				RenderTargetBitmap bitmapIncorniciata = componiBitmapDaMaschera( canvasDefinitivo );

				_viewModel.salvareImmagineIncorniciata(firstFotoInCanvas ,bitmapIncorniciata );

			} finally {
				w.Close();
			}

			rimuoviTutteLeManigliette();

			firstFotoInCanvas = null;
		}


		private RenderTargetBitmap componiBitmapDaMaschera( Canvas canvas ) {

			BitmapSource bmpSource = (BitmapSource)imageMask.Source;

			Int32 newWidth = bmpSource.PixelWidth;
			Int32 newHeight = bmpSource.PixelHeight;

			RenderTargetBitmap rtb = new RenderTargetBitmap( newWidth, newHeight, bmpSource.DpiX, bmpSource.DpiY, PixelFormats.Pbgra32 );

			// Renderizzo tutte le images che contengono sia le fotine che la maschera.
			foreach( Visual visual in canvas.Children ) {
				Image fotina = (Image)visual;
				rtb.Render( visual );
			}

			// ... poi la maschera per ultima che copre tutto.
			// rtb.Render( imageMask );

			if( rtb.CanFreeze )
				rtb.Freeze();

			return rtb;
		}

		void initGestioneMaschere() {
		}

		/// <summary>
		/// Quando rifiuto la maschera, devo togliere dal video eventuali componenti grafici rimasti in mezzo ai piedi.
		/// </summary>
		void azzeraGestioneMaschere() {
			// Elimino le foto che sono state droppate sul canvas
			canvasMsk.Children.Clear();

			//Riazzero la prima foto in maschera
			firstFotoInCanvas = null;
		}

		private void listBoxImmaginiDaModificare_Drop( object sender, DragEventArgs e ) {

			var s1 = e.Source;
			var s2 = e.OriginalSource;

			if( e.Effects == DragDropEffects.Move ) {

				// Ok sto spostando la foto dal canvas di modifica alla lista di attesa.
				using( new UnitOfWorkScope() ) {

					var oo = e.Data.GetData( typeof( Fotografia ) );

					if( oo != null ) {
						Fotografia daTogliere = oo as Fotografia;
						_viewModel.rifiutareCorrezioni( daTogliere, true );
					}
				}

			} else {

				// Sto cliccando sulla foto nella lista di attesa. Non so perché mi 
				// solleva questo evento.
// DACANC:				_viewModel.forzaRefreshStato();

			}
		}


		private void listBoxImmaginiDaModificare_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{

			ListBoxItem listBoxItem = SelectItemOnRightClick( e );
			if( listBoxItem != null ) {
				((FotoRitoccoViewModel)viewModelBase).selettoreAzioniRapideViewModel.ultimaFotoSelezionata = (Fotografia)listBoxItem.Content;
				// Questo mi evita di selezionare la foto quando clicco con il destro.
				e.Handled = true;
			}
		}

		private ListBoxItem SelectItemOnRightClick(System.Windows.Input.MouseButtonEventArgs e)
		{
			Point clickPoint = e.GetPosition( listBoxImmaginiDaModificare );
			object element = listBoxImmaginiDaModificare.InputHitTest(clickPoint);
			ListBoxItem clickedListBoxItem = null;
			if( element != null )
			{
				clickedListBoxItem = GetVisualParent<ListBoxItem>( element );
				if( clickedListBoxItem != null ) 
				{					
/*
					Fotografia f = (Fotografia)clickedListBoxItem.Content;
					if( ! _viewModel.fotografieDaModificareCW.SelectedItems.Contains( f ) )
						_viewModel.fotografieDaModificareCW.SelectedItems.Add( f );
 */
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

		private void selectionFailed(object sender, SelectionFailedEventArgs e)
		{
			StringBuilder msg = new StringBuilder();
			msg.AppendFormat("Non puoi selezionare più di {0} foto", e.maxNumSelected);

			_viewModel.trayIconProvider.showInfo("AVVISO", msg.ToString(), 1500);
		}

		/// <summary>
		/// Dato il nome del componente, ricerco l'immaginetta e la rendo attiva.
		/// </summary>
		/// <param name="name"></param>
		void selezionaFotina( string name ) {

			foreach( UIElement child in canvasMsk.Children ) {
				
				if( child is Image ) {
					
					Image childImage = child as Image;
					if( childImage.Name == name )
						childImage.Tag = "!SELEZ!";  // occhio questo valore convenzionale è usato in un trigger di style.
					else
						childImage.Tag = null;
				}
			}
		}

		/// <summary>
		/// Mi dice quante fotine sono state buttate sulla maschera
		/// </summary>
		int quanteFotineSulTavolo {
			get {
				return canvasMsk.Children.OfType<Image>().Count();
			}
		}

		/// <summary>
		/// Ritorna l'immagine che rappresenta la fotina selezionata
		/// </summary>
		Image fotinaSelezionata {
			get {
				return canvasMsk.Children.OfType<Image>().SingleOrDefault( i => i.Tag.Equals( "!SELEZ!" ) );
			}
		}


		/// <summary>
		/// Gestisco la proprietà Zindex sulle fotine per stabilire l'ordine di sovrapposizione.
		///  la foto più Front avrà un valore di 80 e le altre a scendere di uno.
		/// </summary>
		/// <param name="imageFotina"></param>
		void portaInPrimoPianoFotina( Image imageFotina ) {

			selezionaFotina( imageFotina.Name );

			foreach( Visual visual in canvasMsk.Children ) {
				if( visual is Image ) {
					Image fotina = (Image)visual;
					if( fotina == imageFotina )
						Canvas.SetZIndex( fotina, 90 );
					else {
						int appo = Canvas.GetZIndex( fotina );
						Canvas.SetZIndex( fotina, appo - 1 );
					}
				}
			}


		}


		/// <summary>
		/// Questa è la property che mi dice quale fotina è quella selezionata (attiva).
		/// La fotina attiva mi serve per esempio per zommarla con la rotella.
		/// </summary>
		public string nomeFotinaSelezionata {
			get;
			private set;
		}



		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( RitoccoPuntualeMsg rpMsg ) {

			// TODO : ma serve ????

			// Questi sono effetti
			bindaSliderLuminosita( true );
			bindaSliderContrasto( true );
			bindaSlidersDominanti( true );

			// Queste sono trasformazioni
			bindaSliderRuota( true );
			bindaSliderZoom( true );
			bindaSliderTrasla( true );

			// Provo a dare il fuoco al mio usercontrol ma nel thread della GUI
			Action focusAction = () => fotoRitoccoUserControl.Focus();
			this.Dispatcher.BeginInvoke( focusAction, DispatcherPriority.ApplicationIdle );


		}

		private void imageRitoccata_MouseWheel( object sender, MouseWheelEventArgs e ) {
			if( Keyboard.IsKeyDown( Key.LeftCtrl ) ) {
				// Rotazione
				double angolo = e.Delta > 0 ? sliderRuota.SmallChange : sliderRuota.SmallChange * (-1);
				sliderRuota.Value += angolo;
			} else {
				// Zoom
				double zoom = e.Delta > 0 ? sliderZoom.SmallChange : sliderZoom.SmallChange * (-1);
				sliderZoom.Value += zoom;
			}
		}

		// Serve per gestire il drag della foto
		private Point mouseClick;
		double posizioneX;
		double posizioneY;
		private void imageRitoccata_MouseDown( object sender, MouseButtonEventArgs e ) {

			mouseClick = e.GetPosition( null );

			// Questo mi permette di istanziare la trasformazione di traslazione
			sliderTrasla_ValueChanged( sender, null );

			double offsetX = 0;
			double offsetY = 0;
			// paranoia: dovrebbe sempre esistere. Però...
			TranslateTransform tt = (TranslateTransform) _viewModel.trasformazioneTranslate;
			if( tt != null ) {
				offsetX = tt.X;
				offsetY = tt.Y;
			}

			posizioneX = mouseClick.X - offsetX;
			posizioneY = mouseClick.Y - offsetY;

			((Image)sender).CaptureMouse();
		}

		private void imageRitoccata_MouseMove( object sender, MouseEventArgs e ) {
			if( ((Image)sender).IsMouseCaptured ) {

				Point mouseCurrent = e.GetPosition( null );

				double left = mouseCurrent.X - posizioneX;
				double top = mouseCurrent.Y - posizioneY;

				TransformGroup qq = imageRitoccata.RenderTransform as TransformGroup;

				if( qq != null ) {
					//	TranslateTransform tt = (TranslateTransform)qq.Children.First( c => c is TranslateTransform );
					sliderTraslaX.Value = left;
					sliderTraslaY.Value = top;
				} else
					System.Diagnostics.Debugger.Break(); // Strano che sia nullo. Come mai ?
			}
		}

		private void imageRitoccata_MouseUp( object sender, MouseButtonEventArgs e ) {
			((Image)sender).ReleaseMouseCapture();
		}

		/// <summary>
		/// Gestisco il tasto 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void onFotoRitoccoUserControl_KeyDown( object sender, KeyEventArgs e ) {

			if( e.Key == Key.Space ) {

				// Console.Beep( 440, 300 );  // Segnale per sapere che ha preso il tasto.

				if( _viewModel.modalitaEdit == ModalitaEdit.GestioneMaschere && _viewModel.possoSalvareMaschera ) {
					salvareMascheraButton_Click( null, null );
				}

				if( _viewModel.modalitaEdit == ModalitaEdit.FotoRitocco && _viewModel.applicareCorrezioniCommand.CanExecute( null ) ) {
					_viewModel.applicareCorrezioniCommand.Execute( null );
				}
			}

			if( e.Key == Key.Escape ) {

				if( _viewModel.modalitaEdit == ModalitaEdit.FotoRitocco && _viewModel.rifiutareCorrezioniCommand.CanExecute( null ) ) {
					_viewModel.rifiutareCorrezioniCommand.Execute( null );
				}
				
			}


		}

	}
}
