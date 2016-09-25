using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Servizi.EliminaFotoVecchie
{
    public enum Fase
    {
        FineEliminazione,
        FineEliminazioneAlbumNonReferenziati
    };

    class EliminaFotoVecchieMsg : Messaggio
    {
		public EliminaFotoVecchieMsg( object sender ) : base( sender ) {
		}

        public Fotografo fotografo { get; set; }

        public string cartellaSorgente { get; set; }

        public DateTime dataIntervallo { get; set; }

        public bool riscontratiErrori { get; set; }
       
        public Fase fase { get; set; }
       
        public int totFotoEliminate { get; set; }
    }



}
