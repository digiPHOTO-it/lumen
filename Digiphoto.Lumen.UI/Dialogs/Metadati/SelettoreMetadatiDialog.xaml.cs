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
using System.Windows.Shapes;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Collections;

namespace Digiphoto.Lumen.UI.Dialogs
{
	/// <summary>
	/// Interaction logic for SelettoreMetadatiDialog.xaml
	/// </summary>
	public partial class SelettoreMetadatiDialog : Window
	{
		private IEnumerable<Fotografia> listaFoto;

		public SelettoreMetadatiDialog( IEnumerable<Fotografia> listaFoto ) {

			InitializeComponent();

			this.listaFoto = listaFoto;

			SelettoreMetadatiViewModel selettoreMetadatiViewModel = new SelettoreMetadatiViewModel2( listaFoto );
			this.selettoreMetadati.DataContext = selettoreMetadatiViewModel;
		}

	}
}
