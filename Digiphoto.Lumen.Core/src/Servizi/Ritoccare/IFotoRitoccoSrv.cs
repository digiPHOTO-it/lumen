using System.Collections.Generic;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Util;
using System;
using System.Collections;
using Digiphoto.Lumen.Imaging;

namespace Digiphoto.Lumen.Servizi.Ritoccare {
	
	/// <summary>
	///	 Questo servizio serve a pilotare la schermata di ritocco delle foto
	/// </summary>

	public interface IFotoRitoccoSrv : IServizio {

		/// <summary>
		/// Aggiunge una correzione a quelle esistenti sulla foto.
		/// Ma senza riapplicarle
		/// </summary>
		/// <param name="fotografia"></param>
		/// <param name="correzione"></param>
		void addCorrezione( Fotografia fotografia, Correzione correzione, bool salvare );
		void addCorrezione( Fotografia fotografia, Correzione correzione );


		/** Applico tutte i ritocchi grafici indicati nel preciso ordine */
		IImmagine applicaCorrezione( IImmagine immaginePartenza, Correzione correzione );

		IImmagine applicaCorrezioni( IImmagine fotografia, CorrezioniList correzioni, IdrataTarget cosaRicalcolo );

		void removeCorrezione( Fotografia fotografia, Type quale );

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

		void modificaMetadati( Fotografia foto );

		/// <summary>
		/// Data una foto con eventuali modifiche già presenti,
		/// lancio un programma esterno (per esempio MSPaint oppure Gimp oppure photoshop)
		/// Quando il programma avrà terminato il suo lavoro, vado a salvare la foto su disco con un nome diverso.
		/// </summary>
		/// <param name="foto">la foto da modificare</param>
		Fotografia [] modificaConProgrammaEsterno( Fotografia [] fotografie );

		void acquisisciImmagineIncorniciataWithArtista( string nomeFileImg );

		void clonaImmagineIncorniciata(Fotografia fotoOrig, string nomeFileImg);

		/// <summary>
		/// Ritorno l'elenco delle miniature delle maschere.
		/// Se le miniature non esistono, allora le creo.
		/// </summary>
		String [] caricaMiniatureMaschere();

		void clonaFotografie(Fotografia[] fotografie);

		/// <returns></returns>


		/// <summary>
		///  Utilizzando il Correttore opportuno, converte la Correzione nella sua implementazione concreta.
		///  Il Correttore è anche un TypeConverter.
		/// </summary>
		/// <typeparam name="T">Puo essere ShaderEffect (o derivate) oppure Transform</typeparam>
		/// <param name="fotografia"></param>
		/// <returns>una lista di oggetti del tipo desiderato.</returns>
		IList<T> converteCorrezioni<T>( Fotografia fotografia );

		/// <summary>
		/// Utilizzando il correttore opportuno, converto un oggetto che può essere
		/// di tipo ShaderEffect oppure Transform -> in una Correzione.
		/// </summary>
		/// <param name="effettiTrasf"></param>
		/// <returns>Una lista di Correzioni</returns>
		CorrezioniList converteInCorrezioni( IEnumerable<Object> effettiTrasf );

		Correzione converteInCorrezione( TipoCorrezione tipoDest, Object effettoOrTrasformazione );


		// TODO : forse si può eliminare ???
		Correttore getCorrettore( object obj );
	}
}
