using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using System.Collections;
using Digiphoto.Lumen.Model;
using System.ComponentModel;
using Digiphoto.Lumen.Comandi;
using Digiphoto.Lumen.Servizi.Ricerca;

namespace Digiphoto.Lumen.Servizi.Explorer {


	public interface IFotoExplorerSrv : IServizio {

		/** Queste sono tutte le foto estratte dall'archivio */
		BindingList<Fotografia> fotografie { get; }

		/** Questa è la fotografia corrente */
		Fotografia fotoCorrente { get; set; }

		/** invoca l'operazione richiesta */
		void invoca( Comando comandoUtente, Target target );

		// Cerca le foto nell'archivio e le carica in memoria.
		void cercaFoto( ParamRicercaFoto param );
	}
}
