using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Controls;
using System.Runtime.Serialization;
using System.Windows.Data;

namespace Digiphoto.Lumen.UI.Adorners {

	/// <summary>
	/// Questo tipo di Adorner, mi serve per gestire la composizione delle foto
	/// (gestione delle maschere).
	/// Questo consente di avere delle manigliette sulla immagine per 
	/// le operazioni di trasformazione :
	/// rotate - scale - flip - move (traslare)
	/// </summary>
	public class ComposingAdorner2 : Adorner {

		public event EventHandler<EventArgs> CambiatoQualcosa;

		[Flags]
		public enum Maniglie {
			Rotate	= 0x01,
			Scale	= 0x02,
			Move	= 0x04,
			Flip	= 0x08,
			All		= 0xff
		}

		Thumb rotateHandle;
		Thumb moveHandle;
		Thumb scaleHandle;
		Thumb flipHandle;

		/// <summary>
		/// Questo è il bordo blu
		/// </summary>
		Path outline;
		
		/// <summary>
		/// Questa è la collezione degli elementi visuali dell'adorner, ed è composta dal bordo blu e dalle manigliette
		/// </summary>
		VisualCollection visualChildren;

		/// <summary>
		/// Posizione centrale del componente adornato. Da qui parte la rotazione e lo zoom
		/// </summary>
		Point center;

		TranslateTransform translateTfx;
		Point posizInizioMove;

		RotateTransform rotateTfx;
		double angoloIniziale;

		ScaleTransform scaleTfx;

		MatrixTransform flipTfx;

		// il gruppo che contiene tutte le trasformazioni
		TransformGroup transformGroup;
		
		private const int HANDLESIZE = 20;


		#region Binding Properties
		public static readonly DependencyProperty traslaXProperty = DependencyProperty.Register(
				"traslaX", typeof( double ), typeof( ComposingAdorner2 ), new PropertyMetadata( default( double ) ) );

		public double traslaX
		{
			get { return (double)GetValue( traslaXProperty ); }
			set { SetValue( traslaXProperty, value ); }
		}

		public static readonly DependencyProperty traslaYProperty = DependencyProperty.Register(
			"traslaY", typeof( double ), typeof( ComposingAdorner2 ), new PropertyMetadata( default( double ) ) );

		public double traslaY
		{
			get { return (double)GetValue( traslaYProperty ); }
			set { SetValue( traslaYProperty, value ); }
		}

		public static readonly DependencyProperty ruotaAngleProperty = DependencyProperty.Register(
				"ruotaAngle", typeof( double ), typeof( ComposingAdorner2 ), new PropertyMetadata( default( double ) ) );

		public double ruotaAngle
		{
			get { return (double)GetValue( ruotaAngleProperty ); }
			set { SetValue( ruotaAngleProperty, value ); }
		}

		public static readonly DependencyProperty scaleFactorProperty = DependencyProperty.Register(
				"scaleFactor", typeof( double ), typeof( ComposingAdorner2 ), new PropertyMetadata( default( double ) ) );

		public double scaleFactor
		{
			get { return (double)GetValue( scaleFactorProperty ); }
			set { SetValue( scaleFactorProperty, value ); }
		}

		#endregion Binding Properties


		#region Costruttori

		public ComposingAdorner2( UIElement adornedElement ) : this( adornedElement, Maniglie.All ) {
		}

