using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Imaging.Correzioni {
	
	public class Logo : ImgOverlay, ICloneable {

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

		public override bool isSommabile( Correzione altra ) {
			return (altra is Logo);
		}

		public override Correzione somma( Correzione altra ) {
			return altra;
		}


		public object Clone() {
			Logo clone = new Logo {
				nomeFileLogo = this.nomeFileLogo,
				pcCopri = this.pcCopri,
				posiz = this.posiz,
				rotazione = this.rotazione,
				traslazione = this.traslazione,
				zoom = this.zoom
			};
			return clone;
		}
	}

}
