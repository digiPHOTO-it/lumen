using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Point=System.Drawing.Point;
using System.Windows.Data;

namespace Digiphoto.Lumen.UI.Adorners
{
	public class CroppingAdorner : Adorner
	{
		#region Private variables
		// Width of the thumbs.  I know these really aren't "pixels", but px
		// is still a good mnemonic.
		private const int _cpxThumbWidth = 6;

		// PuncturedRect to hold the "Cropping" portion of the adorner
		private PuncturedRect _prCropMask;

		// Canvas to hold the thumbs so they can be moved in response to the user
		private Canvas _cnvThumbs;

		// Cropping adorner uses Thumbs for visual elements.  
		// The Thumbs have built-in mouse input handling.
		private CropThumb _crtTopLeft, _crtTopRight, _crtBottomLeft, _crtBottomRight;
		private CropThumb _crtTop, _crtLeft, _crtBottom, _crtRight;
		private CropThumb _moveHandle;
		private CheckBox _checkBoxKeepAspect;
		private Line _lineVert, _lineHoriz;

		// To store and manage the adorner's visual children.
		private VisualCollection _vc;

		// DPI for screen
		private static double s_dpiX, s_dpiY;
		#endregion

		#region Properties
		public Rect ClippingRectangle
		{
			get
			{
				return _prCropMask.RectInterior;
			}
		}

		/// <summary>
		/// Se voglio costringere l'area di crop ad essere proporzionale al
		/// Ratio dell'area di stampa, posso impostare qui il rapporto tra 
		/// la larghezza e l'altezza (esempio per un A4 = 297/210 = 1,414285714285714
		/// L'orientamento in realtà non è importante. Ci penso io a farlo coincidere
		/// </summary>
		public float AspectRatio {
			get;
			set;
		}

		private bool _mantainAspectRatio;
		public bool MantainAspectRatio {
			get {
				return _mantainAspectRatio;
			}
			set {
				_mantainAspectRatio = value;
			}
		}

		#endregion

		#region Routed Events
		public static readonly RoutedEvent CropChangedEvent = EventManager.RegisterRoutedEvent(
			"CropChanged",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(CroppingAdorner));

		public event RoutedEventHandler CropChanged
		{
			add
			{
				base.AddHandler(CroppingAdorner.CropChangedEvent, value);
			}
			remove
			{
				base.RemoveHandler(CroppingAdorner.CropChangedEvent, value);
			}
		}
		#endregion

		#region Dependency Properties
		static public DependencyProperty FillProperty = Shape.FillProperty.AddOwner(typeof(CroppingAdorner));

		public Brush Fill
		{
			get { return (Brush)GetValue(FillProperty); }
			set { SetValue(FillProperty, value); }
		}

		private static void FillPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			CroppingAdorner crp = d as CroppingAdorner;

			if (crp != null)
			{
				crp._prCropMask.Fill = (Brush)args.NewValue;
			}
		}
		#endregion

		#region Constructor
		static CroppingAdorner()
		{
			Color clr = Colors.Red;
			System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd((IntPtr)0);

			s_dpiX = g.DpiX;
			s_dpiY = g.DpiY;
			clr.A = 80;
			FillProperty.OverrideMetadata(typeof(CroppingAdorner),
				new PropertyMetadata(
					new SolidColorBrush(clr),
					new PropertyChangedCallback(FillPropChanged)));
		}

