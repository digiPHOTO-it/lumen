using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Model {

	public partial class Carrello {

		public Decimal prezzoNettoTotale {

			get {
				return righeCarrello.Sum( r => r.prezzoNettoTotale );
			}
		}

		/// <summary>
		/// Mi dice quante foto da stampare ci sono nel carrello 
		/// (non tiene conto dei files da masterizzare)
		/// </summary>
		public int sommatoriaQtaFotoDaStampare {
			get {
				return righeCarrello.OfType<RiCaFotoStampata>().Sum( rfs => rfs.quantita );
			}
		}

	}
}
