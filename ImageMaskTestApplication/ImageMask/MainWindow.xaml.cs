using System;
using System.Collections.Generic;
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
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Digiphoto.Lumen.Imaging;

namespace ImageMask {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		
		public MainWindow() {
			InitializeComponent();
		}


		private void listBoxMaschere_Loaded( object sender, RoutedEventArgs e ) {
			listBoxMaschere.ItemsSource = new List<ImageInfo>() {
				new ImageInfo(){Height=100, Width=double.NaN, Uri=new Uri("/maschere/mask1.png",UriKind.Relative)},
				new ImageInfo(){Height=100, Width=double.NaN,Uri=new Uri("/maschere/mask2.png",UriKind.Relative)},
			};
		}


		private void listBoxMaschere_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ListBox lb = (ListBox)sender;
			ImageInfo ii = (ImageInfo)lb.SelectedItem;
			BitmapImage msk = new BitmapImage(ii.Uri);
			imgMaschera.Source = msk;
		}

		private void listBoxImmagini_Loaded( object sender, RoutedEventArgs e ) {
			listBoxImmagini.ItemsSource = new List<ImageInfo>() {
				new ImageInfo(){Height=100, Width=double.NaN,Uri=new Uri("immagini/Desert.jpg",UriKind.Relative)},
				new ImageInfo(){Height=100, Width=double.NaN,Uri=new Uri("immagini/Koala.jpg",UriKind.Relative)},
				new ImageInfo(){Height=100, Width=double.NaN,Uri=new Uri("immagini/Penguins.jpg",UriKind.Relative)},
				new ImageInfo(){Height=100, Width=double.NaN,Uri=new Uri("immagini/modella.jpg",UriKind.Relative)}
			};
		}

		private void listBoxImmagini_PreviewMouseDown( object sender, MouseButtonEventArgs e ) {
			ListBoxItem container = sender as ListBoxItem;
			if( container != null ) {
				primoPiano( false );
				DragDrop.DoDragDrop( container, container.DataContext, DragDropEffects.Copy );
			}
		}

		private void AddAdorner( UIElement element ) {
			AdornerLayer adornerlayer = AdornerLayer.GetAdornerLayer( element );
			if( adornerlayer.GetAdorners( element ) == null || adornerlayer.GetAdorners( element ).Length == 0 ) {
				MyImageAdorner adorner = new MyImageAdorner( element );
				adornerlayer.Add( adorner );
			}
		}

		void image_PreviewMouseDown( object sender, MouseButtonEventArgs e ) {
			AddAdorner( (Image)sender );
			e.Handled = true;
		}

		private void MyCanvas_Drop( object sender, DragEventArgs e ) {
			ImageInfo imageInfo = e.Data.GetData( typeof( ImageInfo ) ) as ImageInfo;
			if( imageInfo != null ) {
				Image image = new Image();
				image.Height = imageInfo.Height;
				image.Width = imageInfo.Width;
				image.Source = new BitmapImage( imageInfo.Uri );
				Point position = e.GetPosition( MyCanvas );
				Canvas.SetLeft( image, position.X );
				Canvas.SetTop( image, position.Y );
				MyCanvas.Children.Add( image );
				AddAdorner( image );
				image.PreviewMouseDown += new MouseButtonEventHandler( image_PreviewMouseDown );

				primoPiano( true );
			}
		}

		private void MyCanvas_MouseDown( object sender, MouseButtonEventArgs e ) {
			rimuoviTutteLeManigliette();
		}

		/// <summary>
		/// Elimina tutti gli Adornes da tutte le immagini che sono state aggiunte
		/// </summary>
		void rimuoviTutteLeManigliette() {

			// Rimuove tutti gli adorner (toglie le manigliette)
			foreach( UIElement element in MyCanvas.Children ) {
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
		void primoPiano( bool avanti ) {

			if( avanti ) {
				// Porto davanti il canvas con la maschera in modo che si sovrapponga alla foto (tanto ha il buco trasparente)
				Canvas.SetZIndex( fondoCanvas, 99 );
				Canvas.SetZIndex( MyCanvas, 10 );
			} else {
				// Porto davanti il canvas che riceverà la foto (per permettere il drop)
				Canvas.SetZIndex( MyCanvas, 99 );
				Canvas.SetZIndex( fondoCanvas, 10 );
			}
		}

		private void buttonZPiu_Click( object sender, RoutedEventArgs e ) {
			int zIndex = 1;
			foreach( UIElement element in MyCanvas.Children )
				Canvas.SetZIndex( element, ++zIndex );
		}

		private void buttonZMeno_Click( object sender, RoutedEventArgs e ) {
			int zIndex = 99;
			foreach( UIElement element in MyCanvas.Children )
				Canvas.SetZIndex( element, --zIndex );
		}

		private void buttonSalva_Click( object sender, RoutedEventArgs e ) {

			salvaCanvasSuFile( MyCanvas, @"c:\temp\orig.png" );

			Canvas destCanvas = trasformaCanvasDefinitivo();

			Window w = new Window();
			w.Content = destCanvas;
			w.ShowDialog();

			salvaCanvasSuFile( destCanvas, @"c:\temp\dest.png" );

		}

		void salvaCanvasSuFile( Canvas canvas, string nomeFile ) {

			BitmapSource bmpSource = (BitmapSource)imgMaschera.Source;

			Int32 newWidth = bmpSource.PixelWidth;
			Int32 newHeight = bmpSource.PixelHeight;

			RenderTargetBitmap rtb = new RenderTargetBitmap( newWidth, newHeight, bmpSource.DpiX, bmpSource.DpiY, PixelFormats.Pbgra32 );

			// Prima renderizzo le fotine ...
            foreach (Visual visual in canvas.Children) {
				Image fotina = (Image)visual;
				rtb.Render(visual);
			}

			// ... poi la maschera per ultima che copre tutto.
			//rtb.Render( imgMaschera );

            if (rtb.CanFreeze) 
				rtb.Freeze();

            BitmapFrame frame = BitmapFrame.Create(rtb);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(frame);

			// ----- scrivo su disco
            using (FileStream fs = new FileStream(nomeFile, FileMode.Create)) {
                encoder.Save(fs);
                fs.Flush();
            }
        }

		private Canvas trasformaCanvasDefinitivo() {

			BitmapSource bmpSource = (BitmapSource)imgMaschera.Source;

			Int32 newWidth  = (int) ((double)bmpSource.PixelWidth * 96d / bmpSource.DpiX);
			Int32 newHeight = (int) ((double)bmpSource.PixelHeight * 96d / bmpSource.DpiY);

			Canvas c = new Canvas();
			c.Background = new SolidColorBrush( Colors.Transparent );			

			foreach( Visual visual in MyCanvas.Children ) {

				Image fotina = (Image)visual;
				Image fotona = new Image();
				fotona.Source = fotina.Source.Clone();  // Clono l'immagine iniziale della foto

				Rect rectFotina = new Rect( Canvas.GetLeft( fotina ), Canvas.GetTop( fotina ), fotina.ActualWidth, fotina.ActualHeight );
				Size sizeFondo = new Size( imgMaschera.ActualWidth, imgMaschera.ActualHeight );
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
			maschera.Source = imgMaschera.Source.Clone();
			maschera.Width = c.Width;
			maschera.Height = c.Height;
			c.Children.Add( maschera );

			// Non so se serve, ma dovrebbe provocare un ricalcolo delle posizioni dei componenti all'interno del canvas.
			c.InvalidateMeasure();
			c.InvalidateArrange();
			return c;
		}

		private void buttonReset_Click( object sender, RoutedEventArgs e ) {
			MyCanvas.Children.Clear();
			imgMaschera.Source = null;
		}


	}
}
