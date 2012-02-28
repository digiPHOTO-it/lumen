using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Util;
using System.IO;
using Digiphoto.Lumen.Applicazione;
using System.Windows.Media;


namespace Digiphoto.Lumen.UI.Pubblico {

	public class Slide {
		
		public ImageSource imgProvino {
			get;
			set;
		}

		public string etichetta {
			get;
			set;
		}
	}

	public class SlideShow {

		public int righe {
			get;
			set;
		}

		public int colonne {
			get;
			set;
		}

		public int millisecondiIntervallo {
			get;
			set;
		}

		public List<Slide> itemsShow {
			get;
			set;
		}

	}
}