		public CroppingAdorner(UIElement adornedElement, Rect rcInit)
			: base(adornedElement)
		{
			_vc = new VisualCollection(this);
			_prCropMask = new PuncturedRect();
			_prCropMask.IsHitTestVisible = false;
			_prCropMask.RectInterior = rcInit;
			_prCropMask.Fill = Fill;
			_vc.Add(_prCropMask);
			_cnvThumbs = new Canvas();
			_cnvThumbs.HorizontalAlignment = HorizontalAlignment.Stretch;
			_cnvThumbs.VerticalAlignment = VerticalAlignment.Stretch;

			_vc.Add(_cnvThumbs);
			BuildCorner(ref _crtTop, Cursors.SizeNS);
			BuildCorner(ref _crtBottom, Cursors.SizeNS);
			BuildCorner(ref _crtLeft, Cursors.SizeWE);
			BuildCorner(ref _crtRight, Cursors.SizeWE);
			BuildCorner(ref _crtTopLeft, Cursors.SizeNWSE);
			BuildCorner(ref _crtTopRight, Cursors.SizeNESW);
			BuildCorner(ref _crtBottomLeft, Cursors.SizeNESW);
			BuildCorner(ref _crtBottomRight, Cursors.SizeNWSE);

			// Add handlers for Cropping.
			_crtBottomLeft.DragDelta += new DragDeltaEventHandler(HandleBottomLeft);
			_crtBottomRight.DragDelta += new DragDeltaEventHandler(HandleBottomRight);
			_crtTopLeft.DragDelta += new DragDeltaEventHandler(HandleTopLeft);
			_crtTopRight.DragDelta += new DragDeltaEventHandler(HandleTopRight);
			_crtTop.DragDelta += new DragDeltaEventHandler(HandleTop);
			_crtBottom.DragDelta += new DragDeltaEventHandler(HandleBottom);
			_crtRight.DragDelta += new DragDeltaEventHandler(HandleRight);
			_crtLeft.DragDelta += new DragDeltaEventHandler(HandleLeft);


			// Aggiungo maniglia per spostamento
			_moveHandle = new CropThumb( 15 );
			_moveHandle.Cursor = Cursors.SizeAll;
			_cnvThumbs.Children.Add( _moveHandle );
			_moveHandle.ToolTip = "Sposta";
//			_moveHandle.Background = Brushes.Orange;  // TODO non funziona il colore. Perché ??
			_moveHandle.Background = new SolidColorBrush( Colors.Orange );
			_moveHandle.DragDelta += new DragDeltaEventHandler( moveHandle_DragDelta );

			// Aggiungo una checkbox per gestire il rispetto delle proporzioni della carta.
			_checkBoxKeepAspect = new CheckBox();

			Binding bind = new Binding();
			// bind.Mode = BindingMode.TwoWay;
			bind.Source = this;
			bind.Path = new PropertyPath( "MantainAspectRatio" );

			_checkBoxKeepAspect.DataContext = this;
			_checkBoxKeepAspect.SetBinding( CheckBox.IsCheckedProperty, bind );
			_checkBoxKeepAspect.IsChecked = true;  // non so perché ma il binding non funziona alla prima battuta. Io imposto la property ma non si illumina la checkbox. Allora lo forzo io.
			_checkBoxKeepAspect.ToolTip = "Mantieni proporzioni";
			_cnvThumbs.Children.Add( _checkBoxKeepAspect );
			//

			// Linee
			_lineVert = new Line();
			_lineVert.StrokeThickness = 1;
			_lineVert.Stroke = System.Windows.Media.Brushes.Red;
			_lineVert.StrokeDashArray = new DoubleCollection() { 4, 5 };
			_cnvThumbs.Children.Add( _lineVert );
			//
			_lineHoriz = new Line();
			_lineHoriz.Stroke = _lineVert.Stroke;
			_lineHoriz.StrokeThickness = _lineVert.StrokeThickness;
			_lineHoriz.StrokeDashArray = _lineVert.StrokeDashArray;
			_cnvThumbs.Children.Add( _lineHoriz );
			//

			// We have to keep the clipping interior withing the bounds of the adorned element
			// so we have to track it's size to guarantee that...
			FrameworkElement fel = adornedElement as FrameworkElement;

			if (fel != null)
			{
				fel.SizeChanged += new SizeChangedEventHandler(AdornedElement_SizeChanged);
			}
		}

		#endregion

