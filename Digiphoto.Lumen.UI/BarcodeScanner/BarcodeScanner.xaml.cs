using Digiphoto.Lumen.UI.Mvvm;
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

namespace Digiphoto.Lumen.UI.BarcodeScanner {

	/// <summary>
	/// Interaction logic for BarcodeScanner.xaml
	/// </summary>
	public partial class BarcodeScanner : UserControlBase {

		public BarcodeScanner() {
			InitializeComponent();
		}
		
		private BarcodeScannerViewModel viewModel {
			get {
				return this.DataContext as BarcodeScannerViewModel;
			}
		}

	}
}
