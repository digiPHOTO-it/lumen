﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Servizi.Explorer {
	
	public class RicercaModificataMessaggio : Messaggio {

		public bool abortito {
			get;
			set;
		}

		public RicercaModificataMessaggio( object sender ) : base( sender ) {
		}
	}
}
