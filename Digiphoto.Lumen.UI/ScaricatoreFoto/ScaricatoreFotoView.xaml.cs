using System.Windows.Controls;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.Mvvm;

namespace Digiphoto.Lumen.UI {

	/// <summary>
	/// Interaction logic for ScaricatoreFoto.xaml
	/// </summary>
	public partial class ScaricatoreFoto : UserControlBase {

		public ScaricatoreFoto() {
			InitializeComponent();

			this.DataContextChanged += ScaricatoreFoto_DataContextChanged;
		}

		private void ScaricatoreFoto_DataContextChanged( object sender, System.Windows.DependencyPropertyChangedEventArgs e ) {

			associaDialogProvider();

			selettoreCartella1.DataContext = scaricatoreFotoViewModel.selettoreCartellaViewModel;
		}

		protected ScaricatoreFotoViewModel scaricatoreFotoViewModel {
			get {
				return (ScaricatoreFotoViewModel)this.DataContext;
			}
		}

	}
}