		public ComposingAdorner2( UIElement adornedElement, Maniglie qualiManiglie ) : base( adornedElement ) {

			visualChildren = new VisualCollection( this );

			// --- GRUPPO
			transformGroup = adornedElement.RenderTransform as TransformGroup;
			if( transformGroup == null ) {
				transformGroup = new TransformGroup();
			}

			// --- ROTAZIONE
			if( qualiManiglie == Maniglie.All || ( qualiManiglie & Maniglie.Rotate) == Maniglie.Rotate ) {
				rotateHandle = new Thumb();
				rotateHandle.Cursor = Cursors.Hand;
				rotateHandle.Width = HANDLESIZE;
				rotateHandle.Height = HANDLESIZE;
				
				rotateHandle.Background = Brushes.Blue;
				rotateHandle.DragStarted += rotateHandle_DragStarted;
				rotateHandle.DragDelta += rotateHandle_DragDelta;
				rotateHandle.DragCompleted += rotateHandle_DragCompleted;

				//
				rotateTfx = initRotateBinding();
				transformGroup.Children.Add( rotateTfx );
			}

			// --- FLIP SPECCHIO
			if( qualiManiglie == Maniglie.All || (qualiManiglie & Maniglie.Flip) == Maniglie.Flip ) {
				flipHandle = new Thumb();
				flipHandle.Cursor = Cursors.Hand;
				flipHandle.Width = 20;
				flipHandle.Height = 20;
				flipHandle.MinWidth = 20;
				flipHandle.MinHeight = 20;
				flipHandle.Background = Brushes.Orange;
				flipHandle.PreviewMouseDown += new MouseButtonEventHandler( flipHandle_MouseDown );
			}

			// --- MOVE SPOSTA TRASLA
			if( qualiManiglie == Maniglie.All || (qualiManiglie & Maniglie.Move) == Maniglie.Move ) {
				moveHandle = new Thumb();
				moveHandle.Cursor = Cursors.SizeAll;
				moveHandle.Width = double.NaN;  // grande quanto tutta la foto
				moveHandle.Height = double.NaN; // grande quanto tutta la foto
				moveHandle.Background = Brushes.Transparent;
				moveHandle.Opacity = 0;

				moveHandle.DragDelta += new DragDeltaEventHandler( moveHandle_DragDelta );
				moveHandle.DragStarted += new DragStartedEventHandler( moveHandle_DragStarted );
				moveHandle.DragCompleted += new DragCompletedEventHandler( moveHandle_DragCompleted );
				moveHandle.MouseRightButtonDown += new MouseButtonEventHandler( moveHandle_PreviewMouseRightButtonDown );
				moveHandle.PreviewMouseWheel += moveHandle_PreviewMouseWheel;

				//
				translateTfx = initTransformBinding();
				transformGroup.Children.Add( translateTfx );
			}

			// --- SCALE ZOOM
			if( qualiManiglie == Maniglie.All || (qualiManiglie & Maniglie.Scale) == Maniglie.Scale ) {
				scaleHandle = new Thumb();
				scaleHandle.Cursor = Cursors.SizeNS;
				scaleHandle.Width = 20;
				scaleHandle.Height = 20;
				scaleHandle.MinWidth = 20;
				scaleHandle.MinHeight = 20;
				scaleHandle.Background = Brushes.Green;

				scaleHandle.DragStarted += scaleHandle_DragStarted;
				scaleHandle.DragDelta += scaleHandle_DragDelta;
				scaleHandle.DragCompleted += scaleHandle_DragCompleted;

				//
				scaleFactor = 1;
				scaleTfx = initScaleBinding();
				// TODO vedremo
				// scaleRotella = new ScaleTransform();

				transformGroup.Children.Add( scaleTfx );
			}


			// ---
			outline = new Path();
			
			outline.Stroke = Brushes.Blue;
			outline.StrokeThickness = 1;

			visualChildren.Add( outline );

			if( rotateHandle != null )
				visualChildren.Add( rotateHandle );
			if( moveHandle != null )
				visualChildren.Add( moveHandle );
			if( scaleHandle != null )
				visualChildren.Add( scaleHandle );
			if( flipHandle != null )
				visualChildren.Add( flipHandle );

			adornedElement.RenderTransform = transformGroup;
		}


		~ComposingAdorner2() {

			// Si rompe perché gli oggetti grafici sono stati creati nel thread della UI
			// rilasciaTuttiBindings();

			rilasciaTuttiListener();
		}

		private void rilasciaTuttiListener() {

			// Rilascio qualche ascoltatore. Non so se serve davvero ma non si sa mai.
			if( rotateHandle != null ) {
				rotateHandle.DragStarted -= rotateHandle_DragStarted;
				rotateHandle.DragDelta -= rotateHandle_DragDelta;
				rotateHandle.DragCompleted -= rotateHandle_DragCompleted;
			}

			if( moveHandle != null ) {
				moveHandle.DragStarted -= moveHandle_DragStarted;
				moveHandle.DragDelta -= moveHandle_DragDelta;
				moveHandle.DragCompleted -= moveHandle_DragCompleted;
			}

			if( scaleHandle != null ) {
				scaleHandle.DragStarted -= scaleHandle_DragStarted;
				scaleHandle.DragDelta -= scaleHandle_DragDelta;
				scaleHandle.DragCompleted -= scaleHandle_DragCompleted;
			}
		}

		#endregion Costruttori

