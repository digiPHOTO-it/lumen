using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Digiphoto.Lumen.Imaging.Correzioni {

	[XmlRoot]
	[XmlInclude(typeof(Correzione))]
	public class CorrezioniList : List<Correzione> {


		public void sostituire( Correzione vecchia, Correzione nuova ) {

			int pos = this.FindIndex( x => x == vecchia );
			if( pos >= 0 )
				this[pos] = nuova;
		}

		/// <summary>
		/// Controllo se la lista delle correzioni ne contiene una di un determinato tipo
		/// </summary>
		/// <param name="type">Indicare una classe che derivi da Correzione</param>
		/// <returns>true se trovo una correzione con il tipo (classe) indicata</returns>
		public bool Contains( Type type ) {
			bool trovato = false;
			foreach( Correzione c in this ) {
				if( c.GetType() == type ) {
					trovato = true;
					break;
				}
			}
			return trovato;
		}

	}


}
