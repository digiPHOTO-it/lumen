using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Core;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Servizi.Ricerca {

	public class ParamCercaCarrello : ParamCerca {

		public Fotografo [] fotografi {	get; set; }

		public string intestazione { get; set; }

		public bool? isVenduto { get; set; }

		/// <summary>
		/// true = solo SelfService
		/// false = solo normali
		/// null = entrambi (nessun filtro)
		/// </summary>
		public bool? soloSelfService { get; set; }

		public IList<FaseDelGiorno>  fasiDelGiorno { get; set; }

		public DateTime? giornataIniz {	get; set; }
		public DateTime? giornataFine { get; set; }

		public bool idratareImmagini { get; set; }

		public ParamCercaCarrello() {

			// Istanzio la lista vuota che mi è più comoda
			fasiDelGiorno = new List<FaseDelGiorno>();
			idratareImmagini = true;
			isVenduto = false;
		}

		public ParamCercaCarrello ShallowCopy()
		{
			return (ParamCercaCarrello)this.MemberwiseClone();
		}

		public override string ToString() {

			StringBuilder sb = new StringBuilder( "--ParamRicercaFoto--" );


			if( fotografi != null ) {
				sb.Append( "\r\nFotografi : " + fotografi.Length );
				foreach( Fotografo f in fotografi )
					sb.Append( "\r\n\t(" + f.id + ")\t" + f.cognomeNome );
			}

			if( intestazione != null )
				sb.Append( "\r\nDidascalia :\t" + intestazione );

			if( fasiDelGiorno != null ) {
				sb.Append( "\r\nFasi del giorno : " + fasiDelGiorno.Count );
				foreach( FaseDelGiorno f in fasiDelGiorno )
					sb.Append( "\r\n\t(" + (short)f + ")\t" + f.ToString() );
			}

			if( giornataIniz != null )
				sb.Append( "\r\nGiornata inizio : " + giornataIniz );

			if( giornataFine != null )
				sb.Append( "\r\nGiornata fine : " + giornataFine );

			if( paginazione != null )
				sb.Append( "\r\nPaginzione : " + paginazione );

			return sb.ToString();
		}

		public void setFaseGiorno( FaseDelGiorno fase, bool accendiSpegni ) {
			if( accendiSpegni ) {
				// Devo accendere
				if( fasiDelGiorno.Contains( fase ) == false )
					fasiDelGiorno.Add( fase );
			} else {
				// Devo spegnere
				if( fasiDelGiorno.Contains( fase ) == true )
					fasiDelGiorno.Remove( fase );
			}
		}


	}
}
