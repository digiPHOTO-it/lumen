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
using Digiphoto.Lumen.Imaging.Wic.Correzioni;
using System.Windows.Shapes;
using Digiphoto.Lumen.PresentationFramework;
using log4net;
using static Digiphoto.Lumen.UI.Adorners.ComposingAdorner;
using Digiphoto.Lumen.Imaging.Correzioni;
using static Digiphoto.Lumen.UI.Adorners.ComposingAdorner2;

namespace Digiphoto.Lumen.UI.FotoRitocco {

	/// <summary>
	/// Interaction logic for FotoRitocco.xaml
	/// </summary>
	public partial class FotoRitocco : UserControlBase, IObserver<RitoccoPuntualeMsg> {

		protected static readonly ILog _giornale = LogManager.GetLogger( typeof( ViewModelBase ) );

		private FotoRitoccoViewModel viewModel {
			get {
				return (FotoRitoccoViewModel) this.DataContext;
			}
		}

		public FotoRitocco() {
			
			InitializeComponent();

			this.DataContextChanged += fotoRitocco_DataContextChanged;

			// Mi sottoscrivo per ascoltare i messaggi di fotoritocco per ribindare i controlli.
			IObservable<RitoccoPuntualeMsg> observable = LumenApplication.Instance.bus.Observe<RitoccoPuntualeMsg>();
			observable.Subscribe( this );

			// Questo bottone è solo UI. Lo gestisco io da qui, senza viewmodel
			toggleButtonReticolo.DataContext = this;
		}

		private void fotoRitocco_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {

			selettoreMaschera.DataContext = viewModel.selettoreMascheraViewModel;

			viewModel.dialogProvider = this;
			viewModel.editorModeChangedEvent += cambiareModoEditor;

			viewModel.PropertyChanged += _viewModel_PropertyChanged;

			//			this.KeyDown += new KeyEventHandler( onFotoRitoccoUserControl_KeyDown );			

			// Fino a che non renderizzo i controlli per davvero, non so quanto sia l'area di fotoritocco. Per ora quindi mi setto un valore verosimile e funzionante.
			viewModel.frpContenitoreMaxW = 500;
			viewModel.frpContenitoreMaxH = 500;
		}

		void _viewModel_PropertyChanged( object sender, PropertyChangedEventArgs e ) {

			if( e.PropertyName == "logo" ) {
				// cambiato il logo.
				this.Dispatcher.BeginInvoke( creaImmaginettaLogoAction );
			}


			if( e.PropertyName == "scritta" ) {	

				// Devo eseguirlo nel thread della UI perché mi servono le dimensioni dei componenti già renderizzati
				this.Dispatcher.BeginInvoke( scrittaToUIAction );
	
			}


			if( e.PropertyName == "fotografiaInModifica" ) {

				// Se la nuova fotografia ha la ratio diversa da quella prima...
				eliminaReticoloPerpendicolare();

				if( viewModel.fotografiaInModifica == null ) {
					azzeraGestioneRitocco();
				}
			}


			if( e.PropertyName == "modificheInCorso" ) {


			}
		}

		#region Aggancio Controlli Bindings

		private void sliderLuminosita_ValueChanged( object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e ) {

			if( viewModel != null ) {
				if( viewModel.forseCambioEffettoCorrente( typeof( LuminositaContrastoEffect ) ) )
					bindaSliderLuminositaContrasto();
				viewModel.forseInizioModifiche();
			}
		}

		private void sliderContrasto_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {

			if( viewModel != null ) {
				if( viewModel.forseCambioEffettoCorrente( typeof( LuminositaContrastoEffect ) ) )
					bindaSliderLuminositaContrasto();
				viewModel.forseInizioModifiche();
			}
		}


		private void sliderRuota_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {

			if( viewModel != null ) {
				if( viewModel.forseCambioTrasformazioneCorrente( FotoRitoccoViewModel.TFXPOS_ROTATE ) )
					bindaSliderRuota();
				viewModel.forseInizioModifiche();
			}
		}


		private void sliderDominanti_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {

			if( viewModel != null ) {
				if( viewModel.forseCambioEffettoCorrente( typeof( DominantiEffect ) ) )
					bindaSlidersDominanti();
				viewModel.forseInizioModifiche();
			}
		}

		private void sliderZoom_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {

			if( viewModel != null ) {
				if( viewModel.forseCambioTrasformazioneCorrente( FotoRitoccoViewModel.TFXPOS_ZOOM ) )
					bindaSliderZoom();
				viewModel.forseInizioModifiche();
			}
		}

