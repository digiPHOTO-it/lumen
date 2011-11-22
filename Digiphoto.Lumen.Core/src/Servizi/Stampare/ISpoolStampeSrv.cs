using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Stampare {


	/**
	 * Lo spool di stampa è quello che presiede tutte le stampe.
	 */
	public interface ISpoolStampeSrv : IServizio {

		/** Aggiunge la stampa alla coda */
		void accodaStampa( Fotografia foto, ParamStampaFoto param );
	}
}
