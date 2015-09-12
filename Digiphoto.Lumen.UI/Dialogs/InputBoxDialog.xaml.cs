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
using System.Collections;

namespace Digiphoto.Lumen.UI.Dialogs {
	/// <summary>
	/// Interaction logic for InputBoxDialog.xaml
	/// </summary>
	public partial class InputBoxDialog : Window {

		public InputBoxDialog()
		{
			
			InitializeComponent();

		}

		private void buttonOk_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			this.Hide();
		}

		private void buttonAnnulla_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Hide();
		}
	}
}
