using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.OnRide.UI.Model {

	class FotoItem {

		public FileInfo fileInfo {
			get; set;
		}

		public string tag {
			get; set;
		}

		public bool daTaggare {
			get; set;
		}

		public bool daEliminare {
			get; set;
		}

		public string nomeFileTag {
			get {
				return fileInfo == null ? null : fileInfo.FullName + ".tag.txt";
			}
		}

	}
}
