using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi;

namespace Digiphoto.Lumen.Servizi.EliminaFotoVecchie
{

    public class ParamEliminaFotoVecchieSrv
    {
        public DateTime dataIntervallo { get; set; }
        public Fotografo fotografo { get; set; }
    }


    public interface IEliminaFotoVecchie
    {
        void init(ParamEliminaFotoVecchieSrv param);

        IList<String> getListaCartelleDaEliminare();

        void elimina(String pathCartellaDaEliminare);

        Fotografo diChiSonoQuesteFoto();
    }

}
