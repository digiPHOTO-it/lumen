using System.Collections.Generic;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Servizi.Selezionare;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Servizi.Ritoccare {
	
	/// <summary>
	///	 Questo servizio serve a pilotare la schermata di ritocco delle foto
	/// </summary>

	public interface IFotoRitoccoSrv : IServizio {

		List<Fotografia> fotografieDaModificare {
			get;
		}

		/// <summary>
		/// Aggiunge una correzione a quelle esistenti sulla foto.
		/// Fatto questo, riapplica tutto.
		/// </summary>
		/// <param name="fotografia"></param>
		/// <param name="correzione"></param>
		void addCorrezione( Fotografia fotografia, Correzione correzione, bool salvare );
		void addCorrezione( Fotografia fotografia, Correzione correzione );
		void addCorrezione( Target target, Correzione correzione );

		/// <summary>
		/// Partendo dall'immagine iniziale, ricrea il provino applicando tutte le correzioni
		/// </summary>
		/// <param name="fotografia"></param>
		void applicaCorrezioniTutte( Fotografia fotografia );

		void tornaOriginale( Fotografia fotografia, bool salvare );
		void tornaOriginale( Fotografia fotografia );
		void tornaOriginale( Target target );

		/// <summary>
		/// Se ho iniziato a correggere la foto, ma poi mi accorgo che il risultato non mi
		/// piace, devo tornare indietro (UNDO) ed eliminare le modifiche che ho fatto in modo
		/// temporaneo (transiente).
		/// Quindi occorre rileggere da disco l'attributo "correzioni" e ricaricare da disco anche 
		/// l'immagine del provino
		/// </summary>
		void undoCorrezioniTransienti( Fotografia fotografia );
		void undoCorrezioniTransienti( Target target );

		/// <summary>
		/// Quando correggo le foto, non scrivo subito sul db le modifiche apportate.
		/// Questo perché voglio essere sempre in grado di annullare.
		/// Con questo metodo, rendo persistenti le correzioniXml che ancora sono transienti.
		/// </summary>
		void salvaCorrezioniTransienti( Fotografia fotografia );
		void salvaCorrezioniTransienti( Target target );


	}
}
