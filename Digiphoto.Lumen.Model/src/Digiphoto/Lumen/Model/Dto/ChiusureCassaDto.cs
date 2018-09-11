using System;
using System.Collections.Generic;
using System.Text;

namespace Digiphoto.Lumen.Model.Dto {

	[Serializable]
	public class ChiusureCassaDto {

		const string SEP_FIELD = ";";

		public ChiusureCassaDto() {
			listaChiusureGiorni = new List<ChiusuraCassaGiornoDto>();
		}

		public string pdv;

		public List<ChiusuraCassaGiornoDto> listaChiusureGiorni;


		public string serializeToPiccolaString() {

			StringBuilder ret = new StringBuilder( "1:" ); // tipo di oggetto. 1 = questo!
			ret.Append( pdv );

			// collezione di giornata
			ret.Append( "[2:" );
			foreach( var chiusuraGiorno in listaChiusureGiorni ) {
				ret.Append( chiusuraGiorno.serializeToString() );
			}

			ret.Append( "]" );		// la parentesi conclude la collezione
			ret.Append( "*" );		// fine oggetto

			return ret.ToString();
		}
	}
}
