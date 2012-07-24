using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Servizi.Stampare
{
    public class StampanteInstallata
    {
        public StampanteInstallata()
        {
        }

        public StampanteInstallata(String nomeStampante)
        {
            this.NomeStampante = nomeStampante;
        }

        public StampanteInstallata(String nomeStampante, String portaStampante)
        {
            this.NomeStampante = nomeStampante;
            this.PortaStampante = portaStampante;
        }


        public String NomeStampante 
        {
			get;
			set;
		}

		public String PortaStampante 
        {
			get;
			set;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			if( NomeStampante != null )
				sb.Append( NomeStampante );
			if( PortaStampante != null )
				sb.Append( " [" + PortaStampante + "]" );
			return sb.ToString();
		}

        /// <summary>
        /// Create a new StampanteInstallata object.
        /// </summary>
        /// <param name="NomeStampante">nomeStampante.</param>
        public static StampanteInstallata CreateStampanteInstallata(global::System.String nomeStampante, global::System.String portaStampante)
        {
            StampanteInstallata stampanteInstallata = new StampanteInstallata();
            stampanteInstallata.NomeStampante = nomeStampante;
            stampanteInstallata.PortaStampante = portaStampante;
            return stampanteInstallata;
        }

		public override int GetHashCode() {
			return 17 + 31 * NomeStampante.GetHashCode();
		}

		public override bool Equals( object obj ) {

			bool sonoUguali = false;

			if( obj is StampanteInstallata ) {
				StampanteInstallata altra = (StampanteInstallata)obj;
				sonoUguali = this.NomeStampante.Equals( altra.NomeStampante );
			}

			return sonoUguali;
		}

    }

    public interface IStampantiInstallateSrv : IServizio 
    {
		IList<StampanteInstallata> stampantiInstallate {
			get;
		}

        StampanteInstallata getStampanteInstallataByString(String nomeStampante);
    }
}
