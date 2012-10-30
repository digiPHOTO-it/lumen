using System.Collections.Generic;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Util;
using System;

namespace Digiphoto.Lumen.Servizi.Ritoccare {
	
	/// <summary>
	///	 Questo servizio serve a pilotare la schermata di ritocco delle foto
	/// </summary>

	public interface IFotoRitoccoSrv : IServizio {


		/// <summary>
		/// Aggiunge una correzione a quelle esistenti sulla foto.
		/// Fatto questo, riapplica tutto.
		/// </summary>
		/// <param name="fotografia"></param>
		/// <param name="correzione"></param>
		void addCorrezione( Fotografia fotografia, Correzione correzione, bool salvare );
		void addCorrezione( Fotografia fotografia, Correzione correzione );

		void removeCorrezione( Fotografia fotografia, Type quale );

		/// <summary>
		/// Partendo dall'immagine iniziale, ricrea il provino applicando tutte le correzioni
		/// </summary>
		/// <param name="fotografia"></param>
		void applicaCorrezioniTutte( Fotografia fotografia );

		void tornaOriginale( Fotografia fotografia, bool salvare );
		void tornaOriginale( Fotografia fotografia );

		/// <summary>
		/// Se ho iniziato a correggere la foto, ma poi mi accorgo che il risultato non mi
		/// piace, devo tornare indietro (UNDO) ed eliminare le modifiche che ho fatto in modo
		/// temporaneo (transiente).
		/// Quindi occorre rileggere da disco l'attributo "correzioni" e ricaricare da disco anche 
		/// l'immagine del provino
		/// </summary>
		void undoCorrezioniTransienti( Fotografia fotografia );

		/// <summary>
		/// Quando correggo le foto, non scrivo subito sul db le modifiche apportate.
		/// Questo perché voglio essere sempre in grado di annullare.
		/// Con questo metodo, rendo persistenti le correzioniXml che ancora sono transienti.
		/// </summary>
		void salvaCorrezioniTransienti( Fotografia fotografia );

		void modificaMetadati( Fotografia foto );

		/// <summary>
		/// Data una foto con eventuali modifiche già presenti,
		/// lancio un programma esterno (per esempio MSPaint oppure Gimp oppure photoshop)
		/// Quando il programma avrà terminato il suo lavoro, vado a salvare la foto su disco con un nome diverso.
		/// </summary>
		/// <param name="foto">la foto da modificare</param>
		Fotografia [] modificaConProgrammaEsterno( Fotografia [] fotografie );

		void acquisisciImmagineIncorniciata( string nomeFileImg );

		/// <summary>
		/// Ritorno l'elenco delle miniature delle maschere.
		/// Se le miniature non esistono, allora le creo.
		/// </summary>
		String [] caricaMiniatureMaschere();

		void clonaFotografie(Fotografia[] fotografie);

	}
}
