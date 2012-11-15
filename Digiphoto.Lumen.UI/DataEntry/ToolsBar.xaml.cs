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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Digiphoto.Lumen.UI.Mvvm;

namespace Digiphoto.Lumen.UI.DataEntry
{
	/// <summary>
	/// Interaction logic for BarraAzioni.xaml
	/// </summary>
	public partial class ToolsBar : UserControlBase
	{
		#region possoSalvare Dependency Property

		public static readonly DependencyProperty possoSalvareProperty = DependencyProperty.Register("possoSalvare", typeof(bool), typeof(ToolsBar), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None));

		public bool possoSalvare
		{
			get
			{
				return (bool)GetValue(possoSalvareProperty);
			}
			set
			{
				SetValue(possoSalvareProperty, value);
			}
		}

		#endregion possoSalvare

		public ToolsBar()
		{
			InitializeComponent();
		}
	}
}
