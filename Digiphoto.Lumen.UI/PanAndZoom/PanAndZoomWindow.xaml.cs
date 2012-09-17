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
using System.Windows.Shapes;

namespace Digiphoto.Lumen.UI.PanAndZoom {

	/// <summary>
	/// Interaction logic for PanAndZoomWindow.xaml
	/// </summary>
	public partial class PanAndZoomWindow : Window {
		
		public PanAndZoomWindow() {
			InitializeComponent();;
		}

		private void Window_KeyUp( object sender, KeyEventArgs e ) {
			if( e.Key == Key.Escape )
				zoomViewer.Reset();
		}
	}
}
