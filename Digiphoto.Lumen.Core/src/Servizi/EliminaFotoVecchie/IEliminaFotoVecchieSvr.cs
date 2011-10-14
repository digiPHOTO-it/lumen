using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.EliminaFotoVecchie{


    public class ParamEliminaFotoVecchie{
        public Fotografo fotografo { get; set; }
        public string cartellaSorgente { get; set; }
        public DateTime intervalloEliminazione{ get; set; }
    }

    public interface IEliminaFotoVecchie : IServizio{

        public void setAttributiEliminaFotoVecchie(ParamEliminaFotoVecchie paramEliminaFotoVecchie){}

        public void elimima() {}
    }


}
