﻿using System.Collections.Generic;
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

		void addCorrezione(ref CorrezioniList correzioni, Correzione correzione);

		void addLogo( Fotografia fotografia, Logo logo, bool salvare );
		Logo addLogoDefault( Fotografia fotografia, String posiz, bool salvare );

		/** Applico tutte i ritocchi grafici indicati nel preciso ordine */
		IImmagine applicaCorrezione( IImmagine immaginePartenza, Correzione correzione );

		IImmagine applicaCorrezioni( IImmagine fotografia, CorrezioniList correzioni, IdrataTarget cosaRicalcolo );

		void removeCorrezione( Fotografia fotografia, Type quale );

		void tornaOriginale( Fotografia fotografia, bool salvare );
		void tornaOriginale( Fotografia fotografia );
		void tornaOriginale( IEnumerable<Fotografia> fotografie );
		void tornaOriginale( IEnumerable<Fotografia> fotografie, bool salvare );


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
		Fotografia modificaConProgrammaEsterno( Fotografia fotografia );

//		void acquisisciImmagineIncorniciataWithArtista( string nomeFileImg );

		void clonaImmagineIncorniciata(Fotografia fotoOrig, string nomeFileImg);

		/// <summary>
		/// Ritorno l'elenco delle maschere valorizzando solo il nome.
		/// Se le miniature non esistono, allora le creo.
		/// </summary>
		List<Model.Maschera> caricaListaMaschere( FiltroMask filtro );
		void salvaOrdinamentoMaschere( FiltroMask filtro, List<string> nuovaLista );


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
		IList<T> converteCorrezioni<T>( CorrezioniList correzioni );

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

		String getCartellaMaschera( FiltroMask filtro );

		/// <summary>
		/// Partendo dall'immagine originale, esegue una rotazione di un angolo retto e
		/// poi riscrive l'immagine originale.
		/// Avevamo posulato di non modificare mai l'immagine originale, ma questa è una comodità
		/// non indifferente (sia per il programmatore che per l'utente finale).
		/// </summary>
		/// <param name="fotografia"></param>
		/// <param name="ruota"></param>
		void autoRuotaSuOriginale( Fotografia fotografia, Ruota ruota );

		void ruotare( IEnumerable<Fotografia> fotografie, int pGradi );

		void applicareAzioneAutomatica( IEnumerable<Fotografia> fotografie, AzioneAuto azioneAuto );

		/// <summary>
		/// Rigenera i provini (in background) di una o più foto
		/// </summary>
		/// <param name="fotografie"></param>
		void provinare( IEnumerable<Fotografia> fotografie );

	}

}
