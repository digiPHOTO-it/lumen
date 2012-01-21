﻿using System;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Servizi.VolumeCambiato {

	public class VolumeCambiatoMessaggio : Messaggio {
		
		public VolumeCambiatoMessaggio( object sender ) : base( sender ) {
		}

		public string nomeVolume { get; set; }
		public bool montato { get; set; }
	}
}
