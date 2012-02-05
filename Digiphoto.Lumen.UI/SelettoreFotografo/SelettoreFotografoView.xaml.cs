using System.Windows;
using System.Windows.Controls;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm;

namespace Digiphoto.Lumen.UI {

	public partial class SelettoreFotografo : UserControlBase {

		public SelettoreFotografo() {

			InitializeComponent();
		}

		#region Proprietà

		public Fotografo fotografoSelezionato {
			get {
				return sceltafotografoViewModel.fotografoSelezionato;
			}
		}

		private SelettoreFotografoViewModel sceltafotografoViewModel {
			get {
				return (SelettoreFotografoViewModel)base.viewModelBase;
			}
		}

		#endregion

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
