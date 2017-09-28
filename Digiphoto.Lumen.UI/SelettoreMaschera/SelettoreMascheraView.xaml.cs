using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Util;
using System.Windows;
using System.Windows.Controls;

namespace Digiphoto.Lumen.UI.SelettoreMaschera {

	/// <summary>
	/// Interaction logic for SelettoreMaschera.xaml
	/// </summary>
	public partial class SelettoreMaschera : UserControlBase {

		#region Dependency Property

		public static readonly DependencyProperty filtroProperty = DependencyProperty.Register( "filtro", typeof( FiltroMask ), typeof( SelettoreMaschera ), new FrameworkPropertyMetadata( FiltroMask.MskSingole, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

		public FiltroMask filtro {
			get {
				return (FiltroMask)GetValue( filtroProperty );
			}
			set {
				SetValue( filtroProperty, value );
			}
		}

		#endregion Dependency Property

		public SelettoreMaschera() {
			InitializeComponent();
		}

	}
}
