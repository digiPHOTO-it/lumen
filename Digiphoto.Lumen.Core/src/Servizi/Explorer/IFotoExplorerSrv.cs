using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using System.Collections;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Servizi.Explorer {


	public interface IFotoExplorerSrv : IServizio {

		/** Queste sono tutte le foto estratte dall'archivio */
		List<Fotografia> fotografie { get; }

		/** Questa è la fotografia corrente */
		Fotografia fotoCorrente { get; set; }

		// Cerca le foto nell'archivio e le carica in memoria.
		void cercaFoto( ParamCercaFoto param );

	}
}
