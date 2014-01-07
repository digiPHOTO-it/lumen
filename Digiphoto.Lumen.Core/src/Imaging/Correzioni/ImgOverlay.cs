using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Imaging.Correzioni {

	public abstract class ImgOverlay : Correzione {

		/// <summary>
		/// Traslazione rispetto alla immagine originale.
		/// Qui ho l'informazione anche della grandezza della immagine originale
		/// </summary>
		public Trasla traslazione {
			get;
			set;
		}

		/// <summary>
		/// Eventuale fattore di zoom per ingrandire o rimpicciolire la foto.
		/// Questo fattore è dato anche dalla dimensione della foto.
		/// </summary>
		public Zoom zoom {
			get;
			set;
		}

		/// <summary>
		/// Eventuale rotazione rispetto alla immagine originale
		/// </summary>
		public Ruota rotazione {
			get;
			set;
		}

		public override bool isSommabile( Correzione altra ) {
			return false;
		}

	}

}
