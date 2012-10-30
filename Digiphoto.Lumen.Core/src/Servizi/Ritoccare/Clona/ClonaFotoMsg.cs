using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;
using System.IO;

namespace Digiphoto.Lumen.Servizi.Ritoccare.Clona {
		
	public enum FaseClone {
			InizioClone,
			FineClone
	};

	public class ClonaFotoMsg : Messaggio {

		public ClonaFotoMsg(object sender, string descrizione)
			: base(sender, descrizione)
		{
		}

		public EsitoClone esitoScarico {
			get;
			set;
		}
		
		public FaseClone fase {get;	set;}
	}
}
