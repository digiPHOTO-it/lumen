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
