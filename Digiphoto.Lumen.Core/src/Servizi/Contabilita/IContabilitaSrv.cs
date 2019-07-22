using Digiphoto.Lumen.Servizi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Core.Servizi.Contabilita {

	public interface IContabilitaSrv : IServizio {

		/// <summary>
		/// Cerca quali date non sono state inserite.
		/// </summary>
		/// <returns></returns>
		List<DateTime> getListaGiorniNonChiusi();

	}
}
