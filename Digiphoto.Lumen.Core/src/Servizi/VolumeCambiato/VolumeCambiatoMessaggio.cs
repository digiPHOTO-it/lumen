using System;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Servizi.VolumeCambiato {

	public class VolumeCambiatoMessaggio : Messaggio {
		
		public VolumeCambiatoMessaggio( Object sender, EventArgs e ) : base( sender, e ) {
		}

		public VolumeCambiatoMessaggio() {
		}

		public string nomeVolume { get; set; }
		public bool montato { get; set; }
	}
}
