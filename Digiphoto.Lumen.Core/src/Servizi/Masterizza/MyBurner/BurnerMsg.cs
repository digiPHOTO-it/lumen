using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Servizi.Masterizzare.MyBurner
{
    public enum Fase
    {
        NessunaOperazione,
        MasterizzazioneIniziata,
        MasterizzazioneCompletata,
        MasterizzazioneFallita,
        FormattazioneIniziata,
        FormattazioneCompletata,
        FormattazioneFallita,
        ValidatingCurrentMedia,
        FormattingMedia,
        ErrorMedia,
        InitializingHardware,
        OptimizingLaserIntensity,
        FinalizingWriting,
        Completed,
        Verifying
    };

    public class BurnerMsg : Messaggio
    {
		public BurnerMsg( object sender ) : base( sender ) {
		}

        public Fase fase { get; set; }

        public String capacity { get; set; }

        public int progress { get; set; }

        public int totaleFileAggiunti { get; set; }

        public String statusMessage { get; set; }
    }
}
