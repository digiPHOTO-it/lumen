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

namespace Digiphoto.Lumen.UI
{
    /// <summary>
	/// Interaction logic for SelettoreFormatoCarta.xaml
    /// </summary>
    public partial class SelettoreFormatoCarta : UserControlBase
    {
        public SelettoreFormatoCarta()
        {
            InitializeComponent();
        }

		private SelettoreFormatoCartaViewModel selettoreFormatoCartaViewModel {
			get {
				return (SelettoreFormatoCartaViewModel)base.viewModelBase;
			}
		}
    }
}
