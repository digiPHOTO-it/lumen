using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Digiphoto.Lumen.GestoreConfigurazione.UI.Util {

	public class UtilWinForm {
		public static string scegliCartella() {
			string cartella = null;

			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.ShowNewFolderButton = true;
			DialogResult result = dlg.ShowDialog();
			if( result == System.Windows.Forms.DialogResult.OK )
				cartella = dlg.SelectedPath;

			return cartella;
		}
	}
}
