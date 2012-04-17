using System;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Data;
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



namespace Digiphoto.Lumen.UI.FotoRitocco {

	/// <summary>
	/// Interaction logic for FotoRitocco.xaml
	/// </summary>
	public partial class FotoRitocco : UserControlBase {

		FotoRitoccoViewModel _viewModel;

		public FotoRitocco() {
			
			InitializeComponent();

			_viewModel = (FotoRitoccoViewModel) this.DataContext;

			_viewModel.editorModeChangedEvent += cambiareModoEditor;
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
				if( _viewModel.forseCambioTrasformazioneCorrente( typeof( RotateTransform ) ) )
					bindaSliderRuota();
		}

		private void bindaSliderRuota() {

			// Bindings con i componenti per i parametri
			Binding binding = new Binding();
			binding.Source = sliderRuota;
			binding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			binding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( _viewModel.trasformazioneCorrente, RotateTransform.AngleProperty, binding );
		}

		/// <summary>
		/// Siccome gli slider sono due, ma l'effetto è uno solo, allora bindo sempre entrambi
		/// </summary>
		private void bindaSliderLuminositaContrasto() {
			bindaSliderLuminosita();
			bindaSliderContrasto();
		}

		/// <summary>
		/// Creo il binding tra la property dell'effetto e lo slider che lo muove
		/// </summary>
		private void bindaSliderContrasto() {
			Binding contrBinding = new Binding();
			contrBinding.Source = sliderContrasto;
			contrBinding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			contrBinding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( _viewModel.luminositaContrastoEffect, LuminositaContrastoEffect.ContrastProperty, contrBinding );
		}

		/// <summary>
		/// Creo il binding tra la property dell'effetto e lo slider che lo muove
		/// </summary>
		private void bindaSliderLuminosita() {
			Binding lumBinding = new Binding();
			lumBinding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			lumBinding.Source = sliderLuminosita;
			lumBinding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( _viewModel.luminositaContrastoEffect, LuminositaContrastoEffect.BrightnessProperty, lumBinding );
		}

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

		void cambiareModoEditor( object sender, EditorModeEventArgs args ) {
			if( args.modalitaEdit == ModalitaEdit.GestioneMaschere ) {
				primoPianoCanvasMask( false );
				Console.Write( "STOP" );
			} else {
				primoPianoCanvasMask( true );
			}
		}

		private void canvasMsk_Drop( object sender, DragEventArgs e ) {

			Fotografia foto = e.Data.GetData( typeof( Fotografia ) ) as Fotografia;
			if( foto != null ) {
				Image image = new Image();
				image.Width = foto.imgProvino.ww;
				image.Height = foto.imgProvino.hh;

				BitmapSource bmpSrc = ((ImmagineWic)foto.imgProvino).bitmapSource;

				WriteableBitmap wb = new WriteableBitmap(bmpSrc);

//				image.BeginInit();
				image.Source = wb;
//				image.EndInit();
				
				Point position = e.GetPosition( canvasMsk );
				Canvas.SetLeft( image, position.X );
				Canvas.SetTop( image, position.Y );
				canvasMsk.Children.Add( image );
				AddAdorner( image );
				image.PreviewMouseDown += new MouseButtonEventHandler( image_PreviewMouseDown );

				primoPianoCanvasMask( true );
			}
		}

