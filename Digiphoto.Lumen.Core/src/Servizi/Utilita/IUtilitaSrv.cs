using Digiphoto.Lumen.Servizi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Core.Servizi.Utilita {

	public interface IUtilitaSrv : IServizio {

		/// <summary>
		/// Spedizione dei log in sede.
		/// Consente di trasmettere i log zippati.
		/// </summary>
		bool inviaLog();

	}
}
