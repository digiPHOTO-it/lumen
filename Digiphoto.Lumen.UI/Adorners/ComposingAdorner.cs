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
	public class ComposingAdorner : Adorner {

		Thumb rotateHandle;
		Thumb moveHandle;
		Thumb scaleHandle;
		Thumb flipHandle;

		Path outline;
		VisualCollection visualChildren;
		Point center;
		Point posizInizioMove;
		TranslateTransform translate;
		RotateTransform rotation;
		ScaleTransform scaleManiglia;
		ScaleTransform scaleRotella;
		MatrixTransform flip;
		TransformGroup transformGroup;
		const int HANDLEMARGIN = 10;


		public ComposingAdorner( UIElement adornedElement ) : base( adornedElement ) {

			visualChildren = new VisualCollection( this );

			// ---
			rotateHandle = new Thumb();
			rotateHandle.Cursor = Cursors.Hand;
			rotateHandle.Width = 20;
			rotateHandle.Height = 20;
			rotateHandle.Background = Brushes.Blue;

			rotateHandle.DragDelta += new DragDeltaEventHandler( rotateHandle_DragDelta );
			rotateHandle.DragCompleted += new DragCompletedEventHandler( rotateHandle_DragCompleted );

			// ---
			flipHandle = new Thumb();
			flipHandle.Cursor = Cursors.Hand;
			flipHandle.Width = 20;
			flipHandle.Height = 20;
			flipHandle.MinWidth = 20;
			flipHandle.MinHeight = 20;
			flipHandle.Background = Brushes.Orange;

			flipHandle.PreviewMouseDown += new MouseButtonEventHandler( flipHandle_MouseDown );

			// ---
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

			// ---
			scaleHandle = new Thumb();
			scaleHandle.Cursor = Cursors.SizeNS;
			scaleHandle.Width = 20;
			scaleHandle.Height = 20;
			scaleHandle.MinWidth = 20;
			scaleHandle.MinHeight = 20;
			scaleHandle.Background = Brushes.Green;

			scaleHandle.DragDelta += new DragDeltaEventHandler( scaleHandle_DragDelta );
			scaleHandle.DragCompleted += new DragCompletedEventHandler( scaleHandle_DragCompleted );

			// ---
			outline = new Path();


/* esempio
			Style style = new Style();
			Setter s1 = new Setter( Path.StrokeProperty, Brushes.Green );
			style.Setters.Add( s1 );
			outline.Style = style;
*/
			
			outline.Stroke = Brushes.Blue;
			outline.StrokeThickness = 1;


/*  TODO : non funziona. Sistemare
			DataTrigger dt = new DataTrigger();


			Binding tagBinding = new Binding( "Tag" );
			tagBinding.Source = adornedElement;
//			outline.SetBinding( Image.TagProperty, tagBinding );

			dt.Value = "!SELEZ!";
			dt.Binding = tagBinding;

			Setter setter = new Setter( Path.StrokeProperty, Brushes.Yellow );
			dt.Setters.Add( setter );

			// Creo lo stile per 
			Style style = new Style( outline.GetType() );
			style.Triggers.Add( dt );
			outline.Style = style;
*/
			// ---


			rotation = new RotateTransform();
			translate = new TranslateTransform();
			scaleManiglia = new ScaleTransform();
			scaleRotella = new ScaleTransform();
			flip = new MatrixTransform();
				
			transformGroup = adornedElement.RenderTransform as TransformGroup;
			if( transformGroup == null ) {
				transformGroup = new TransformGroup();
			}

			visualChildren.Add( outline );
			visualChildren.Add( rotateHandle );
			visualChildren.Add( moveHandle );
			visualChildren.Add( scaleHandle );
			visualChildren.Add( flipHandle );
		}

		protected override Size ArrangeOverride( Size finalSize ) {

			center = new Point( AdornedElement.RenderSize.Width / 2, AdornedElement.RenderSize.Height / 2 );

			const double minSize = 15;
			
			//
			Rect rotateHandleRect = new Rect( -AdornedElement.RenderSize.Width / 2, -AdornedElement.RenderSize.Height / 2, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height );
			rotateHandle.ToolTip = "Ruota";
			rotateHandle.Arrange( rotateHandleRect );
			if( rotateHandle.Width < minSize )
				rotateHandle.Width = minSize;

			//
			Rect flipHandleRect = new Rect( AdornedElement.RenderSize.Width/2, AdornedElement.RenderSize.Height/2, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height );
			flipHandle.ToolTip = "Specchio";
			flipHandle.Arrange( flipHandleRect );

			//
			Rect scalehandleRect = new Rect( 0, -AdornedElement.RenderSize.Height / 2, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height );
			scaleHandle.ToolTip = "Scala";
			scaleHandle.Arrange( scalehandleRect );

			//
			Rect finalRect = new Rect( finalSize );
			moveHandle.ToolTip = "Sposta";
			moveHandle.Arrange( finalRect );

			outline.Data = new RectangleGeometry( finalRect );
			outline.Arrange( finalRect );

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

		void scaleHandle_DragCompleted( object sender, DragCompletedEventArgs e ) {
			MoveNewTransformToAdornedElement( scaleManiglia );
		}

		void scaleHandle_DragDelta( object sender, DragDeltaEventArgs e ) {
			Point pos = Mouse.GetPosition( this );
			double deltaY = center.Y - pos.Y;
			double scaleRatio = deltaY / center.Y;
			scaleManiglia.ScaleX = scaleRatio;
			scaleManiglia.ScaleY = scaleRatio;
			scaleManiglia.CenterX = center.X;
			scaleManiglia.CenterY = center.Y;
			outline.RenderTransform = scaleManiglia;
		}

		const double _ampiezza = 0.02;
		private void moveHandle_PreviewMouseWheel( object sender, MouseWheelEventArgs args ) {

			if( Keyboard.IsKeyDown( Key.LeftCtrl ) ) {
				// rotazione
				double angolo = (args.Delta > 0 ? 5 : -5);
				rotation.Angle = angolo;
				rotation.CenterX = center.X;
				rotation.CenterY = center.Y;
				outline.RenderTransform = rotation;
				MoveNewTransformToAdornedElement( rotation );

			} else {
				// zoom (o scale)
				double incrementoScala = 1 + (args.Delta > 0 ? _ampiezza : _ampiezza * -1);
				scaleRotella.ScaleX = incrementoScala;
				scaleRotella.ScaleY = incrementoScala;
				outline.RenderTransform = scaleRotella;
				MoveNewTransformToAdornedElement( scaleRotella );
			}
		}



		void moveHandle_DragStarted( object sender, DragStartedEventArgs e ) {
			posizInizioMove = Mouse.GetPosition( this );
		}

		void moveHandle_DragCompleted( object sender, DragCompletedEventArgs e ) {

			if( e.VerticalChange == 0 && e.HorizontalChange == 0 ) {
				// Non ho spostato per niente. Ho solo cliccato sulla foto ma non la ho spostata.
			} else
				MoveNewTransformToAdornedElement( translate );
		}

		void moveHandle_DragDelta( object sender, DragDeltaEventArgs e ) {
			Point pos = Mouse.GetPosition( this );

			double deltaX = pos.X - posizInizioMove.X;
			double deltaY = pos.Y - posizInizioMove.Y;

			translate.X = deltaX;
			translate.Y = deltaY;
			outline.RenderTransform = translate;
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

			Matrix mMatrix = new Matrix();
			mMatrix.Scale( -1.0, 1.0 );
			mMatrix.OffsetX = AdornedElement.RenderSize.Width;  // il flip è solo orizzontale (niente Y)
			flip.Matrix = mMatrix;

			// Cerco se ho già inserito un flip nel gruppo delle trasformazioni...
			int pos = cercaFlip();
			if( pos < 0 )
				MoveNewTransformToAdornedElement( flip );  // ... no. Lo aggiungo
			else {
				transformGroup.Children.RemoveAt( pos );   // ... si, nel gruppo è già inserito un flip, allora lo rimuovo.
				this.InvalidateArrange();  // Le manigliette si devono spostare. Quindi invalido per ricalcolare.
			}
		}

		/// <summary>
		/// Cerco se nelle trasformazioni è già presente il flip
		/// </summary>
		/// <returns>la posizione nel vettore</returns>
		int cercaFlip() {
			int pos = -1;
			for( int ii = 0; ii < transformGroup.Children.Count && pos < 0; ii++ )
				if( transformGroup.Children[ii].Value.Equals( flip.Value ) )
					pos = ii;
			return pos;
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

			// Apply the rotation to the outline.
			rotation.Angle = angle;
			rotation.CenterX = center.X;
			rotation.CenterY = center.Y;
			outline.RenderTransform = rotation;
		}

		/// <summary>
		/// Rotates to the same angle as outline.
		/// </summary>
		void rotateHandle_DragCompleted( object sender,	DragCompletedEventArgs e ) {
			MoveNewTransformToAdornedElement( rotation );
		}

		private void MoveNewTransformToAdornedElement( Transform transform ) {
			if( transform == null ) {
				return;
			}
			var newTransform = transform.Clone();
			newTransform.Freeze();
			transformGroup.Children.Insert( 0, newTransform );
			AdornedElement.RenderTransform = transformGroup;

			outline.RenderTransform = Transform.Identity;
			this.InvalidateArrange();
		}


		public void sposta( double deltaX, double deltaY ) {
						
			translate.X = deltaX;
			translate.Y = deltaY;
			outline.RenderTransform = translate;
			MoveNewTransformToAdornedElement( translate );
		}
	}
}