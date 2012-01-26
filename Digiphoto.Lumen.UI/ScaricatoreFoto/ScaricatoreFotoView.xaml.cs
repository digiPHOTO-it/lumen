using System.Windows.Controls;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.UI {

	/// <summary>
	/// Interaction logic for ScaricatoreFoto.xaml
	/// </summary>
	public partial class ScaricatoreFoto : UserControl {

		ScaricatoreFotoViewModel _scaricatoreViewModel;


		public ScaricatoreFoto() {

			InitializeComponent();
			
			_scaricatoreViewModel = (ScaricatoreFotoViewModel) this.DataContext;
		}

	}
}
