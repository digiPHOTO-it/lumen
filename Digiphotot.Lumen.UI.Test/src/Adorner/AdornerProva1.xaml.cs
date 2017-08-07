using Digiphoto.Lumen.UI.Adorners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Digiphoto.Lumen.UI.Test.Adorner {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class AdornerProva1 : Window {
		public AdornerProva1() {
			InitializeComponent();


		}

		private void Window_Loaded( object sender, RoutedEventArgs e ) {
			AdornerLayer adornerlayer = AdornerLayer.GetAdornerLayer( vb2 );
			ComposingAdorner2 adorner = new ComposingAdorner2( vb2 );
			adornerlayer.Add( adorner );
		}
	}
}
