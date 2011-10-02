using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;
using System.IO;

namespace Digiphoto.Lumen.Servizi.Scaricatore {
		
	public enum Fase {
			FineScarico,
			FineLavora
	};

	public class ScaricoFotoMsg : Messaggio {

		public ScaricoFotoMsg() {
			fotoDaLavorare = new List<FileInfo>();
		}

		public ScaricoFotoMsg( string descrizione ) : base( descrizione ) {
		}

		public ScaricoFotoMsg( Object sender, EventArgs e ) : base( sender, e ) {
		}

		public bool riscontratiErrori { get; set; }
		public int totFotoCopiateOk { get; set; }
		public int totFotoNonCopiate { get; set; }
		public int totFotoNonEliminate { get; set; }

		public List<FileInfo> fotoDaLavorare {
			get;
			set;
		}

		public string cartellaSorgente {
			get;
			set;
		}

		
		public Fase fase {get;	set;}


		public bool _puoiTogliereLaFlashCard {
			get;
			set;
		}
	}
}
