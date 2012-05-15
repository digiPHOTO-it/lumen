﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using System.Collections;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Core;

namespace Digiphoto.Lumen.Servizi.Explorer {


	public interface IFotoExplorerSrv : IServizio {

		/** Queste sono tutte le foto estratte dall'archivio */
		List<Fotografia> fotografie { get; }

		/** Questa è la fotografia corrente */
		Fotografia fotoCorrente { get; set; }

		// Cerca le foto nell'archivio e le carica in memoria.
		void cercaFoto( ParamCercaFoto param );

		// Questa funzionalità sarebbe utile anche in altri servizi.
		// Forse andrebbe messo nel servizio FotografiaEntityRepository, che però attualmente non esiste. 
		// Quindi lo appoggio qui perché ora mi serve qui.
		// Eventualmente servire anche nel fotoritocco, lo sposterò più avanti.
		void modificaMetadatiFotografie( IEnumerable<Fotografia> fotografie, MetadatiFoto metadati );
	}

	public class MetadatiFoto {

		public string didascalia { set; get; }
		public Evento evento { set; get; }
		public FaseDelGiorno? faseDelGiorno { set; get; }

		public bool isEmpty() {
			return didascalia == null && evento == null && faseDelGiorno == null;
		}
	}
}
