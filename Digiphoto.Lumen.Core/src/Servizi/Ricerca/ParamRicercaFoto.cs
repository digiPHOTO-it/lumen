using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Core;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Ricerca {

	public class ParamRicercaFoto {

		public Evento [] eventi { get; set; }
		public Fotografo [] fotografi {	get; set; }

		public string didascalia { get; set; }
		public FaseDelGiorno [] fasiDelGiorno { get; set; }
		public int [] numeriFotogrammi { get; set; }

		public DateTime? giornataIniz {	get; set; }
		public DateTime? giornataFine { get; set; }

		/** Numero di record di ampiezza della paginazione. Se NULL allora ninente */
		public Paginazione paginazione { get; set; }

		public override string ToString() {

			StringBuilder sb = new StringBuilder( "--ParamRicercaFoto--" );

			if( eventi != null ) {
				sb.Append( "\r\nEventi : " + eventi.Length );
				foreach( Evento e in eventi )
					sb.Append( "\r\n\t(" + e.id + ")\t" + e.descrizione );
			}

			if( fotografi != null ) {
				sb.Append( "\r\nFotografi : " + fotografi.Length );
				foreach( Fotografo f in fotografi )
					sb.Append( "\r\n\t(" + f.id + ")\t" + f.cognomeNome );
			}

			if( didascalia != null )
				sb.Append( "\r\nDidascalia :\t" + didascalia );

			if( fasiDelGiorno != null ) {
				sb.Append( "\r\nFasi del giorno : " + fasiDelGiorno.Length );
				foreach( FaseDelGiorno f in fasiDelGiorno )
					sb.Append( "\r\n\t(" + (short)f + ")\t" + f.ToString() );
			}

			if( numeriFotogrammi != null ) {
				sb.Append( "\r\nNum. fotogrammi : " + numeriFotogrammi.Length );
				foreach( int numFotogramma in numeriFotogrammi )
					sb.Append( "\r\n\t(" + numFotogramma + ")" );
			}

			if( giornataIniz != null )
				sb.Append( "\r\nGiornata inizio : " + giornataIniz );

			if( giornataFine != null )
				sb.Append( "\r\nGiornata fine : " + giornataFine );

			if( paginazione != null )
				sb.Append( "\r\nPaginzione : " + paginazione );

			return sb.ToString();
		}

		public class Paginazione {

			public int skip { get; set; }
			public int take { get; set; }
			
			public override string ToString() {
				StringBuilder sb = new StringBuilder( "--Paginazione--" );
				sb.Append( "\r\nSkip: " + skip );
				sb.Append( "\r\nTake: " + take );
				return sb.ToString();
			}
		}

	}
}
