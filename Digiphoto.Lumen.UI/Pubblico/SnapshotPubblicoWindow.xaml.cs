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

namespace Digiphoto.Lumen.UI.Pubblico {
	/// <summary>
	/// Interaction logic for ClonePubblicoWindow.xaml
	/// </summary>
	public partial class SnapshotPubblicoWindow : Window {

		private SnapshotPubblicoViewModel _snapshotPubblicoViewModel;

		public SnapshotPubblicoWindow() {
			InitializeComponent();

			// creo ed associo il datacontext
			_snapshotPubblicoViewModel = new SnapshotPubblicoViewModel();
			this.DataContext = _snapshotPubblicoViewModel;

			// When the ViewModel asks to be closed, 
			// close the window.
			EventHandler handler = null;
			handler = delegate {
				_snapshotPubblicoViewModel.RequestClose -= handler;
				_snapshotPubblicoViewModel.Dispose();
				this.DataContext = null;
				this.Close();
			};
			_snapshotPubblicoViewModel.RequestClose += handler;
		}

	}
}
