using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Digiphoto.Lumen.UI.Preferenze {
	/// <summary>
	/// Interaction logic for PreferenzeWindow.xaml
	/// </summary>
	public partial class PreferenzeWindow : Window {

		public PreferenzeWindow() {

			InitializeComponent();

			this.DataContextChanged += PreferenzeWindow_DataContextChanged;
		}

		private void PreferenzeWindow_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {

			PreferenzeViewModel vm = (PreferenzeViewModel)DataContext;

			// When the ViewModel asks to be closed, 
			// close the window.
			EventHandler handler = null;
			handler = delegate {
				vm.RequestClose -= handler;
				// this.DataContext = null;
				this.Close();
			};

			vm.RequestClose += handler;
		}


		private void proprietaMonitorButton_Click( object sender, RoutedEventArgs e ) {

			// String path = Environment.GetFolderPath( Environment.SpecialFolder.System );
			String exe = "rundll32.exe";
			String arguments = "shell32.dll,Control_RunDLL desk.cpl,,3";
			Process.Start( exe, arguments );
		}

	}
}
