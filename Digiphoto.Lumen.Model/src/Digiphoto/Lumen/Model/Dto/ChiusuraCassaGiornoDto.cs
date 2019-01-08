using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Model.Dto {

	[Serializable]
	public class ChiusuraCassaGiornoDto {

		public DateTime giornata;

		// Questi sono i dati delle chiusure di cassa
		public Decimal ccIncassoDichiarato;
		public Decimal ccIncassoPrevisto;

/* per ora non li invio. forse non servono e appesantiscono
		// Questi sono i dati dei carrelli
		public Nullable<Decimal> caIncassoDichiarato;
		public Nullable<Decimal> caIncassoPrevisto;
*/

		public int totFotoStampate;
		public int totFotoMasterizzate;
		public int totFotoScattate;

		const string SEP_FIELD = ";";

		public string serializeToString() {

			StringBuilder ret = new StringBuilder();

			// La data invece che metterla in chiaro, la esprimo con il numero di giorni passati dal 01-01-2010
			var timeSpan = giornata.Subtract( new DateTime( 2018, 1, 1 ) );
			int ggPassati = (int)timeSpan.TotalDays;
			ret.Append( ggPassati.ToString( "X" ) );
			ret.Append( SEP_FIELD );

			ret.Append( ((short)this.ccIncassoDichiarato).ToString( "X" ) );
			ret.Append( SEP_FIELD );

			ret.Append( ((short)this.ccIncassoPrevisto).ToString( "X" ) );
			ret.Append( SEP_FIELD );

			ret.Append( this.totFotoScattate.ToString( "X" ) );
			ret.Append( SEP_FIELD );

			ret.Append( this.totFotoStampate.ToString( "X" ) );
			ret.Append( SEP_FIELD );

			ret.Append( this.totFotoMasterizzate.ToString( "X" ) );
			
			// Sull'ultimo non metto il separatore di campo.

			ret.Append( "*" );		// fine oggetto
			return ret.ToString();
		}
	}

}
