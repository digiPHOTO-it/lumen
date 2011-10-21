using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Servizi.EliminaFotoVecchie
{
    public enum Fase
    {
        FineEliminazione
    };

    class EliminaFotoVecchieMsg
    {
        public string cartellaSorgente { get; set; }

        public DateTime dataIntervallo { get; set; }

        public bool riscontratiErrori { get; set; }
       
        public Fase fase { get; set; }
       
        public int totFotoEliminate { get; set; }
    }


}
