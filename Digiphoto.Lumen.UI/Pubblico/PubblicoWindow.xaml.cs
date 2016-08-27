using Digiphoto.Lumen.Config;
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
using System.Windows.Shapes;

namespace Digiphoto.Lumen.UI.Pubblico {
	/// <summary>
	/// Interaction logic for PubblicoWindow.xaml
	/// </summary>
	public partial class PubblicoWindow : Window {
		public PubblicoWindow() {
			InitializeComponent();

			if( String.IsNullOrEmpty( Configurazione.infoFissa.descrizPuntoVendita ) )
				this.Title = "Photo Gallery - digiPHOTO Lumen";
			else
				this.Title = "Photo Gallery - " + Configurazione.infoFissa.descrizPuntoVendita;
			
		}
	}
}
