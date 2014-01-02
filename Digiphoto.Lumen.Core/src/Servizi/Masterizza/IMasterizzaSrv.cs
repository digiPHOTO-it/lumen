using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using System.Collections;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Masterizzare {

	public enum TipoDestinazione {
		NULLA,
		MASTERIZZATORE,
		CARTELLA
	}

	/**
	 * Questo servizio implementa una coda di attesa di foto che devono essere copiate.
	 * Al momento della copia, occorre creare un carrello che registra l'incasso.
	 * Attenzione: le foto devono essere elaborate: non possiamo vendere i provini ma quelle buone.	
	 */
	public interface IMasterizzaSrv : IServizio {

		/** L'utente deve scegliere dove copiare le foto.
		 * Se il tipo è CARTELLA, allora la destinazione è il nome della cartella.
		 * Se il tipo è MASTERIZZATORE allora la destinazione indica il 
		 * masterizzatore da usare (potrebbero essercene più di uno).
		 */
		void impostaDestinazione( TipoDestinazione tipoDestinazione, String destinazione );
		
		void addFotografia( Fotografia foto );

		void addFotografie( IEnumerable<Fotografia> fotografie );

		IList<Fotografia> fotografie {
			get;
		}

		/** Indica se devo notificare la progressione della copia file per file.
		 * Se falso, viene notificato soltanto l'inizio e la fine
		 */
		bool notificareProgressione {
			get;
			set;
		}

		/** Quando il servizio è completato viene acceso questo flag .
		 * Una volta che il servizio è completato, non è più possibile masterizzare ulteriormente.
		 * Occorre fare la dispose e istanziarne un altro
		 */
		bool isCompletato {
			get;
		}

		/** Numero di foto copiate sulla chiavetta oppure copiate sul masterizzatore */
		int totFotoCopiate {
			get;
		}

		int totFotoNonCopiate {
			get;
		}

		/** imposto il prezzo forfettario del cd.
		 * Infatti tale prezzo non dipende mai dalle foto contenute, ma è sempre stabilito al volo
		 */
		decimal prezzoForfaittario {
			get;
			set;
		}


	}
}
