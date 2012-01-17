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
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.UI {

	/// <summary>
	/// Interaction logic for ScaricatoreFoto.xaml
	/// </summary>
	public partial class ScaricatoreFoto : UserControl {

		ScaricatoreFotoViewModel _scaricatoreViewModel;

		private ParamScarica _paramScarica;

		public ScaricatoreFoto() {

			InitializeComponent();

			_scaricatoreViewModel = (ScaricatoreFotoViewModel) this.DataContext;

			paramScarica = new ParamScarica();
			paramScarica.flashCardConfig = new FlashCardConfig();

		}


		public ParamScarica paramScarica {
			get;
			set;
		}


		private void button1_Click( object sender, RoutedEventArgs e ) {

			Console.WriteLine( "stop" );
		}


	}
}
