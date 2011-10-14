using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.EliminaFotoVecchie{
    public class EliminaFotoVecchieMsg : Messaggio{

        public enum Fase
        {
            FineEliminazione,
            FineLavoro
        };

        Fotografia foto;

        public EliminaFotoVecchieMsg() {
            
        }

        public EliminaFotoVecchieMsg(string descrizione): base(descrizione){

		}

        public string cartellaSorgente { get; set; }
        public Fase fase { get; set; }
        public int totFotoEliminate { get; set; }
        public int totFotoNonEliminate { get; set; }
    }


}