		#region Thumb handlers
		// Generic handler for Cropping
		private void HandleThumb(
			double drcL,
			double drcT,
			double drcW,
			double drcH,
			double dx,
			double dy)
		{
			Rect rcInterior = _prCropMask.RectInterior;

			bool stoMuovendoInOrizzontale = (drcL != 0 || drcW != 0) && dx != 0;
			bool stoMuovendoInVerticale = (drcT != 0 || drcH != 0) && dy != 0;


			if (rcInterior.Width + drcW * dx < 0)
			{
				dx = -rcInterior.Width / drcW;
			}

			if (rcInterior.Height + drcH * dy < 0)
			{
				dy = -rcInterior.Height / drcH;
			}

			rcInterior = new Rect(
				rcInterior.Left + drcL * dx,
				rcInterior.Top + drcT * dy,
				rcInterior.Width + drcW * dx,
				rcInterior.Height + drcH * dy);



			if( 1==0 || MantainAspectRatio ) {
				if( stoMuovendoInOrizzontale || stoMuovendoInVerticale ) {
					KeepAspectRatio( ref rcInterior, stoMuovendoInOrizzontale );
				}
			}


			_prCropMask.RectInterior = rcInterior;
			SetThumbs(_prCropMask.RectInterior);
			RaiseEvent( new RoutedEventArgs(CropChangedEvent, this));
		}

		/// <summary>
		/// Se richiesto, devo forzare il rettangolo di crop a mantenere le proporzioni
		/// </summary>
		/// <param name="rect">E' il rettangolo tratteggiato che rappresenta l'area di crop</param>
		private void KeepAspectRatio( ref Rect rect, bool stoMuovendoInOrizzontale ) {

			if( AspectRatio <= 0f )
				throw new ArgumentOutOfRangeException( "aspect ratio not valid : " + AspectRatio );


			// Controllo sforamento del minimo. Succede di brutto.
			if( rect.Top < 0 )
				rect.Y = 0;
			if( rect.Left < 0 )
				rect.X = 0;

			if( stoMuovendoInOrizzontale ) {
				// -- tengo buona la larghezza
				rect.Height = ControlloPerimetroW(rect.Width, rect.Left) / AspectRatio;
        
				// -- conrollo di non sforare il massimo
		        double testSforaH;
				testSforaH = ControlloPerimetroH( rect.Height, rect.Top );
				if( testSforaH < rect.Height ) {
					rect.Height = testSforaH;
					rect.Width = rect.Height * AspectRatio;
				}

			} else {
				// -- tengo buona l'altezza
				rect.Width = ControlloPerimetroH(rect.Height, rect.Top) * AspectRatio;
    
				// -- conrollo di non sforare il massimo
				double testSforaW;
				testSforaW = ControlloPerimetroW( rect.Width, rect.Left );
				if( testSforaW != rect.Width ) {
					rect.Width = testSforaW;
					rect.Height = rect.Width / AspectRatio;
				}
			}

			

			// Faccio qualche controllo di sicurezza, ma soltanto se sono compilato in debug.
			System.Diagnostics.Debug.Assert( rect.Left >= 0 );
			System.Diagnostics.Debug.Assert( rect.Top >= 0 );
			System.Diagnostics.Debug.Assert( rect.Width > 0 );	
			System.Diagnostics.Debug.Assert( rect.Width <= AdornedElement.RenderSize.Width );
			System.Diagnostics.Debug.Assert( rect.Height > 0 );
			System.Diagnostics.Debug.Assert( rect.Height <= AdornedElement.RenderSize.Height );
			System.Diagnostics.Debug.Assert( Math.Abs( ((rect.Width / rect.Height) - AspectRatio) ) < 0.0000001 );
		}

		private double ControlloPerimetroH( double newH, double newT ) {
			if( newT + newH > AdornedElement.RenderSize.Height )
				newH = AdornedElement.RenderSize.Height - newT;
			return newH;
		}

		private double ControlloPerimetroW( double newW, double newL ) {
			if( newL + newW > AdornedElement.RenderSize.Width )
				newW = AdornedElement.RenderSize.Width - newL;
			return newW;
		}

		private double ControlloPerimetroL( double newL, double newW ) {
			if( newL < 0 )
				newL = 0;
			else if( newL + newW > AdornedElement.RenderSize.Width )
				newL = AdornedElement.RenderSize.Width - newW;
			return newL;
		}

		private double ControlloPerimetroT( double newT, double newH ) {
			if( newT < 0 )
				newT = 0;
			else if( newT + newH > AdornedElement.RenderSize.Height )
				newT = AdornedElement.RenderSize.Height - newH;
			return newT;
		}

