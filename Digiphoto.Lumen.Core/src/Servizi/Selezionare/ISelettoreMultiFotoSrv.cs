using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Servizi.Selezionare {
	
	/// <summary>
	/// Questa interfaccia mi permette di selezionare una o più foto e di eseguire dei comandi
	/// su di esse.
	/// </summary>
	public interface ISelettoreMultiFotoSrv : IServizio {

		Fotografia fotoCorrente {
			get;
		}

		IEnumerable<Fotografia> fotoSelezionate {
			get;
		}

	}
}
