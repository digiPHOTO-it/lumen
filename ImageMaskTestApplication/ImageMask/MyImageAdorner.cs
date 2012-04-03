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

namespace ImageMask {

	public class MyImageAdorner : Adorner {

		Thumb rotateHandle;
		Thumb moveHandle;
		Thumb scaleHandle;
		Thumb flipHandle;

		Path outline;
		VisualCollection visualChildren;
		Point center;
		TranslateTransform translate;
		RotateTransform rotation;
		ScaleTransform scale;
		MatrixTransform flip;
		TransformGroup transformGroup;
		const int HANDLEMARGIN = 10;


		public MyImageAdorner( UIElement adornedElement ) : base( adornedElement ) {

			visualChildren = new VisualCollection( this );

			// ---
			rotateHandle = new Thumb();
			rotateHandle.Cursor = Cursors.Hand;
			rotateHandle.Width = 10;
			rotateHandle.Height = 10;
			rotateHandle.Background = Brushes.Blue;

			rotateHandle.DragDelta += new DragDeltaEventHandler( rotateHandle_DragDelta );
			rotateHandle.DragCompleted += new DragCompletedEventHandler( rotateHandle_DragCompleted );

			// ---
			flipHandle = new Thumb();
			flipHandle.Cursor = Cursors.Hand;
			flipHandle.Width = 10;
			flipHandle.Height = 10;
			flipHandle.Background = Brushes.Orange;

			flipHandle.PreviewMouseDown += new MouseButtonEventHandler( flipHandle_MouseDown );

			// ---
			moveHandle = new Thumb();
			moveHandle.Cursor = Cursors.SizeAll;
			moveHandle.Width = 15;
			moveHandle.Height = 15;
			moveHandle.Background = Brushes.Blue;

			moveHandle.DragDelta += new DragDeltaEventHandler( moveHandle_DragDelta );
			moveHandle.DragCompleted += new DragCompletedEventHandler( moveHandle_DragCompleted );

			// ---
			scaleHandle = new Thumb();
			scaleHandle.Cursor = Cursors.SizeNS;
			scaleHandle.Width = 10;
			scaleHandle.Height = 10;
			scaleHandle.Background = Brushes.Green;

			scaleHandle.DragDelta += new DragDeltaEventHandler( scaleHandle_DragDelta );
			scaleHandle.DragCompleted += new DragCompletedEventHandler( scaleHandle_DragCompleted );

			// ---
			outline = new Path();
			outline.Stroke = Brushes.Blue;
			outline.StrokeThickness = 1;

			rotation = new RotateTransform();
			translate = new TranslateTransform();
			scale = new ScaleTransform();
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

			//
			Rect rotateHandleRect = new Rect( -AdornedElement.RenderSize.Width / 2, -AdornedElement.RenderSize.Height / 2, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height );
			rotateHandle.ToolTip = "Ruota";
			rotateHandle.Arrange( rotateHandleRect );

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
			MoveNewTransformToAdornedElement( scale );
		}

		void scaleHandle_DragDelta( object sender, DragDeltaEventArgs e ) {
			Point pos = Mouse.GetPosition( this );
			double deltaY = center.Y - pos.Y;
			double scaleRatio = deltaY / center.Y;
			scale.ScaleX = scaleRatio;
			scale.ScaleY = scaleRatio;
			scale.CenterX = center.X;
			scale.CenterY = center.Y;
			outline.RenderTransform = scale;
		}

		void moveHandle_DragCompleted( object sender, DragCompletedEventArgs e ) {
			MoveNewTransformToAdornedElement( translate );
		}

		void moveHandle_DragDelta( object sender, DragDeltaEventArgs e ) {
			Point pos = Mouse.GetPosition( this );

			double deltaX = pos.X - center.X;
			double deltaY = pos.Y - center.Y;

			translate.X = deltaX;
			translate.Y = deltaY;
			outline.RenderTransform = translate;
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
	}
}