		private void sliderTrasla_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {
			if( viewModel != null ) {
				if( viewModel.forseCambioTrasformazioneCorrente( FotoRitoccoViewModel.TFXPOS_TRANSLATE ) )
					bindaSliderTrasla();
				viewModel.forseInizioModifiche();
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

			if( mantieniValori && viewModel.dominantiEffect == null ) {
				sliderDominanteRed.Value = 0;
				sliderDominanteGreen.Value = 0;
				sliderDominanteBlue.Value = 0;
				return;
			}

			double salvaValoreR = viewModel.dominantiEffect.Red;
			double salvaValoreG = viewModel.dominantiEffect.Green;
			double salvaValoreB = viewModel.dominantiEffect.Blue;

			bindaSliderDominanteRGB( sliderDominanteRed,   DominantiEffect.RedProperty );
			bindaSliderDominanteRGB( sliderDominanteGreen, DominantiEffect.GreenProperty );
			bindaSliderDominanteRGB( sliderDominanteBlue,  DominantiEffect.BlueProperty );

			if( mantieniValori ) {
				viewModel.dominantiEffect.Red = salvaValoreR;
				viewModel.dominantiEffect.Green = salvaValoreG;
				viewModel.dominantiEffect.Blue = salvaValoreB;
			}
		}

		private void bindaSliderDominanteRGB( Slider sliderSorgente, DependencyProperty prop ) {
			Binding binding = new Binding();
			binding.Source = sliderSorgente;
			binding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			binding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( viewModel.dominantiEffect, prop, binding );
		}

		private void bindaSliderRuota() {
			bindaSliderRuota( false );
		}
		
		private void bindaSliderRuota( bool mantieniValore ) {

			if( FotoRitoccoViewModel.isTrasformazioneNulla( viewModel.trasformazioneRotate ) ) {
				sliderRuota.Value = 0;  // per sicurezza riporto lo slider nella sua posizione neutra
				return;
			}

			double salvaValore = ((RotateTransform)viewModel.trasformazioneRotate).Angle;

			Binding binding = new Binding();
			binding.Source = sliderRuota;
			binding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			binding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( viewModel.trasformazioneRotate, RotateTransform.AngleProperty, binding );

			if( mantieniValore ) {
				((RotateTransform)viewModel.trasformazioneRotate).Angle = salvaValore;
			}
		}

		private void bindaSliderZoom() {
			bindaSliderZoom( false );
		}

		private void bindaSliderZoom( bool mantieniValore ) {

			if( FotoRitoccoViewModel.isTrasformazioneNulla( viewModel.trasformazioneZoom ) ) {
				sliderZoom.Value = 1;
				return;
			}

			double scaleX = ((ScaleTransform)viewModel.trasformazioneZoom).ScaleX;
			double scaleY = ((ScaleTransform)viewModel.trasformazioneZoom).ScaleY;

			Binding binding = new Binding();
			binding.Source = sliderZoom;
			binding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			binding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( viewModel.trasformazioneZoom, ScaleTransform.ScaleXProperty, binding );
			BindingOperations.SetBinding( viewModel.trasformazioneZoom, ScaleTransform.ScaleYProperty, binding );

			if( mantieniValore ) {
				((ScaleTransform)viewModel.trasformazioneZoom).ScaleX = scaleX;
				((ScaleTransform)viewModel.trasformazioneZoom).ScaleY = scaleY;
			}
		}

		private void bindaSliderTrasla() {
			bindaSliderTrasla( false );
		}

		private void bindaSliderTrasla( bool mantieniValore ) {
		
			if( FotoRitoccoViewModel.isTrasformazioneNulla( viewModel.trasformazioneTranslate ) ) {
				sliderTraslaX.Value = 0;
				sliderTraslaY.Value = 0;
				return;
			}

			double salvaX = ((TranslateTransform)viewModel.trasformazioneTranslate).X;
			double salvaY = ((TranslateTransform)viewModel.trasformazioneTranslate).Y;

			Binding bindingSx = new Binding();
			bindingSx.Source = sliderTraslaX;
			bindingSx.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			bindingSx.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( viewModel.trasformazioneTranslate, TranslateTransform.XProperty, bindingSx );

			Binding bindingSy = new Binding();
			bindingSy.Source = sliderTraslaY;
			bindingSy.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			bindingSy.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( viewModel.trasformazioneTranslate, TranslateTransform.YProperty, bindingSy );

			// Setto anche le dimensioni di riferimento dell'area di modifica. Mi serviranno per riproporzionare sui provini o sulle risultanti

			if( mantieniValore ) {
				((TranslateTransform)viewModel.trasformazioneTranslate).X = salvaX;
				((TranslateTransform)viewModel.trasformazioneTranslate).Y = salvaY;
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

			if( mantieniValore && viewModel.luminositaContrastoEffect == null ) {
				sliderContrasto.Value = 1;
				return;
			}

			double salvaValore = viewModel.luminositaContrastoEffect.Contrast;

			Binding contrBinding = new Binding();
			contrBinding.Source = sliderContrasto;
			contrBinding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			contrBinding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( viewModel.luminositaContrastoEffect, LuminositaContrastoEffect.ContrastProperty, contrBinding );

			if( mantieniValore )
				viewModel.luminositaContrastoEffect.Contrast = salvaValore;
		}

		private void bindaSliderLuminosita() {
			bindaSliderLuminosita( false );
		}

		/// <summary>
		/// Creo il binding tra la property dell'effetto e lo slider che lo muove
		/// </summary>
		private void bindaSliderLuminosita( bool mantieniValore ) {

			if( viewModel.luminositaContrastoEffect == null ) {
				sliderLuminosita.Value = 0;
				return;
			}

			double salvaValore = viewModel.luminositaContrastoEffect.Brightness;

			Binding lumBinding = new Binding();
			lumBinding.Mode = BindingMode.TwoWay; // .TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			lumBinding.Source = sliderLuminosita;
			lumBinding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( viewModel.luminositaContrastoEffect, LuminositaContrastoEffect.BrightnessProperty, lumBinding );

			if( mantieniValore )
				viewModel.luminositaContrastoEffect.Brightness = salvaValore;
		}

		#endregion Aggancio Controlli Bindings

		void cambiareModoEditor( object sender, EditorModeEventArgs args ) {

			switch( args.modalitaEdit ) {
				
				case ModalitaEdit.GestioneMaschere:

					initGestioneMaschere();
					break;

				case ModalitaEdit.FotoRitocco:

					initGestioneRitocco();
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


				imageFotina.ContextMenu = (ContextMenu )this.Resources ["contextMenuImageFotina"];
				foreach( MenuItem item in imageFotina.ContextMenu.Items ) {
					if( item.Name == "menuItemBringToFront" ) {
						item.Click += menuItemBringToFront_Click;
					}
					if( item.Name == "menuItemRemoveFromComposition" ) {
						item.Click += menuItemRemoveFromComposition_Click;
					}

				}

				portaInPrimoPianoFotina( imageFotina );

 				primoPianoCanvasMask( true );
			}
		}
		
		/// <summary>
		/// Elimina tutti gli Adornes da tutte le immagini che sono state aggiunte
		/// </summary>
		private void rimuoviTutteLeManiglietteMsk() {

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

		private void rimuoviTutteLeManiglietteRit() {

			rimuovereManiglietteScritta();
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

		private ComposingAdorner AddAdorner( UIElement element ) {

			ComposingAdorner adorner = null;

			AdornerLayer adornerlayer = AdornerLayer.GetAdornerLayer( element );
			if( adornerlayer == null ) {
				element.UpdateLayout();
				adornerlayer = AdornerLayer.GetAdornerLayer( element );
			}

			if( adornerlayer != null ) {
				if( adornerlayer.GetAdorners( element ) == null || adornerlayer.GetAdorners( element ).Length == 0 ) {
					adorner = new ComposingAdorner( element );
					adornerlayer.Add( adorner );
				}
			} else {
				if( Debugger.IsAttached )
					Debugger.Break();  // Strano: non dovrebbe : Indagare
			}

			return adorner;
		}

		private ComposingAdorner2 AddAdorner2( UIElement element, Maniglie qualiManiglie ) {

			ComposingAdorner2 adorner = null;
			
			AdornerLayer adornerlayer = AdornerLayer.GetAdornerLayer( element );
			if( adornerlayer == null ) {
				element.UpdateLayout();
				adornerlayer = AdornerLayer.GetAdornerLayer( element );
			}

			if( adornerlayer != null ) { 
				if( adornerlayer.GetAdorners( element ) == null || adornerlayer.GetAdorners( element ).Length == 0 ) {
					adorner = new ComposingAdorner2( element, qualiManiglie );
					adornerlayer.Add( adorner );
				}
			} else {
				if( Debugger.IsAttached )
					Debugger.Break();  // Strano: non dovrebbe : Indagare
			}

			return adorner;
		}


		private void menuItemBringToFront_Click( object sender, RoutedEventArgs e ) {

			MenuItem menuItem = sender as MenuItem;

			ContextMenu contextMenu = menuItem.Parent as ContextMenu;

			Image imageTarget = contextMenu.PlacementTarget as Image;

			if( imageTarget != null )
				portaInPrimoPianoFotina( imageTarget );
		}

		private void menuItemRemoveFromComposition_Click( object sender, RoutedEventArgs e ) {

			MenuItem menuItem = sender as MenuItem;

			ContextMenu contextMenu = menuItem.Parent as ContextMenu;

			Image imageTarget = contextMenu.PlacementTarget as Image;

			if( imageTarget != null ) {
				canvasMsk.Children.Remove( imageTarget );
			}

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
					var dragData = new DataObject( typeof( Fotografia ), (Fotografia)container.DataContext);
					DragDrop.DoDragDrop( container, dragData, DragDropEffects.Copy );
				}
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

			// Devo "Arrangiare" il canvas altrimenti non ha dimensione (e la foto viene nera)
			var size = new Size( newWidth, newHeight );
			c.Measure( size );
			c.Arrange( new Rect( size ) );
			
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
			using( FileStream fs = FileUtil.waitForFile( nomeFile ) ) {
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

			RenderTargetBitmap bitmapIncorniciata = componiBitmapDaMaschera( canvasDefinitivo );

			viewModel.salvareImmagineIncorniciata(firstFotoInCanvas ,bitmapIncorniciata );

			azzeraGestioneMaschere();
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

		private void initGestioneRitocco() {

			azzeraGestioneMaschere();

			// ---

			listBoxImmaginiDaModificare.SetValue( MultiSelect.IsEnabledProperty, true );
			listBoxImmaginiDaModificare.SetValue( MultiSelect.MaxNumSelectedItemProperty, 3 );

			primoPianoCanvasMask( true );
		}

		void initGestioneMaschere() {

			azzeraGestioneRitocco();

			// Porto il canvas di gestione delle maschere in primo piano
			primoPianoCanvasMask( false );

		}

		void azzeraGestioneRitocco() {

			rimuoviTutteLeManiglietteRit();

		}

		/// <summary>
		/// Quando rifiuto la maschera, devo togliere dal video eventuali componenti grafici rimasti in mezzo ai piedi.
		/// </summary>
		void azzeraGestioneMaschere() {

			rimuoviTutteLeManiglietteMsk();

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
						viewModel.rifiutareCorrezioni( daTogliere, true );
					}
				}

			} else {

				// Sto cliccando sulla foto nella lista di attesa. Non so perché mi 
				// solleva questo evento.
// DACANC:				_viewModel.forzaRefreshStato();

			}
		}


		private void listBoxImmaginiDaModificare_PreviewMouseRightButtonDown( object sender, MouseButtonEventArgs e ) {
		
			// Se ci sono modifiche in corso, non permetto di modificare la selezione
			if( viewModel.modificheInCorso || viewModel.modalitaEdit ==  ModalitaEdit.GestioneMaschere ) {
				e.Handled = false;
				return;
			}
			
			ListBoxItem listBoxItem = SelectItemOnRightClick( e );
			if( listBoxItem != null ) {

				viewModel.setModalitaSingolaFoto( (Fotografia)listBoxItem.Content );

                // Questo mi evita di selezionare la foto quando clicco con il destro.
                // e.Handled = true;
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

			viewModel.trayIconProvider.showInfo("AVVISO", msg.ToString(), 1500);
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
				return canvasMsk.Children.OfType<Image>().SingleOrDefault( i => "!SELEZ!".Equals(i.Tag) );
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

		Action _scrittaToUIAction;
		Action scrittaToUIAction
		{
			get
			{
				if( _scrittaToUIAction == null )
					_scrittaToUIAction = new Action( scrittaToUI );
				return _scrittaToUIAction;
			}
		}

		Action _creaImmaginettaLogoAction;
		Action creaImmaginettaLogoAction {
			get {
				if( _creaImmaginettaLogoAction == null )
					_creaImmaginettaLogoAction = new Action( creaImmaginettaLogo );
				return _creaImmaginettaLogoAction;
			}
		}

		public void OnNext( RitoccoPuntualeMsg rpMsg ) {

			// Questi sono effetti
			bindaSliderLuminosita( true );
			bindaSliderContrasto( true );
			bindaSlidersDominanti( true );

			// Queste sono trasformazioni
			bindaSliderRuota( true );
			bindaSliderZoom( true );
			bindaSliderTrasla( true );

			// Devo dare il fuoco allo UserControl del fotoritocco, altrimenti non mi sente l'evento KeyDown per salvare le correzioni.
			spostareIlFocus( fotoRitoccoUserControl );
		}

		void spostareIlFocus( UIElement element ) {

			if( !element.IsFocused ) {
				Action focusAction = () => element.Focus();
				this.Dispatcher.BeginInvoke( focusAction, DispatcherPriority.ApplicationIdle );
			}
		}
	

		void creaImmaginettaLogo() {

			// Rimuovo eventuale elemento precedente
			var prec = AiutanteUI.FindVisualChild<Image>( gridRitocco, "imageLogino" );
			if( prec != null )
				gridRitocco.Children.Remove( prec );


			if( viewModel.logo == null )
				return;
			String nomeFileLogo = PathUtil.nomeCompletoLogo( viewModel.logo );
			if( File.Exists( nomeFileLogo ) == false ) {
				ShowError( nomeFileLogo, "Logo inesistente", null );
				return;
			}
			Image imageLogino = new Image();


			// Metto un nome univoco al componente
			imageLogino.Name = "imageLogino";


			BitmapImage bmpLogo = new BitmapImage( new Uri(nomeFileLogo) );

			imageLogino.BeginInit();
			imageLogino.Source = bmpLogo;
			imageLogino.EndInit();

//			imageLogino.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
//			imageLogino.VerticalAlignment = System.Windows.VerticalAlignment.Center;

			Thickness margin = calcolaCoordinateLogo( (int)bmpLogo.Width, (int)bmpLogo.Height, ref imageLogino );
			imageLogino.Margin = margin;

			Canvas.SetZIndex( imageLogino, 90 );
			gridRitocco.Children.Add( imageLogino );
			// AddAdorner( imageLogino );


			imageLogino.ContextMenu = (ContextMenu)this.Resources["contextMenuImageFotina"];
			foreach( MenuItem item in imageLogino.ContextMenu.Items ) {
				if( item.Name == "menuItemBringToFront" ) {
					item.Click += menuItemBringToFront_Click;
				}
				if( item.Name == "menuItemRemoveFromComposition" ) {
					item.Click += menuItemRemoveFromComposition_Click;
				}
			}

			// portaInPrimoPianoFotina( imageLogino );

		}
		



		private void Adorner_CambiatoQualcosa( object sender, EventArgs e ) {
			viewModel.forseInizioModifiche();
		}

		void scrittaToUI() {

			// Elimmino eventuale adorner precedente
			var al1 = AdornerLayer.GetAdornerLayer( viewBoxScritta );
				if( al1 != null ) { 
					Adorner[] toRemoveArray = al1.GetAdorners( viewBoxScritta );
					Adorner toRemove;
					if( toRemoveArray != null ) {
						toRemove = toRemoveArray[0];
						al1.Remove( toRemove );
				}
			}

			// Pulisco tutte le eventuali trasformazioni precedenti
			viewBoxScritta.RenderTransform = new TransformGroup();

				
			var q = this.gridRitocco.ActualWidth == 0 || imageRitoccata.ActualWidth == 0;
			if( q ) {
				// Se arrivo qui, significa che la videata, ancora non è mai stata renderizzata, e quindi i componenti sono nulli.
				// Succede quando chiamo in modifica una foto dalla gallery per la prima volta,
				// e questa ha la scritta da decorare
				this.InvalidateMeasure();
				this.InvalidateVisual();
				this.UpdateLayout();
			}
			
			// Se la scritta è nulla, ho già spento tutto ed esco.
			if( viewModel.scritta == null )
				return;

				
			creareManiglietteScritta();
			
		}


		/// <summary>
		/// Dagli oggetti video (UI), creo i valori per la Scritta
		/// in modo che sia poi riproducibile sui vari formati della foto
		/// 
		/// l'oggetto Scritta che è un oggetto particolarmente complesso
		/// perché ha lui stesso le trasformazioni
		/// </summary>
		void uiToScritta() {

			if( viewModel == null || viewModel.scritta == null )
				return;


			var qq = textPathScritta.PointToScreen( new Point( textPathScritta.ActualWidth, textPathScritta.ActualHeight ) ) - textPathScritta.PointToScreen( new Point( 0, 0 ) );
			viewModel.scritta.width = Convert.ToInt32( qq.X );
			viewModel.scritta.height = Convert.ToInt32( qq.Y );

			Vector offset = VisualTreeHelper.GetOffset( viewBoxScritta );

			GeneralTransform generalTransform1 = viewBoxScritta.TransformToAncestor( gridRitocco );
			Point currentPoint1 = generalTransform1.Transform( new Point( 0, 0 ) );
#if true
			viewModel.scritta.rifContenitoreW = Convert.ToInt32( imageRitoccata.ActualWidth );
			viewModel.scritta.rifContenitoreH = Convert.ToInt32( imageRitoccata.ActualHeight );


			GeneralTransform generalTransform2 = viewBoxScritta.TransformToVisual( imageRitoccata );
			Point currentPoint2 = generalTransform2.Transform( new Point( 0, 0 ) );

			viewModel.scritta.left = Convert.ToInt32( currentPoint2.X );
			viewModel.scritta.top = Convert.ToInt32( currentPoint2.Y );
#else
			_viewModel.scritta.rifContenitoreW = borderImage1.ActualWidth;
			_viewModel.scritta.rifContenitoreH = borderImage1.ActualHeight;

			// La differenza con il caso precedente è di un solo pixel dovuto al riquadro del bordo.
			GeneralTransform generalTransform3 = viewBoxScritta.TransformToVisual( borderImage1 );
			Point currentPoint3 = generalTransform3.Transform( new Point( 0, 0 ) );

			_viewModel.scritta.left = currentPoint3.X;
			_viewModel.scritta.top = currentPoint3.Y;
#endif


			// Creo le trasformazioni
			Trasla trasla = null;
			Ruota ruota = null;
			Zoom zoom = null;



			var transformGrp = viewBoxScritta.RenderTransform;
			if( transformGrp is TransformGroup ) {

				foreach( Transform tr in (transformGrp as TransformGroup).Children ) {

					if( tr is RotateTransform ) {

						// memorizzo la rotazione
						RotateTransform rot = (RotateTransform)tr;
						ruota = new Ruota();
						ruota.gradi = (float)rot.Angle;
					}

					if( tr is TranslateTransform ) {

						// memorizzo la traslazione (spostamento)
						trasla = new Trasla();
						trasla.rifW = viewModel.scritta.rifContenitoreW;
						trasla.rifH = viewModel.scritta.rifContenitoreH;

						TranslateTransform tra = (TranslateTransform)tr;
						trasla.offsetX = tra.X;
						trasla.offsetY = tra.Y;
					}

					if( tr is ScaleTransform ) {
						ScaleTransform stx = (ScaleTransform)tr;
						zoom = new Zoom();
						zoom.fattore = stx.ScaleX;  // X o Y sono uguali
					}

				}

				// scarico
				viewModel.scritta.traslazione = (trasla == null || trasla.isInutile) ? null : trasla ;
				viewModel.scritta.rotazione = (ruota == null || ruota.isInutile) ? null : ruota;
				viewModel.scritta.zoom = (zoom == null || zoom.isInutile) ? null : zoom;
			} 

		}

		/// <summary>
		/// Creo adorner con le menigliette per gestire la scritta
		/// </summary>
		void creareManiglietteScritta() {

			// Centro e posiziono rispetto alla immagine della foto
			//			viewBoxScritta.Width = borderCornice.ActualWidth;
			viewBoxScritta.Width = double.NaN;
			viewBoxScritta.Height = double.NaN;
			if( viewBoxScritta.Width == 0 && Debugger.IsAttached )
				Debugger.Break();
			Canvas.SetLeft( viewBoxScritta, double.NaN );
			Canvas.SetTop( viewBoxScritta, double.NaN );

			var quali = Maniglie.All ^ Maniglie.Flip;
			ComposingAdorner2 adorner = AddAdorner2( viewBoxScritta, quali );
			if( adorner != null )
				adorner.CambiatoQualcosa += Adorner_CambiatoQualcosa;
			else
				_giornale.Error( "Impossibile che adorner sia nullo" );

			// ---
			// ---

			if( viewModel.scritta != null ) {

				// Applico eventuali valori di trasformazione precedenti

				if( viewModel.scritta.rotazione != null && viewModel.scritta.rotazione.isInutile == false ) {
					adorner.impostaRotazioneDefault( viewModel.scritta.rotazione.gradi );
				}

				if( viewModel.scritta.traslazione != null && viewModel.scritta.traslazione.isInutile == false ) {

					var x = viewModel.scritta.traslazione.offsetX * imageRitoccata.ActualWidth / viewModel.scritta.traslazione.rifW;
					var y = viewModel.scritta.traslazione.offsetY * imageRitoccata.ActualHeight / viewModel.scritta.traslazione.rifH;

					adorner.impostaTraslazioneDefault( x, y );
				}

				if( viewModel.scritta.zoom != null && viewModel.scritta.zoom.isInutile == false ) {
					adorner.impostaZoomDefault( viewModel.scritta.zoom.fattore );
				}
			}
		}

		void rimuovereManiglietteScritta() {
			// Rimuovo eventuale adorner dalla scritta
			AdornerLayer adornerlayer = AdornerLayer.GetAdornerLayer( viewBoxScritta );
			if( adornerlayer != null ) { 
				var adorners = adornerlayer.GetAdorners( viewBoxScritta );
				if( adorners != null ) {
					for( int i = adorners.Length - 1; i >= 0; i-- ) {
						ComposingAdorner2 ca = (adorners[i] as ComposingAdorner2);
						ca.CambiatoQualcosa -= Adorner_CambiatoQualcosa;
						adornerlayer.Remove( adorners[i] );
					}
				}
			} else {
				// Come mai ?? Indagare ! Possiblile che non abbia già il suo adornerlayer ?
				// Significa che ancora quel componente non è stato ancora renderizzato neanche una volta ???
				// Oppure NON è visible.
			}
		}

		private Thickness calcolaCoordinateLogo( int wl, int hl, ref Image imageLogino ) {

			Thickness margin = new Thickness();

			if( LogoCorrettore.isLogoPosizionatoManualmente( viewModel.logo ) ) {
				// TODO
			} else {

				Point relativeLocation = borderCornice.TranslatePoint( new Point( 0, 0 ), gridRitocco );

				Rect r = LogoCorrettore.calcolaCoordinateLogoAutomatiche( (int)borderCornice.ActualWidth, (int)borderCornice.ActualHeight, wl, hl, viewModel.logo );

				imageLogino.Width = r.Width;
				imageLogino.Height = r.Height;

				margin.Left = r.Left + relativeLocation.X;
				margin.Top = r.Top + relativeLocation.Y;

				margin.Right = gridRitocco.ActualWidth - relativeLocation.X - r.Left - r.Width;
				margin.Bottom = gridRitocco.ActualHeight - relativeLocation.Y - r.Top - r.Height;
			}

			return margin;
		}

		private void imageRitoccata_MouseWheel( object sender, MouseWheelEventArgs e ) {

			bool saveModificheInCorso = viewModel.modificheInCorso;

			if( Keyboard.IsKeyDown( Key.LeftCtrl ) ) {
				// Rotazione
				spostareIlFocus( sliderRuota );
				double angolo = e.Delta > 0 ? sliderRuota.SmallChange : sliderRuota.SmallChange * (-1);
				sliderRuota.Value += angolo;
			} else if( Keyboard.IsKeyDown( Key.LeftShift ) ) {
				// Traslazione Y
				spostareIlFocus( sliderTraslaY );
				double deltaY = e.Delta > 0 ? sliderTraslaY.SmallChange : sliderTraslaY.SmallChange * (-1);
				sliderTraslaY.Value += deltaY;
			} else if( Keyboard.IsKeyDown( Key.LeftAlt ) ) {
				// Traslazione X
				spostareIlFocus( sliderTraslaX );
				double deltaX = e.Delta > 0 ? sliderTraslaX.SmallChange : sliderTraslaX.SmallChange * (-1);
				sliderTraslaX.Value += deltaX;
			} else {
				// Zoom
				double zoom = e.Delta > 0 ? sliderZoom.SmallChange : sliderZoom.SmallChange * (-1);
				sliderZoom.Value += zoom;
			}


			// Questo utilizzo con la ruota del mouse, siccome non ci sono dei click fisici sulle superfici
			// delle views, non provocano l'aggiornamento dello stato dei Command.CanExecute
			// In pratica non si accende il pulsante "Applica" come succede invece se muovo fisicamente gli 
			// sliders. Devo far rivalutare i CanExecute dei comandi.
			if( viewModel.modificheInCorso != saveModificheInCorso )
				CommandManager.InvalidateRequerySuggested();
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
			TranslateTransform tt = (TranslateTransform) viewModel.trasformazioneTranslate;
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

				if( Keyboard.IsKeyDown( Key.LeftShift ) ) {
					// Blocco l'asse X
					spostareIlFocus( sliderTraslaY );
					left = 0;
				} else if( Keyboard.IsKeyDown( Key.LeftAlt ) ) {
					// Blocco l'asse Y
					spostareIlFocus( sliderTraslaX );
					top = 0;
				}
				

				TransformGroup qq = imageRitoccata.RenderTransform as TransformGroup;

				if( qq != null ) {
					//	TranslateTransform tt = (TranslateTransform)qq.Children.First( c => c is TranslateTransform );
					sliderTraslaX.Value = left;
					sliderTraslaY.Value = top;
				} else {
					// Strano che sia nullo. Come mai ?
				}
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

			// Applicare le correzioni con il tasto SPAZIO
			if( e.Key == Key.Space ) {

				// Se premo lo spazio nella text box della scritta, non devo fare nulla: sto solo scrivere
				if( ((FrameworkElement)e.OriginalSource).Name == "textBoxScritta" )
					return;

				if( viewModel.modalitaEdit == ModalitaEdit.GestioneMaschere && viewModel.possoSalvareMaschera ) {
					salvareMascheraButton_Click( null, null );
				}

				if( viewModel.modalitaEdit == ModalitaEdit.FotoRitocco ) {
					if( viewModel.applicareCorrezioniCommand.CanExecute( null ) )
						viewModel.applicareCorrezioniCommand.Execute( null );
					viewModel.selezionaProssimaFoto();
				}
			}

			// Spostamento della immaginetta con le freccette durante la composizione maschere.
			if( e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right ) {
				if( viewModel.modalitaEdit == ModalitaEdit.GestioneMaschere && fotinaSelezionata != null ) {
					// Sposto la fotina selezionata
					ComposingAdorner ado = (ComposingAdorner)AdornerLayer.GetAdornerLayer( fotinaSelezionata ).GetAdorners( fotinaSelezionata ).ElementAt(0);
					double deltaX = 0;
					double deltaY = 0;
					const int passo = 1;
					if( e.Key == Key.Left )
						deltaX = -passo;
					if( e.Key == Key.Right )
						deltaX = passo;
					if( e.Key == Key.Up )
						deltaY = -passo;
					if( e.Key == Key.Down )
						deltaY = passo;
					ado.sposta( deltaX, deltaY );
					e.Handled = true;
				}
			}

			// Rifiutare le correzioni transienti con il tasto ESCAPE
			if( e.Key == Key.Escape ) {
				if( viewModel.modalitaEdit == ModalitaEdit.FotoRitocco && viewModel.rifiutareCorrezioniCommand.CanExecute( null ) ) {
					viewModel.rifiutareCorrezioniCommand.Execute( null );
				}
			}


		}

		private void gridRitocco_SizeChanged(object sender, SizeChangedEventArgs e) {	

			viewModel.frpContenitoreMaxW = gridRitocco.ActualWidth;
			viewModel.frpContenitoreMaxH = gridRitocco.ActualHeight;

		}


		void dimensionaBordiPerAreaDiRispetto() {

			// Se non è indicato il rapporto non faccio nulla
			if( !viewModel.esisteRatioAreaRispetto )
				return;

			try {

				// Ora ricalcolo la dimensione dell'area di rispetto
				float ratio = (float)CoreUtil.evaluateExpression( Configurazione.UserConfigLumen.expRatioAreaDiRispetto );

				CalcolatoreAreeRispetto.Geo imageGeo = new CalcolatoreAreeRispetto.Geo();

				imageGeo.w = borderCornice.ActualWidth;
				imageGeo.h = borderCornice.ActualHeight;

				// Calcolo la fascia A
				Rect rettangoloA = CalcolatoreAreeRispetto.calcolaDimensioni( CalcolatoreAreeRispetto.Fascia.FasciaA, ratio, imageGeo );
				CalcolatoreAreeRispetto.Bordi bordiA = CalcolatoreAreeRispetto.calcolcaLatiBordo( CalcolatoreAreeRispetto.Fascia.FasciaA, ratio, imageGeo, imageGeo );

				// Calcolo la fascia B
				Rect rettangoloB = CalcolatoreAreeRispetto.calcolaDimensioni( CalcolatoreAreeRispetto.Fascia.FasciaB, ratio, imageGeo );
				CalcolatoreAreeRispetto.Bordi bordiB = CalcolatoreAreeRispetto.calcolcaLatiBordo( CalcolatoreAreeRispetto.Fascia.FasciaB, ratio, imageGeo, imageGeo );

				var p = borderCornice.TranslatePoint( new Point( 0, 0 ), gridRitocco );
				double currentLeft = p.X;
				double currentTop = p.Y;

				// Setto fascia A
				bordoRispettoA.Width = rettangoloA.Width;
				bordoRispettoA.Height = rettangoloA.Height;
				var left = currentLeft + rettangoloA.Left;
				var top = currentTop + rettangoloA.Top;
				var right = 0;
				var bottom = 0;

				Thickness ticA = new Thickness( left, top, right, bottom );
				bordoRispettoA.Margin = ticA;
				bordoRispettoA.BorderThickness = new Thickness( bordiA.left ? 2 : 0, bordiA.top ? 2 : 0, bordiA.right ? 2 : 0, bordiA.bottom ? 2 : 0 );

				// ---

				// Setto fascia B
				bordoRispettoB.Width = rettangoloB.Width;
				bordoRispettoB.Height = rettangoloB.Height;
				left = currentLeft + rettangoloB.Left;
				top = currentTop + rettangoloB.Top;
				right = 0;
				bottom = 0;

				Thickness ticB = new Thickness( left, top, right, bottom );
				bordoRispettoB.Margin = ticB;
				bordoRispettoB.BorderThickness = new Thickness( bordiB.left ? 2 : 0, bordiB.top ? 2 : 0, bordiB.right ? 2 : 0, bordiB.bottom ? 2 : 0 );
			} catch( Exception ) {
				// pazienza : dovrei loggare l'errore
			}
		}

		private void buttonTakeSnapshotPubblico_Click( object sender, RoutedEventArgs e ) {
			
			((App)Application.Current).gestoreFinestrePubbliche.eseguiSnapshotSuFinestraPubblica( this, this.tabControlRitoccoComposizione );

			// L'utente deve per forza chiudere la finestra
			ShowMessage( "Quando il cliente ha finito di vedere la foto\nclicca su OK per chiudere la finestra pubblica\ncon la snapshot del video", "Attendere che il cliente abbia visto" );

			( (App)Application.Current).gestoreFinestrePubbliche.chiudereFinestraSnapshotPubblico();
		}

		private void closeSnapshotPubblico_Click( object sender, RoutedEventArgs e ) {
			((App)Application.Current).gestoreFinestrePubbliche.chiudereFinestraSnapshotPubblico();
		}

		private void borderCornice_SizeChanged( object sender, SizeChangedEventArgs e ) {
			if( reticoloVisibile == true )
				creaReticoloPerpendicolare();

			dimensionaBordiPerAreaDiRispetto();
		}

		private void creaReticoloPerpendicolare() {
			// uso 10 righe nel lato più piccolo 
			// uso 15 righe nel lato più grande

			int nRigheOriz, nRigheVert;

			if( borderCornice.ActualWidth > borderCornice.ActualHeight ) {
				// landscape
				nRigheOriz = 10;
				nRigheVert = 15;
			} else {
				// portrait
				nRigheOriz = 15;
				nRigheVert = 10;
			}

			// Queste sono le coordinate di origine del borderCornice all'interno della grid.
			var p = borderCornice.TranslatePoint( new Point( 0, 0 ), gridRitocco );
			double currentLeft = p.X;
			double currentTop = p.Y;

			// Creo delle linee orizzontali
			double deltaY = (borderCornice.ActualHeight / nRigheOriz);
			for( int ii = 1; ii < nRigheOriz; ii++ ) {

				Line line = creaLinea();

				line.X1 = currentLeft;
				line.X2 = line.X1 + borderCornice.ActualWidth;
				line.Y1 = currentTop + (ii * deltaY);
				line.Y2 = line.Y1;
				gridRitocco.Children.Add( line );
			}

			// Creo delle linee verticali
			double deltaX = (borderCornice.ActualWidth / nRigheVert);
			for( int ii = 1; ii < nRigheVert; ii++ ) {

				Line line = creaLinea();

				line.X1 = currentLeft + (ii * deltaX);
				line.X2 = line.X1;
				line.Y1 = currentTop;
				line.Y2 = line.Y1 + borderCornice.ActualHeight;
				gridRitocco.Children.Add( line );
			}

		}

		private Line creaLinea() {
			Line line = new Line();
			line.Tag = "reticolo";
			line.HorizontalAlignment = HorizontalAlignment.Left;
			line.VerticalAlignment = VerticalAlignment.Top;

			Style style = this.FindResource( "reticoloPerpendicolareLineStyle" ) as Style;
			line.Style = style;
			return line;
		}
		void eliminaReticoloPerpendicolare() {

			for( int ii = gridRitocco.Children.Count-1; ii >= 0; ii-- ) {
				UIElement element = gridRitocco.Children[ii];
				if( element is FrameworkElement && "reticolo".Equals( (string)((FrameworkElement)element).Tag) )
					gridRitocco.Children.RemoveAt( ii );
			}
		}

		private bool? _reticoloVisibile;
		public bool? reticoloVisibile {
			get {
				return _reticoloVisibile;
			}
			set {
				_reticoloVisibile = value.Value;
				if( value == true ) {
					creaReticoloPerpendicolare();
				} else {
					eliminaReticoloPerpendicolare();
				}
			}
		}

		private void listBoxImmaginiDaModificare_ContextMenuOpening( object sender, ContextMenuEventArgs e ) {

			// Se sto modificando, non permetto il tasto destro
			if( viewModel.modificheInCorso || viewModel.modalitaEdit == ModalitaEdit.GestioneMaschere )
				e.Handled = true;
		}

		/// <summary>
		/// Questo metodo viene eseguito prima del command nel ViewModel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void applicareCorrezioniButton_Click( object sender, RoutedEventArgs e ) {
			uiToScritta();
		}

	}
}