		protected override Size ArrangeOverride( Size finalSize ) {
		
			center = new Point( AdornedElement.RenderSize.Width / 2, AdornedElement.RenderSize.Height / 2 );

			//
			if( rotateHandle != null ) {
				Rect rotateHandleRect = new Rect( -AdornedElement.RenderSize.Width / 2, -AdornedElement.RenderSize.Height / 2, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height );
				rotateHandle.ToolTip = "Ruota (CTRL + rotella del mouse)";
				
				rotateHandle.Arrange( rotateHandleRect );
			}

			//
			if( flipHandle != null ) {
				Rect flipHandleRect = new Rect( AdornedElement.RenderSize.Width / 2, AdornedElement.RenderSize.Height / 2, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height );
				flipHandle.ToolTip = "Specchio";
				flipHandle.Arrange( flipHandleRect );
			}

			//
			if( scaleHandle != null ) {
				Rect scalehandleRect = new Rect( 0, -AdornedElement.RenderSize.Height / 2, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height );
				scaleHandle.ToolTip = "Scala (rotella del mouse)";

				scaleHandle.Arrange( scalehandleRect );

				// Se diminusco troppo la dimensione della maniglia, poi non riesco più ad acchiapparla. 
				// La tengo di dimensione constante
				scaleHandle.Width = HANDLESIZE / Math.Abs( scaleFactor );
			}

			//
			if( moveHandle != null ) {
				Rect finalRect = new Rect( finalSize );
				moveHandle.ToolTip = "Sposta";
				moveHandle.Arrange( finalRect );

				outline.Data = new RectangleGeometry( finalRect );
				outline.Arrange( finalRect );
			}

			return finalSize;
		}



		protected override int VisualChildrenCount {
			get {
				return visualChildren.Count;
			}
		}

		protected override Visual GetVisualChild( int index ) {
			return visualChildren [index];
		}



		private void scaleHandle_DragStarted( object sender, DragStartedEventArgs e ) {

			azioneScaleInizio();																																																																	
		}

		void azioneScaleInizio() {
			scaleTfx.CenterX = center.X;
			scaleTfx.CenterY = center.Y;
			posizInizioMove = Mouse.GetPosition( this );

//			rotellaStavoRuotando = false;
		}

		void scaleHandle_DragDelta( object sender, DragDeltaEventArgs e ) {
			
			Point pos = Mouse.GetPosition( this );

			// faccio cosi che ogni pixel equivale ad un 1 percento.
			var verticalChange = (posizInizioMove.Y - pos.Y);

			var perc = verticalChange/ 100;
			
			this.scaleFactor += perc;

			posizInizioMove = pos;
		}

		void scaleHandle_DragCompleted( object sender, DragCompletedEventArgs e ) {

			concludere();

		}

		const double _ampiezza = 0.04;

//		bool rotellaStavoRuotando = false;
//		bool rotellaStavoScalando = false;

		private void moveHandle_PreviewMouseWheel( object sender, MouseWheelEventArgs args ) {

			if( Keyboard.IsKeyDown( Key.LeftCtrl ) ) {

				// Prepara
//				if( rotellaStavoRuotando == false )
					azioneRotateInizio();

				// Ruota
//				rotellaStavoRuotando = true;
				double angolo = (args.Delta > 0 ? 5 : -5);
				azioneRotateContinuo( angolo );
				angoloIniziale = ruotaAngle;  // Incremento per la prossima rotellata

			} else {

				// Prepara
//				if( rotellaStavoScalando == false )
					azioneScaleInizio();


				// zoom (o scale)
//				rotellaStavoScalando = false;
				double incrementoScala = (args.Delta > 0 ? _ampiezza : _ampiezza * -1);
				scaleFactor += incrementoScala;
			}
		}

		#region Eventi spostamento

		void moveHandle_DragStarted( object sender, DragStartedEventArgs e ) {

			posizInizioMove = Mouse.GetPosition( this );

//			rotellaStavoRuotando = false;
//			rotellaStavoScalando = false;
		}

