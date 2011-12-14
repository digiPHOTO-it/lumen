using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core;

namespace Digiphoto.Lumen.Servizi.Ricerca {

	interface IRicercatoreSrv : IServizio {

		List<Fotografia> cerca( ParamCercaFoto param );

	}
}
