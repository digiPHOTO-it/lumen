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
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Core.Database;
using System.ComponentModel;



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

			_viewModel.PropertyChanged += propertyCambiata;
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

		/// <summary>
		/// Quando viene modificata una maschera, chiudo l'expander delle maschere
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="pcea"></param>
		void propertyCambiata( object sender, PropertyChangedEventArgs pcea ) {
			if( pcea.PropertyName == "mascheraAttiva" ) {
				if( _viewModel.mascheraAttiva != null )
					expanderMaschere.IsExpanded = false;
			}
		}

		void cambiareModoEditor( object sender, EditorModeEventArgs args ) {

			if( args.modalitaEdit == ModalitaEdit.GestioneMaschere ) {
				primoPianoCanvasMask( false );
				expanderMaschere.IsExpanded = false;  // chiudo l'expander per fare spazio
			} else {
				primoPianoCanvasMask( true );

				azzeraGestioneMaschere();
			}
		}

		private void canvasMsk_Drop( object sender, DragEventArgs e ) {

			Fotografia foto = e.Data.GetData( typeof( Fotografia ) ) as Fotografia;
			if( foto != null ) {
				
				// Devo creare una image con la foto grande (il provino non basta più).
				Image image = new Image();

				IImmagine immagine = AiutanteFoto.idrataImmagineGrande( foto );

				// per evitare che l'immagine sfori le dimensioni del video, la faccio sempre grande la metà della zona visibile.
				if( immagine.orientamento == Orientamento.Orizzontale )
					image.Width = imageMask.Width / 2;
				else
					image.Height = imageMask.Height / 2;

				BitmapSource bmpSrc = ((ImmagineWic)immagine).bitmapSource;

				image.BeginInit();
				image.Source = bmpSrc;
				image.EndInit();
				
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
		/// Questo mi serve per togliere dalla modifica una foto.
		/// </summary>
		private void itemsControlImmaginiInModifica_PreviewMouseDown( object sender, MouseButtonEventArgs e ) {

			if( e.OriginalSource is Image ) {
				Image image = e.OriginalSource as Image;
				DragDrop.DoDragDrop( image, image.DataContext, DragDropEffects.Move );
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
			


			foreach( Visual visual in canvasMsk.Children ) {

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
				Size sizeFondo = new Size( imageMask.ActualWidth, imageMask.ActualHeight );
				Size sizeMaschera = new Size( bmpSource.PixelWidth, bmpSource.PixelHeight );

				Rect newRect = Geometrie.proporziona( rectFotina, sizeFondo, sizeMaschera );
				fotona.Width = newRect.Width * factorX;
				fotona.Height = newRect.Height * factorY;
				fotona.Stretch = Stretch.Uniform;

				c.Children.Add( fotona );

				double test = Canvas.GetLeft( fotona );

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

				_viewModel.salvareImmagineIncorniciata( bitmapIncorniciata );

			} finally {
				w.Close();
			}

			rimuoviTutteLeManigliette();
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

		/// <summary>
		/// Quando rifiuto la maschera, devo togliere dal video eventuali componenti grafici rimasti in mezzo ai piedi.
		/// </summary>
		void azzeraGestioneMaschere() {
			// Elimino le foto che sono state droppate sul canvas
			canvasMsk.Children.Clear();
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
	}
}
