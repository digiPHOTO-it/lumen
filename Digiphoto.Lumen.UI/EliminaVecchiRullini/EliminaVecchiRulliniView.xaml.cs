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
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.UI.EliminaVecchiRullini;

namespace Digiphoto.Lumen.UI.EliminaVecchiRullini
{
	/// <summary>
	/// Interaction logic for EliminaVecchiRulliniView.xaml
	/// </summary>
	public partial class EliminaVecchiRulliniView : UserControlBase
	{
		public EliminaVecchiRulliniView()
		{
			InitializeComponent();
		}

		private EliminaVecchiRulliniViewModel eliminaVecchiRulliniViewModel
		{
			get
			{
				return (EliminaVecchiRulliniViewModel)base.viewModelBase;
			}
		}
	}
}