		void moveHandle_DragDelta( object sender, DragDeltaEventArgs e ) {

			Point newPosiz = Mouse.GetPosition( this );

			double deltaX = newPosiz.X - posizInizioMove.X;
			double deltaY = newPosiz.Y - posizInizioMove.Y;


#if false
			// Per adesso non ci penso. Tanto chi è che si metterebbe a spostare una immagine capovolta ?

			// Se ho già ruotato l'elemento, mi muovo di un pixel alla volta, altrimenti l'effetto è amplificato (non so perchè)
			if( ruotaAngle != 0 ) {
				if( deltaX > 1 )
					deltaX = 1;
				if( deltaX < -1 )
					deltaX = -1;
				if( deltaY > 1 )
					deltaY = 1;
				if( deltaY < -1 )
					deltaY = -1;
			}

			// Se la scritta è ribaltata, i movimenti cambiano di segno
			if( (ruotaAngle > 90 && ruotaAngle < 270) || ( ruotaAngle < -90 && ruotaAngle > -270 ) ) {
				deltaY = deltaY * -1;
			}
#endif


			traslaX += deltaX;
			traslaY += deltaY;

			posizInizioMove = newPosiz;
		}

		bool isRovesciata() {

			
				return true;

			if( ruotaAngle < -90 && ruotaAngle > -270 )
				return true;

			return false;
		}

		/// <summary>
		/// Quando premo il tasto destro sulla manigliona della move, devo fare aprire il menu contestuale che sta sulla Image (owner).
		/// Ho dovuto fare un trucco. In pratica qui intercetto il tasto destro sulla minigliona, e quindi lo rilancio allo UIElement interessato.
		/// Vedere qui:
		/// 	http://stackoverflow.com/questions/2008399/programmatically-open-context-menu-using-ui-automation
		/// </summary>
		void moveHandle_PreviewMouseRightButtonDown( object sender, MouseButtonEventArgs e ) {

			FrameworkElement owner = AdornedElement as FrameworkElement;

			// Se l'elemento adornato non ha un menu contestuale, allora non faccio nulla.
			if( owner.ContextMenu == null )
				return;

			//**********************
			//Ouch!!! What a hack
			//**********************

			//ContextMenuEventArgs is a sealed class, with private constructors
			//Instantiate it anyway ...
			ContextMenuEventArgs cmea = (ContextMenuEventArgs)FormatterServices.GetUninitializedObject( typeof( ContextMenuEventArgs ) );
			cmea.RoutedEvent = Image.ContextMenuOpeningEvent;
			cmea.Source = owner;
			
			//This will fire any developer code that is bound to the OpenContextMenuEvent
			owner.RaiseEvent( cmea );

			//The context menu didn't open because this is a hack, so force it open
			owner.ContextMenu.Placement = PlacementMode.Center;
			owner.ContextMenu.PlacementTarget = (UIElement)owner;
			owner.ContextMenu.IsOpen = true;
		}

		void flipHandle_MouseDown( object sender, MouseButtonEventArgs e ) {

			// Cerco se ho già inserito un flip nel gruppo delle trasformazioni...
			int pos = cercaFlip();
			if( pos < 0 ) {
				
				Matrix mMatrix = new Matrix();
				mMatrix.Scale( -1.0, 1.0 );
				mMatrix.OffsetX = AdornedElement.RenderSize.Width;  // il flip è solo orizzontale (niente Y)
				
				flipTfx = new MatrixTransform();
				flipTfx.Matrix = mMatrix;

				// Questa trasformazione deve stare per prima, altrimenti non becca il centro.
				transformGroup.Children.Insert( 0, flipTfx );
			} else {
				transformGroup.Children.RemoveAt( pos );   // ... si, nel gruppo è già inserito un flip, allora lo rimuovo.
			}

			concludere();
			
		}


		void moveHandle_DragCompleted( object sender, DragCompletedEventArgs e ) {

			if( e.VerticalChange == 0 && e.HorizontalChange == 0 ) {
				// Non ho spostato per niente. Ho solo cliccato sulla foto ma non la ho spostata.
			} else {
				concludere();
			}
		}

		#endregion Eventi spostamento

		/// <summary>
		/// Quando ho concluso la trasformazione, devo riposizionare gli elementi di decorazione
		/// e devo notificare eventualmente qualcuno che sta ascoltando.
		/// </summary>
		void concludere() {
			
			// Serve per riposizionare le manigliette
			InvalidateArrange();

			// Se qalcuno vuole essere notificato della avvenuta modifica
			CambiatoQualcosa?.Invoke( this, EventArgs.Empty );
		}

		/// <summary>
		/// Cerco se nelle trasformazioni è già presente il flip
		/// </summary>
		/// <returns>la posizione nel vettore</returns>
		int cercaFlip() {

			var mt = (MatrixTransform) transformGroup.Children.SingleOrDefault( c => c.GetType() == typeof( MatrixTransform ) );
			if( mt == null )
				return -1;
			else
				return transformGroup.Children.IndexOf( mt );
		}

