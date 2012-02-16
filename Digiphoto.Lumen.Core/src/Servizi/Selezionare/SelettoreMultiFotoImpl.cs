using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Servizi.Selezionare {

	public abstract class SelettoreMultiFotoImpl : ServizioImpl, ISelettoreMultiFotoSrv {

		public abstract IEnumerable<Fotografia> tutteLeFoto {
			get;
		}

		public Fotografia fotoCorrente {
			get;
			set;
		}

		public IEnumerable<Fotografia> fotoSelezionate {
			get {
				var querySelezionate = from ff in tutteLeFoto
									   where ff.isSelezionata == true
									   select ff;
				return querySelezionate;
			}

		}
	}
}
