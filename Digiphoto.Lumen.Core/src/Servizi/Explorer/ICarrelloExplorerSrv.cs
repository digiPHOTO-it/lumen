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


	public interface ICarrelloExplorerSrv : IServizio
	{

		/** Queste sono tutti i carrelli estratti dall'archivio */
		ICollection<Carrello> carrelli { get; }

		/** Questo è il carrello corrente */
		Carrello carrelloCorrente { get; set; }

		// Cerca i carrelli nell'archivio, vengono recuperati i soli carrelli non salvati.
		void cercaCarrello( ParamCercaCarrello param );

	}
}
