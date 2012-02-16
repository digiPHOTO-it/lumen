using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Stampare
{

    public class StampanteAbbinata
    {
        public StampanteAbbinata()
        {
        }

        public StampanteAbbinata(StampanteInstallata stampanteInstallata, FormatoCarta formatoCarta)
        {
            this.StampanteInstallata = stampanteInstallata;
            this.FormatoCarta = formatoCarta;
        }

        public StampanteInstallata StampanteInstallata
        {
            get;
            set;
        }

        public FormatoCarta FormatoCarta
        {
            get;
            set;
        }

		public override string ToString() {
			
			StringBuilder sb = new StringBuilder();
			if( this.FormatoCarta != null )
				sb.Append( FormatoCarta.descrizione );

			if( this.StampanteInstallata != null ) {
				if( sb.Length > 0 )
					sb.Append( " su " );
				sb.Append( this.StampanteInstallata.ToString() );
			}

			return sb.ToString();
		}

		// Mi serve per ordinare i formati carta stampabili in base alla importanza (ordinamento) impostato
		public static int CompareByImportanza( StampanteAbbinata a, StampanteAbbinata b ) {

			int? ord1 = null;
			int? ord2 = null;

			if( a != null && a.FormatoCarta != null )
				ord1 = a.FormatoCarta.ordinamento;

			if( b != null && b.FormatoCarta != null )
				ord2 = b.FormatoCarta.ordinamento;

			if( ord1 == null ) {
				if( ord2 == null )
					return 0;
				else
					return -1;
			} else {
				if( ord2 == null )
					return 1;
				else {
					return (int)ord1 - (int)ord2;
				}
			}

		}
    }

	

    public interface IStampantiAbbinateSrv : IServizio 
    {
        void addAbbinamento(StampanteAbbinata stampanteAbbinata);

        void removeAbbinamento(StampanteAbbinata stampanteAbbinata);

		IList<StampanteAbbinata> stampantiAbbinate {
			get;
		}

        String listaStampantiAbbinateToString();

        void sostituisciAbbinamento(IList<StampanteAbbinata> listaStampantiAbbinate);
    }
}
