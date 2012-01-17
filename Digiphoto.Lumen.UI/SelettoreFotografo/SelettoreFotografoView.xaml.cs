using System.Windows;
using System.Windows.Controls;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.UI {

	public partial class SelettoreFotografo : UserControl {

		private SelettoreFotografoViewModel _sceltafotografoViewModel;

		public SelettoreFotografo() {

			InitializeComponent();

			_sceltafotografoViewModel = (SelettoreFotografoViewModel) this.DataContext;
		}

		public Fotografo fotografoSelezionato {
			get {
				return _sceltafotografoViewModel.fotografoSelezionato;
			}
		}

		#region possoCreare Dependency Property
		public static readonly DependencyProperty possoCreareProperty = DependencyProperty.Register( "possoCreare", typeof( bool ),	typeof( SelettoreFotografo ), new FrameworkPropertyMetadata( true,	FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

		public bool possoCreare {
			get {
				return (bool)GetValue( possoCreareProperty );
			}
			set {
				SetValue( possoCreareProperty, value );
			}
		}
		#endregion

	}
}
