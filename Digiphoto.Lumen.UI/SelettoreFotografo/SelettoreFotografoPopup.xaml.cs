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

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for SelettoreFotografoWindow.xaml
	/// </summary>
	public partial class SelettoreFotografoPopup : Window {

		public SelettoreFotografoPopup() {
			InitializeComponent();

			// Mi creo e mi associo il datacontext per eventuali bindings
//			this.DataContext = new SelettoreFotografoPopupViewModel();

		}


		private void okButton_Click( object sender, RoutedEventArgs e ) {
			this.DialogResult = true;
		}


	}
}
