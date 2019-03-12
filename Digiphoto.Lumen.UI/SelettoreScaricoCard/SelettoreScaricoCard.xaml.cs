using Digiphoto.Lumen.UI.Mvvm;

namespace Digiphoto.Lumen.UI {

	/// <summary>
	/// Interaction logic for SelettoreScaricoCard.xaml
	/// </summary>
	public partial class SelettoreScaricoCard : UserControlBase {

		public SelettoreScaricoCard() {
			InitializeComponent();

			this.DataContextChanged += SelettoreScaricoCard_DataContextChanged;
		}

		private void SelettoreScaricoCard_DataContextChanged( object sender, System.Windows.DependencyPropertyChangedEventArgs e ) {
			((SelettoreScaricoCardViewModel)this.DataContext).dialogProvider = this;
		}
	}
}
