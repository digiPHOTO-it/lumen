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
using System.Collections;

namespace Digiphoto.Lumen.UI
{
	/// <summary>
	/// Interaction logic for AzioniRapideView.xaml
	/// </summary>
	public partial class SelettoreAzioniRapide : UserControlBase
	{
		public SelettoreAzioniRapide()
		{
			InitializeComponent();

			DataContextChanged += new DependencyPropertyChangedEventHandler(selettoreAzioniRapideView_DataContextChanged);
		}

		void selettoreAzioniRapideView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			associaDialogProvider();
		}

		SelettoreAzioniRapideViewModel viewModel {
			get {
				return (SelettoreAzioniRapideViewModel) this.DataContext;
			}
		}

	}
}
