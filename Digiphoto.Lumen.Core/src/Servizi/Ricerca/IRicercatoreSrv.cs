using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core;

namespace Digiphoto.Lumen.Servizi.Ricerca {

	public class ParamRicercaFoto {

		public Evento [] eventi { get; set; }
		public Fotografo [] fotografi { get; set; }

		public string didascalia { get; set; }
		public FaseDelGiorno [] fasiDelGiorno { get; set; }
		public int [] numeriFotogrammi {get; set; }

		public DateTime? giornataIniz { get; set; }
		public DateTime? giornataFine { get; set; }

		#region Paginazione

		/** Numero di record da saltare nel risultato. Se NULL allora niente */
		public long? paginazioneSkip { get; set; }

		/** Numero di record di ampiezza della paginazione. Se NULL allora ninente */
		public int? paginazioneLimit { get; set;
		}

		#endregion
	}

	interface IRicercatoreSrv {

		List<Fotografia> cerca( ParamRicercaFoto param );

	}
}
