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
    }

    public interface IStampantiAbbinateSrv : IServizio 
    {
        void addAbbinamento(StampanteAbbinata stampanteAbbinata);

        void removeAbbinamento(StampanteAbbinata stampanteAbbinata);

        IList<StampanteAbbinata> listaStampantiAbbinate();

        String listaStampantiAbbinateToString();

        void sostituisciAbbinamento(IList<StampanteAbbinata> listaStampantiAbbinate);
    }
}
