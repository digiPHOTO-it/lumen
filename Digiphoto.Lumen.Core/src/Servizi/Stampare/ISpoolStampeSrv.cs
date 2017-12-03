using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Stampare {

	// Non modificare i valori della enumeration, perché corrispondono ai metodi del ManagementObject (wmi)
	public enum PrnAzione {
		Pause,
		Resume
	};


	/**
	 * Lo spool di stampa è quello che presiede tutte le stampe.
	 */
	public interface ISpoolStampeSrv : IServizio {

		/** Aggiunge la stampa alla coda */
		void accodaStampaFoto(Fotografia foto, ParamStampaFoto param);

		void accodaStampaProvini( IList<Fotografia> foto, ParamStampaProvini param );

		/** 
		 * Elimina tutte le stempe da tutte le code.
		 * Piazza pulita!
		 */
		void svuotaTutteLeCode();

		IList<CodaDiStampe> code {
			get;
		}

		StampantiAbbinateCollection stampantiAbbinate {
			get;
		}

		/// <summary>
		///  Prendo la prima stampante, e da questa ricavo il rapporto tra W/H
		/// </summary>
		float ratioAreaStampabile {
			get;
		}


		/// <summary>
		/// Mette in pausa tutte stampanti di sistema (solo quelle gestite da Lumen)
		/// </summary>
		void pauseTutteLeStampanti();

		/// <summary>
		/// Riavvia tutte le stampanti di sistema (solo quelle gestite da Lumen)
		/// </summary>
		void resumeTutteLeStampanti();

	}
}
