using Digiphoto.Lumen.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.UI.SelettoreAzioniRapide {

	public class SelezioneFotografoPopupRequestEventArgs : EventArgs {

		public SelezioneFotografoPopupRequestEventArgs( Fotografia fotoFaccia ) {
			this.fotoFaccia = fotoFaccia;
		}

		public Fotografia fotoFaccia { get; private set; }
	}
}