		private void canvasMsk_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e ) {
			// rimuoviTutteLeManigliette();
		}

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
				Canvas.SetZIndex( canvasMskCopertura, 99 );
				Canvas.SetZIndex( canvasMsk, 10 );
			} else {
				// Porto davanti il canvas che riceverà la fotina (per permettere il drop)
				Canvas.SetZIndex( canvasMsk, 99 );
				Canvas.SetZIndex( canvasMskCopertura, 10 );
			}
		}

		private void AddAdorner( UIElement element ) {
			AdornerLayer adornerlayer = AdornerLayer.GetAdornerLayer( element );
			if( adornerlayer.GetAdorners( element ) == null || adornerlayer.GetAdorners( element ).Length == 0 ) {
				ComposingAdorner adorner = new ComposingAdorner( element );
				adornerlayer.Add( adorner );
			}
		}

		void image_PreviewMouseDown( object sender, MouseButtonEventArgs e ) {
			AddAdorner( (Image)sender );
			e.Handled = true;
		}

		private void listBoxImmaginiDaModificare_PreviewMouseDown( object sender, MouseButtonEventArgs e ) {
			ListBoxItem container = sender as ListBoxItem;
			if( container != null ) {
				primoPianoCanvasMask( false );
				DragDrop.DoDragDrop( container, container.DataContext, DragDropEffects.Copy );
			}
		}

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

			BitmapSource bmpSource = (BitmapSource)imageMsk.Source;

			double factorX = 96d / bmpSource.DpiX;
			double factorY = 96d / bmpSource.DpiY;
			Int32 newWidth = (int)((double)bmpSource.PixelWidth * factorX );
			Int32 newHeight = (int)((double)bmpSource.PixelHeight * factorY);

			Canvas c = new Canvas();
			c.Background = new SolidColorBrush( Colors.Transparent );

			foreach( Visual visual in canvasMsk.Children ) {

				Image fotina = (Image)visual;
				
				Image fotona = new Image();
//				fotona.BeginInit();
				fotona.Source = fotina.Source.Clone();  // Clono l'immagine iniziale della foto
				// fotona.EndInit();

				Rect rectFotina = new Rect( Canvas.GetLeft( fotina ), Canvas.GetTop( fotina ), fotina.ActualWidth, fotina.ActualHeight );
				Size sizeFondo = new Size( imageMsk.ActualWidth, imageMsk.ActualHeight );
				Size sizeMaschera = new Size( newWidth, newHeight );

				Rect newRect = Geometrie.proporziona( rectFotina, sizeFondo, sizeMaschera );

				fotona.Width = newRect.Width;
				fotona.Height = newRect.Height;

				c.Children.Add( fotona );

				// Imposto la posizione della foto all'interno del canvas della cornice.
				Canvas.SetLeft( fotona, newRect.Left );
				Canvas.SetTop( fotona, newRect.Top );


				// ----------------------------------------------
				// ---   ora mi occupo delle trasformazioni   ---
				// ----------------------------------------------

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
			}

			// per concludere, aggiungo anche la maschera che deve ricoprire tutto.
			Image maschera = new Image();
			maschera.Source = imageMsk.Source.Clone();
			maschera.Width = c.Width;
			maschera.Height = c.Height;
			c.Children.Add( maschera );

			// Non so se serve, ma dovrebbe provocare un ricalcolo delle posizioni dei componenti all'interno del canvas.
			c.InvalidateMeasure();
			c.InvalidateArrange();
			return c;
		}

		private void salvareCorrezioniButton_Click( object sender, RoutedEventArgs e ) {
			
			Canvas canvasDefinitivo = trasformaCanvasDefinitivo();

			RenderTargetBitmap bitmapIncorniciata = componiBitmapDaMaschera( canvasDefinitivo );

			_viewModel.salvareImmagineIcorniciata( bitmapIncorniciata );
		}

		private RenderTargetBitmap componiBitmapDaMaschera( Canvas canvas ) {

			BitmapSource bmpSource = (BitmapSource)imageMsk.Source;

			Int32 newWidth = bmpSource.PixelWidth;
			Int32 newHeight = bmpSource.PixelHeight;

			RenderTargetBitmap rtb = new RenderTargetBitmap( newWidth, newHeight, bmpSource.DpiX, bmpSource.DpiY, PixelFormats.Pbgra32 );

			// Prima renderizzo le fotine ...
			foreach( Visual visual in canvas.Children ) {
				Image fotina = (Image)visual;
				rtb.Render( visual );
			}

			// Prima renderizzo le fotine ...
			foreach( Visual visual in canvas.Children ) {
				Image fotina = (Image)visual;
				rtb.Render( visual );
			}

			// ... poi la maschera per ultima che copre tutto.
			//rtb.Render( imgMaschera );

			if( rtb.CanFreeze )
				rtb.Freeze();

			return rtb;
		}

	}
}
