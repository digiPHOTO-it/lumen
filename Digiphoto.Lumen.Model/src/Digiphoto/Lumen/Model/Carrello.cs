using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Model {

	public partial class Carrello {

		public const string TIPORIGA_STAMPA = "S";
		public const string TIPORIGA_MASTERIZZATA = "M";


		public Decimal prezzoNettoTotale {

			get {
				return this.righeCarrello.Sum( r => r.prezzoNettoTotale );
			}
		}

		/// <summary>
		/// Mi dice quante foto da stampare ci sono nel carrello 
		/// </summary>
		public int sommatoriaQtaFotoDaStampare {
			get {
				return this.righeCarrello.Where( r => r.discriminator == Carrello.TIPORIGA_STAMPA ).Sum( rfs => rfs.quantita );
			}
		}

		/// <summary>
		/// Mi dice quante foto da masterizzare ci sono nel carrello 
		/// </summary>
		public int sommatoriaFotoDaMasterizzare {
			get {
				return this.righeCarrello.Count( r => r.discriminator == Carrello.TIPORIGA_MASTERIZZATA );
			}
		}


	}
}
