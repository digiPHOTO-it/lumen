using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for SelettoreCartellaView.xaml
	/// </summary>
	public partial class SelettoreCartella : UserControl {

		private SelettoreCartellaViewModel _selettoreCartellaViewModel;


		public SelettoreCartella() {
			InitializeComponent();

			// Creo il *ViewModel e lo imosto come mio data-context
			_selettoreCartellaViewModel = new SelettoreCartellaViewModel();
			this.DataContext = _selettoreCartellaViewModel;
		}

	}
}
