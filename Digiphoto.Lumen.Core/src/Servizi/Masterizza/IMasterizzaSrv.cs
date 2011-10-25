using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using System.Collections;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Masterizza {

	public enum TipoDestinazione {
		MASTERIZZATORE,
		CARTELLA
	}

	/**
	 * Questo servizio implementa una coda di attesa di foto che devono essere copiate.
	 * Al momento della copia, occorre creare un carrello che registra l'incasso.
	 * Attenzione: le foto devono essere elaborate: non possiamo vendere i provini ma quelle buone.	
	 */
	public interface IMasterizzaSrv : IServizio, IList<Fotografia> {

		/** Aggiunge l'intero album alla lista */
		void add( Album album );

		/** L'utente deve scegliere dove copiare le foto.
		 * Se il tipo è CARTELLA, allora la destinazione è il nome della cartella.
		 * Se il tipo è MASTERIZZATORE allora la destinazione indica il 
		 * masterizzatore da usare (potrebbero essercene più di uno).
		 */
		void impostaDestinazione( TipoDestinazione tipoDestinazione, String destinazione );

		/** Il prezzo del disco masterizzato, viene sempre contrattato con il cliente */
		Carrello confermaVendita( Decimal prezzoForfettario );
	}
}
