using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Imaging.Correzioni {
	
	public class Logo : ImgOverlay {

		public enum PosizLogo {
			NordEst,
			SudEst,
			SudOvest,
			NordOvest
		};

		/// <summary>
		/// E' il nome del file senza percorso.
		/// La cartella loghi fa parte della configurazione.
		/// </summary>
		public String nomeFileLogo {
			get;
			set;
		}

		/// <summary>
		/// Percentuale di copertura della immagine originale.
		///  null  : posizionato in manuale.
		/// !null :  posizionato automaticamente.
		/// </summary>
		public Nullable<short> pcCopri {
			get;
			set;
		}

		public Nullable<PosizLogo> posiz {
			get;
			set;
		}
		


	}

}
