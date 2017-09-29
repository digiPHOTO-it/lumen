using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Servizi.Io {

	/**
	 * Questo servizio non contiene intelligenza o stato.
	 * gestisce le operazioni da/verso il filesystem che riguardano le Immagini.
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

		Correttore getCorrettore( Correzione correzione );
		Correttore getCorrettore( TipoCorrezione tipoCorrezione );

		/// <summary>
		/// Quando correggo le foto, non scrivo subito sul db le modifiche apportate.
		/// Questo perché voglio essere sempre in grado di annullare.
		/// Con questo metodo, rendo persistenti le correzioniXml che ancora sono transienti.
		/// </summary>
		void salvaCorrezioniTransienti( Fotografia fotografia );
		
		/// <summary>
		/// Una azione automatica contiene diverse correzioni.
		/// Ribalto queste correzioni sulla foto indicata, e quindi le salvo su disco.
		/// </summary>
		void salvaCorrezioniAutomatiche( IEnumerable<Fotografia> fotografie, AzioneAuto azioneAuto );

		void idrataImmaginiFoto( Fotografia foto, IdrataTarget target, bool forzatamente );

		void idrataMaschera( Model.Maschera maschera, bool ancheOriginale );
	}
}
