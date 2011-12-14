using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Stampare;

namespace Digiphoto.Lumen.Servizi.Vendere {

	public enum ModoVendita : short {
		Carrello = 0,
		StampaDiretta = 1
	}

	/**
	 * Questo servizio funge da Gestore di Carrello ed anche come facade per il servizio IMasterizzaSrv
	 */
	public interface IVenditoreSrv : IServizio {

		ModoVendita modoVendita { get; set; }

		/** Foto da vendere come files */
		void aggiungiMasterrizzate( IList<Fotografia> fotografie );

		/** Foto da vendere come stampe */
		void aggiungiStampe( IList<Fotografia> fotografie, ParamStampaFoto param );

		/** 
		 * Prepara un nuovo carrello vuoto (da riempire).
		 * Il carrello corrente viene "abbandonato" senza nessun salvataggio.
		 */
		void creaNuovoCarrello();

		/**
		 * Il carrello corrente viene venduto. Diventa definitivo
		 */
		void confermaCarrello();

		void abbandonaCarrello();

		/** Lavoro con un carrello alla volta. Esiste un solo carrello "corrente". */
		Carrello carrello { get; }

	}
}
