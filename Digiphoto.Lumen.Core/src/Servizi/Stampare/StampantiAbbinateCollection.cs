using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using  System.Data.Entity.Core.Objects;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Config;
using System.Collections.ObjectModel;

namespace Digiphoto.Lumen.Servizi.Stampare {

	public class StampantiAbbinateCollection : ObservableCollection<StampanteAbbinata> {

		public StampantiAbbinateCollection( string strAbbinate ) : base( StampantiAbbinateUtil.deserializzaList(strAbbinate) ) {
		}

		public StampantiAbbinateCollection( List<StampanteAbbinata> lista ) : base( lista ) {
		}

		public StampantiAbbinateCollection( IEnumerable<StampanteAbbinata> enumera ) : base( enumera ) {
		}

		public string serializzaToString() {
			return StampantiAbbinateUtil.serializzaToString( this );
        }
    }
}
