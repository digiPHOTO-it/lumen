using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi;

namespace Digiphoto.Lumen.Servizi.EliminaFotoVecchie
{


    public interface IEliminaFotoVecchie
    {
        IList<String> getListaCartelleDaEliminare();

        void elimina(String pathCartellaDaEliminare);

		Fotografo diChiSonoQuesteFoto(String pathCartellaDaEliminare);
    }

}
