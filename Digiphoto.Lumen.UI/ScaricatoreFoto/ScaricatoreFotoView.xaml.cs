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

			_scaricatoreViewModel = new ScaricatoreFotoViewModel();
			DataContext = _scaricatoreViewModel;

			paramScarica = new ParamScarica();
			paramScarica.flashCardConfig = new FlashCardConfig();
			
		}


		public ParamScarica paramScarica {
			get;
			set;
		}

		public Fotografo _fotografo;
		public Fotografo fotografo {
			get {
				return _fotografo;
			}
			set {
				_fotografo = value;
				// assegno anche la proprietà del parametro
				paramScarica.flashCardConfig.idFotografo = _fotografo.id;
			}
		}

		private void button1_Click( object sender, RoutedEventArgs e ) {

			string appo = this.selettoreFotografo1.fotografoSelezionato.cognomeNome;
		}


	}
}
