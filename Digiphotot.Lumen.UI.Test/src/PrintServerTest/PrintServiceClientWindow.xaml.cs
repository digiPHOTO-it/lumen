using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Stampare;
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

namespace Digiphoto.Lumen.UI.Test.src.PrintServerTest {
	/// <summary>
	/// Interaction logic for PrintServiceClientWindow.xaml
	/// </summary>
	public partial class PrintServiceClientWindow : Window {
		public PrintServiceClientWindow()
		{
			InitializeComponent();

		
			Guid fotografiaId = Guid.NewGuid();

			// psc.AccodaStampaFoto( fotografiaId );

		}
	}
}
