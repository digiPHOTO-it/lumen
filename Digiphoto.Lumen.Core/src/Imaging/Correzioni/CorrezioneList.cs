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
			//else
				//throw new InvalidOperationException( "Correzione vecchia non trovata" );
		}
	}


}
