using Digiphoto.Lumen.SelfService.SlideShow.Config;
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
using System.Windows.Shapes;

namespace Digiphoto.Lumen.SelfService.SlideShow.Preferenze {
	/// <summary>
	/// Interaction logic for PreferenzeWindow.xaml
	/// </summary>
	public partial class PreferenzeWindow : Window {

		public bool confermato { get; private set; }

		public PreferenzeWindow() {

			InitializeComponent();

			confermato = false;
		}

		private void SalvareButton_Click( object sender, RoutedEventArgs e ) {
			confermato = true;
			this.Hide();
		}

		private void AnnullareButton_Click( object sender, RoutedEventArgs e ) {
			this.Hide();
		}
	}
}
