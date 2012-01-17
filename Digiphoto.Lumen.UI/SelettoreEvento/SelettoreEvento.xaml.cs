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
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for SelettoreEvento.xaml
	/// </summary>
	public partial class SelettoreEvento : UserControl {

		private SelettoreEventoViewModel _selettoreEventoViewModel;


		public SelettoreEvento() {
			InitializeComponent();

			_selettoreEventoViewModel = (SelettoreEventoViewModel) this.DataContext;
		}

		public Evento eventoSelezionato {
			get {
				return _selettoreEventoViewModel.eventoSelezionato;
			}
		}
	}
}