		// Handler for Cropping from the bottom-left.
		private void HandleBottomLeft(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					1, 0, -1, 1,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the bottom-right.
		private void HandleBottomRight(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					0, 0, 1, 1,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the top-right.
		private void HandleTopRight(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					0, 1, 1, -1,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the top-left.
		private void HandleTopLeft(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					1, 1, -1, -1,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the top.
		private void HandleTop(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					0, 1, 0, -1,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the left.
		private void HandleLeft(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					1, 0, -1, 0,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the right.
		private void HandleRight(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					0, 0, 1, 0,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the bottom.
		private void HandleBottom(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					0, 0, 0, 1,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}
		#endregion

		#region Other handlers
		private void AdornedElement_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			FrameworkElement fel = sender as FrameworkElement;
			Rect rcInterior = _prCropMask.RectInterior;
			bool fFixupRequired = false;
			double
				intLeft = rcInterior.Left,
				intTop = rcInterior.Top,
				intWidth = rcInterior.Width,
				intHeight = rcInterior.Height;

			if (rcInterior.Left > fel.RenderSize.Width)
			{
				intLeft = fel.RenderSize.Width;
				intWidth = 0;
				fFixupRequired = true;
			}

			if (rcInterior.Top > fel.RenderSize.Height)
			{
				intTop = fel.RenderSize.Height;
				intHeight = 0;
				fFixupRequired = true;
			}

			if (rcInterior.Right > fel.RenderSize.Width)
			{
				intWidth = Math.Max(0, fel.RenderSize.Width - intLeft);
				fFixupRequired = true;
			}

			if (rcInterior.Bottom > fel.RenderSize.Height)
			{
				intHeight = Math.Max(0, fel.RenderSize.Height - intTop);
				fFixupRequired = true;
			}
			if (fFixupRequired)
			{
				_prCropMask.RectInterior = new Rect(intLeft, intTop, intWidth, intHeight);
			}
		}

		void moveHandle_DragDelta( object sender, DragDeltaEventArgs e ) {

			Rect rcInterior = _prCropMask.RectInterior;

			double newL = rcInterior.Left + e.HorizontalChange;
			rcInterior.X = ControlloPerimetroL( newL, rcInterior.Width );

			double newT = rcInterior.Top + e.VerticalChange;
			rcInterior.Y = ControlloPerimetroT( newT, rcInterior.Height );


			_prCropMask.RectInterior = rcInterior;
			SetThumbs( _prCropMask.RectInterior );
			RaiseEvent( new RoutedEventArgs( CropChangedEvent, this ) );
		}

		#endregion

		#region Arranging/positioning
		private void SetThumbs(Rect rc)
		{
			_crtBottomRight.SetPos(rc.Right, rc.Bottom);
			_crtTopLeft.SetPos(rc.Left, rc.Top);
			_crtTopRight.SetPos(rc.Right, rc.Top);
			_crtBottomLeft.SetPos(rc.Left, rc.Bottom);
			_crtTop.SetPos(rc.Left + rc.Width / 2, rc.Top);
			_crtBottom.SetPos(rc.Left + rc.Width / 2, rc.Bottom);
			_crtLeft.SetPos(rc.Left, rc.Top + rc.Height / 2);
			_crtRight.SetPos(rc.Right, rc.Top + rc.Height / 2);

			// maniglia centrale per spostare l'area
			_moveHandle.SetPos(   rc.Left+(rc.Width/2),  rc.Top+(rc.Height/2) );

			// Check Box per sganciare la proporzione
			Canvas.SetLeft( _checkBoxKeepAspect, rc.Left + (rc.Width / 2) - (_checkBoxKeepAspect.ActualWidth / 2) );
			Canvas.SetTop( _checkBoxKeepAspect, rc.Top + (rc.Height / 4) - (_checkBoxKeepAspect.ActualHeight / 2) );

			// righe
			_lineHoriz.X1 = rc.Left;
			_lineHoriz.Y1 = rc.Top + (rc.Height / 2);
			_lineHoriz.X2 = rc.Right;
			_lineHoriz.Y2 = _lineHoriz.Y1;


			_lineVert.X1 = rc.Left + rc.Width / 2;
			_lineVert.Y1 = rc.Top;
			_lineVert.X2 = _lineVert.X1;
			_lineVert.Y2 = rc.Bottom;

		}

		// Arrange the Adorners.
		protected override Size ArrangeOverride(Size finalSize)
		{
			Rect rcExterior = new Rect(0, 0, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height);
			_prCropMask.RectExterior = rcExterior;
			Rect rcInterior = _prCropMask.RectInterior;
			_prCropMask.Arrange(rcExterior);

			SetThumbs(rcInterior);
			_cnvThumbs.Arrange(rcExterior);
			return finalSize;
		}
		#endregion

		#region Public interface
		public BitmapSource BpsCrop()
		{
			Thickness margin = AdornerMargin();
			Rect rcInterior = _prCropMask.RectInterior;

			Point pxFromSize = UnitsToPx(rcInterior.Width, rcInterior.Height);

			// It appears that CroppedBitmap indexes from the upper left of the margin whereas RenderTargetBitmap renders the
			// control exclusive of the margin.  Hence our need to take the margins into account here...

			Point pxFromPos = UnitsToPx(rcInterior.Left + margin.Left, rcInterior.Top + margin.Top);
			Point pxWhole = UnitsToPx(AdornedElement.RenderSize.Width + margin.Left, AdornedElement.RenderSize.Height + margin.Left);
			pxFromSize.X = Math.Max(Math.Min(pxWhole.X - pxFromPos.X, pxFromSize.X), 0);
			pxFromSize.Y = Math.Max(Math.Min(pxWhole.Y - pxFromPos.Y, pxFromSize.Y), 0);
			if (pxFromSize.X == 0 || pxFromSize.Y == 0)
			{
				return null;
			}
			System.Windows.Int32Rect rcFrom = new System.Windows.Int32Rect(pxFromPos.X, pxFromPos.Y, pxFromSize.X, pxFromSize.Y);

			RenderTargetBitmap rtb = new RenderTargetBitmap(pxWhole.X, pxWhole.Y, s_dpiX, s_dpiY, PixelFormats.Default);
			rtb.Render(AdornedElement);
			return new CroppedBitmap(rtb, rcFrom);
		}
		#endregion

		#region Helper functions
		private Thickness AdornerMargin()
		{
			Thickness thick = new Thickness(0);
			if (AdornedElement is FrameworkElement)
			{
				thick = ((FrameworkElement)AdornedElement).Margin;
			}
			return thick;
		}

		private void BuildCorner(ref CropThumb crt, Cursor crs)
		{
			if (crt != null) return;

			crt = new CropThumb(_cpxThumbWidth);

			// Set some arbitrary visual characteristics.
			crt.Cursor = crs;

			_cnvThumbs.Children.Add(crt);
		}

		private Point UnitsToPx(double x, double y)
		{
			return new Point((int)(x * s_dpiX / 96), (int)(y * s_dpiY / 96));
		}
		#endregion

		#region Visual tree overrides
		// Override the VisualChildrenCount and GetVisualChild properties to interface with 
		// the adorner's visual collection.
		protected override int VisualChildrenCount { get { return _vc.Count; } }
		protected override Visual GetVisualChild(int index) { return _vc[index]; }
		#endregion

		#region Internal Classes
		class CropThumb : Thumb
		{
			#region Private variables
			int _cpx;
			#endregion

			#region Constructor
			internal CropThumb(int cpx)
				: base()
			{
				_cpx = cpx;
			}
			#endregion

			#region Overrides
			protected override Visual GetVisualChild(int index)
			{
				return null;
			}

			protected override void OnRender(DrawingContext drawingContext)
			{
				drawingContext.DrawRoundedRectangle(Brushes.White, new Pen(Brushes.Black, 1), new Rect(new Size(_cpx, _cpx)), 1, 1);
			}
			#endregion

			#region Positioning
			internal void SetPos(double x, double y)
			{
				Canvas.SetTop(this, y - _cpx / 2);
				Canvas.SetLeft(this, x - _cpx / 2);
			}
			#endregion
		}
		#endregion
	}

}

