using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Stampare {

	public class ParamStampaFoto : ICloneable {

		/**
		 * La stampante su cui vado a stampare, deve essere già opportunamente configurata
		 * per accettare il formato carta che sto per indicare.
		 * Il programma non interviene in nessun modo sulle impostazioni del formato o degli
		 * attributi della stampante.
		 * Basta solo il nome per sapere dove andare.
		 */
		public string nomeStampante;

		/**
		 *  Decide automaticamente l'orientamento giusto per stampare
		 *  la foto in modo che riempia più possibile la carta.
		 *  Per esempio se la foto è verticale e l'area di stampa è orizzontale, viene concettualmente girata.
		 */
		public bool autoRuota;

		/** 
		 *  SI = riempie tutta l'area stampabile.
		 *  NO = bordi bianchi
		 */
		public bool autoZoomToFit;

		public FormatoCarta formatoCarta;

		public short numCopie;

		public override string ToString() {

			StringBuilder s = new StringBuilder();
			
			if( nomeStampante != null )
				s.Append( " Stampante=" + nomeStampante );
			if( formatoCarta != null )
				s.Append( " Carta=" + formatoCarta.descrizione );
			s.Append( " Copie=" + numCopie );

			return s.ToString();
		}

		public object Clone() {
			return this.MemberwiseClone();
		}
	}
}
