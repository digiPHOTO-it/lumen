using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.UI.FotoRitocco {
	
	public enum ModalitaEdit {
		DefaultFotoRitocco,
		GestioneMaschere
	}

	public class EditorModeEventArgs : EventArgs {

		public ModalitaEdit modalitaEdit {
			get;
			set;
		}

		public EditorModeEventArgs( ModalitaEdit nuovaModalità ) {
			modalitaEdit = nuovaModalità;
		}

	}
}
