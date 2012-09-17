using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging.Correzioni;

namespace Digiphoto.Lumen.Imaging {

	/**
	 * Questo servizio non contiene intelligenza o stato.
	 * E' un esecutore di interventi grafici.
	 * Verrà pilotato da un altro servizio "più intelligente" e più vicino
	 * al modello.
	 */
	public interface IGestoreImmagineSrv : IServizio {

		/** Crea una immagine leggendola da disco */
		IImmagine load( string fileName );

		/** Crea una immagine piccola (thumbnail) riducendo quella grande passata per parametro */
		IImmagine creaProvino( IImmagine immagineGrande );
		//
		IImmagine creaProvino( IImmagine immagineGrande, long sizeLatoMax );

		/** Salva l'immagine indicata sul filesystem */
		void save( IImmagine immagine, string fileName );

		/** Applico tutte i ritocchi grafici indicati nel preciso ordine */
		IImmagine applicaCorrezioni( IImmagine immaginePartenza, IEnumerable<Correzione> correzioni );

		IImmagine applicaCorrezione( IImmagine immaginePartenza, Correzione correzione );
	}
}
