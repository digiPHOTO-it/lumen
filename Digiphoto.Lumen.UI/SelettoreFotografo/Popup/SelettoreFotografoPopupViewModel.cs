using Digiphoto.Lumen.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.UI {

	public class SelettoreFotografoPopupViewModel  : ClosableWiewModel {

		public IImmagine immagine {
			get;
			set;
		}

	}
}
