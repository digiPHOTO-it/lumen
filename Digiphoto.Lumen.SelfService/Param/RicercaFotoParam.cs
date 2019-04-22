using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.SelfService {

	public class RicercaFotoParam : PaginazParam {

		public string fotografoId {
			get; set;
		}

		public DateTime giorno {
			get;
			set;
		}
			
		public string faseDelGiorno {
			get; set;
		}

	}
}
