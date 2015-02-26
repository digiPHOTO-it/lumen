using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Digiphoto.Lumen.UI
{
    /// <summary>
    /// Interaction logic for SelettoreStampantiInstallateView.xaml
    /// </summary>
    public partial class SelettoreStampanteInstallata : UserControl
    {
        public SelettoreStampanteInstallata()
        {
            InitializeComponent();
        }

		public static readonly DependencyProperty aprireCodaProperty = DependencyProperty.Register( "aprireCoda", typeof( bool ), typeof( SelettoreStampanteInstallata ), new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

		public bool aprireCoda {
			get {
				return (bool)GetValue( aprireCodaProperty );
			}
			set {
				SetValue( aprireCodaProperty, value );
			}
		}

    }
}
