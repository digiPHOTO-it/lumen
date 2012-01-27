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
    }

    public interface IStampantiInstallateSrv : IServizio 
    {
        IList<StampanteInstallata> listaStampantiInstallate();

        StampanteInstallata stampanteInstallataByString(String nomeStampante);
    }
}
