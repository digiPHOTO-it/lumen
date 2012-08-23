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
using Digiphoto.Lumen.UI.Adorners;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.Mvvm;

namespace Digiphoto.Lumen.UI.Pubblico
{
	/// <summary>
	/// Interaction logic for GestSlideShowView.xaml
	/// </summary>
	public partial class GestSlideShowView : UserControlBase
	{

		AdornerLayer aLayer;

		bool _isDown;
		bool _isDragging;
		bool selected = false;
		UIElement selectedElement = null;

		Point _startPoint;
		private double _originalLeft;
		private double _originalTop;

		private UserConfigLumen cfg = Configurazione.UserConfigLumen;

		public GestSlideShowView()
		{
			InitializeComponent();
		}

		#region Proprieta

		private GestSlideShowViewModel gestSlideShowViewModel
		{
			get
			{
				return (GestSlideShowViewModel)base.viewModelBase;
			}
		}

		#endregion

		private void Monitor_Loaded(object sender, RoutedEventArgs e)
		{
			this.MouseLeftButtonDown += new MouseButtonEventHandler(Monitor_MouseLeftButtonDown);
			this.MouseLeftButtonUp += new MouseButtonEventHandler(DragFinishedMouseHandler);
			this.MouseMove += new MouseEventHandler(Monitor_MouseMove);
			this.MouseLeave += new MouseEventHandler(Monitor_MouseLeave);

			monitor.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(myCanvas_PreviewMouseLeftButtonDown);
			monitor.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DragFinishedMouseHandler);
		}

		// Handler for drag stopping on leaving the window
		void Monitor_MouseLeave(object sender, MouseEventArgs e)
		{
			StopDragging();
			e.Handled = true;
		}

		// Handler for drag stopping on user choise
		void DragFinishedMouseHandler(object sender, MouseButtonEventArgs e)
		{
			StopDragging();
			e.Handled = true;
		}

		// Method for stopping dragging
		private void StopDragging()
		{
			if (_isDown)
			{
				_isDown = false;
				_isDragging = false;
			}
		}

		// Hanler for providing drag operation with selected element
		void Monitor_MouseMove(object sender, MouseEventArgs e)
		{
			if (_isDown)
			{
				if ((_isDragging == false) &&
					((Math.Abs(e.GetPosition(monitor).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
					(Math.Abs(e.GetPosition(monitor).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
					_isDragging = true;

				if (_isDragging)
				{
					Point position = Mouse.GetPosition(monitor);
					Canvas.SetTop(selectedElement, position.Y - (_startPoint.Y - _originalTop));
					Canvas.SetLeft(selectedElement, position.X - (_startPoint.X - _originalLeft));
				}
			}
		}

		// Handler for clearing element selection, adorner removal
		void Monitor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (selected)
			{
				selected = false;
				if (selectedElement != null)
				{
					aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
					selectedElement = null;
				}
			}
		}

		// Handler for element selection on the canvas providing resizing adorner
		void myCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			// Remove selection on clicking anywhere the window
			if (selected)
			{
				selected = false;
				if (selectedElement != null)
				{
					// Remove the adorner from the selected element
					if (aLayer!=null && aLayer.GetAdorners(selectedElement)!=null)
					{
						aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
						selectedElement = null;
					}
				}
			}

			// If any element except canvas is clicked, 
			// assign the selected element and add the adorner
			if (e.Source != monitor)
			{
				_isDown = true;
				_startPoint = e.GetPosition(monitor);

				selectedElement = e.Source as UIElement;

				_originalLeft = Canvas.GetLeft(selectedElement);
				_originalTop = Canvas.GetTop(selectedElement);

				aLayer = AdornerLayer.GetAdornerLayer(selectedElement);
				aLayer.Add(new ResizingAdorner(selectedElement));
				selected = true;
				e.Handled = true;
			}
		}
	}
}
