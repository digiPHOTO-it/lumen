using System;
using System.Collections;
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
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;

namespace Digiphoto.Lumen.UI
{
    /// <summary>
	/// Interaction logic for SelettoreAzioniAutomatiche.xaml
    /// </summary>
    public partial class SelettoreAzioniAutomatiche : UserControlBase
    {

		public SelettoreAzioniAutomatiche()
		{
			InitializeComponent();

			this.DataContextChanged += SelettoreAzioniAutomatiche_DataContextChanged;
		}

		private void SelettoreAzioniAutomatiche_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {
			associaDialogProvider();
		}
	}
}