		#region Eventi di Rotazione

		private void rotateHandle_DragStarted( object sender, DragStartedEventArgs e ) {

//			rotellaStavoRuotando = false;

			azioneRotateInizio();
		}

		void azioneRotateInizio() {
			rotateTfx.CenterX = center.X;
			rotateTfx.CenterY = center.Y;
			angoloIniziale = rotateTfx.Angle;
		}

		void rotateHandle_DragDelta( object sender, DragDeltaEventArgs e ) {

			Point pos = Mouse.GetPosition( this );

			double deltaX = pos.X - center.X;
			double deltaY = pos.Y - center.Y;

			double angle;
			if( deltaY.Equals( 0 ) ) {
				if( !deltaX.Equals( 0 ) ) {
					angle = 90;
				} else {
					return;
				}
			} else {
				double tan = deltaX / deltaY;
				angle = Math.Atan( tan );

				angle = angle * 180 / Math.PI;
			}


			// If the mouse crosses the vertical center, 
			// find the complementary angle.
			if( deltaY > 0 ) {
				angle = 180 - Math.Abs( angle );
			}

			// Rotate left if the mouse moves left and right
			// if the mouse moves right.
			if( deltaX < 0 ) {
				angle = -Math.Abs( angle );
			} else {
				angle = Math.Abs( angle );
			}

			if( Double.IsNaN( angle ) ) {
				return;
			}

			// Adjust the offset.
			double tanOffset = AdornedElement.RenderSize.Width / AdornedElement.RenderSize.Height;
			angle += Math.Atan( tanOffset ) * 180 / Math.PI;

			azioneRotateContinuo( angle );
		}


		void azioneRotateContinuo( double angle ) { 

			this.ruotaAngle = angoloIniziale + angle;

		}

		/// <summary>
		/// Rotates to the same angle as outline.
		/// </summary>
		void rotateHandle_DragCompleted( object sender,	DragCompletedEventArgs e ) {

//			rotellaStavoRuotando = false;

			// 
			center.X = rotateTfx.CenterX;
			center.Y = rotateTfx.CenterY;

			concludere();
		}

		#endregion Eventi di rotazione

		#region Binding val trasformazioni

		private TranslateTransform initTransformBinding( TranslateTransform t = null ) {

			var transform = t ?? new TranslateTransform();

			var xBinding = new Binding();
			xBinding.Source = this;
			xBinding.Path = new PropertyPath( "traslaX" );
			xBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			xBinding.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding( transform, TranslateTransform.XProperty, xBinding );

			var yBinding = new Binding();
			yBinding.Source = this;
			yBinding.Path = new PropertyPath( "traslaY" );
			yBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			yBinding.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding( transform, TranslateTransform.YProperty, yBinding );

			return transform;
		}

		private RotateTransform initRotateBinding( RotateTransform t = null ) {

			var transform = t ?? new RotateTransform();

			var aBinding = new Binding();
			aBinding.Source = this;
			aBinding.Path = new PropertyPath( "ruotaAngle" );
			aBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			aBinding.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding( transform, RotateTransform.AngleProperty, aBinding );

			return transform;
		}

		private ScaleTransform initScaleBinding( ScaleTransform t = null ) {

			var transform = t ?? new ScaleTransform();

			var aBinding = new Binding();
			aBinding.Source = this;
			aBinding.Path = new PropertyPath( "scaleFactor" );
			aBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			aBinding.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding( transform, ScaleTransform.ScaleXProperty, aBinding );
			BindingOperations.SetBinding( transform, ScaleTransform.ScaleYProperty, aBinding );

			return transform;
		}


		void rilasciaTuttiBindings() {

			BindingOperations.ClearAllBindings( rotateTfx ); 
			BindingOperations.ClearAllBindings( scaleTfx );
			BindingOperations.ClearAllBindings( translateTfx );
			// IL flip non ha proprietà bindate
		}

		#endregion Binding val trasformazioni


		public void impostaRotazioneDefault( double gradi ) {

			center = new Point( AdornedElement.RenderSize.Width / 2, AdornedElement.RenderSize.Height / 2 );
			rotateTfx.CenterX = center.X;
			rotateTfx.CenterY = center.Y;
			ruotaAngle = gradi;
		}

		public void impostaTraslazioneDefault( double x, double y ) {
			traslaX = x;
			traslaY = y;
		}

	}
}