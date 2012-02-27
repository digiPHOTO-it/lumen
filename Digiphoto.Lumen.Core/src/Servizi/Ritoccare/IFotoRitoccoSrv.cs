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
	}
}
