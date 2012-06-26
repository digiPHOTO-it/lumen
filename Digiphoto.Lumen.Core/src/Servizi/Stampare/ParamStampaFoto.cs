using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.Servizi.Stampare {

	public class ParamStampaFoto : ParamStampa
	{
		public ParamStampaFoto() {
			this.autoRuota = true;
			this.numCopie = 1;
		}

		/** 
		 *  SI = riempie tutta l'area stampabile.
		 *  NO = bordi bianchi
		 */
		public bool autoZoomNoBordiBianchi;

		public override string ToString() {

			StringBuilder s = new StringBuilder();
			
			if( nomeStampante != null )
				s.Append( " Stampante=" + nomeStampante );
			if( formatoCarta != null )
				s.Append( " Carta=" + formatoCarta.descrizione );
			s.Append( " Copie=" + numCopie );

			return s.ToString();
		}

		/**
		 * Questa informazione mi serve nella gestione dell'esito.
		 * se ci sono problemi devo stornare
		 */
		public Guid idRigaCarrello;

	}
}
