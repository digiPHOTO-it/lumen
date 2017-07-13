using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Servizi.Explorer {
	
	/// <summary>
	/// Questo messaggio viene inviato quando nella gallery viene impostata la visualizzazione ad alta qualità
	/// </summary>
	public class HiQualityMsg : Messaggio {

		public HiQualityMsg( object sender ) : base( sender ) {
		}
	}
}
