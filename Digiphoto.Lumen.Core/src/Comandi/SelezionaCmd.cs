using System;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Comandi {
	
	/**
	 * Questo comando serve per selezionare o deselezionare le foto per gruppi.
	 */
	public class SelezionaComando : Comando {

		private bool _accendi;

		/**
		 * accendi = true   ==>   seleziono
		 * accendi = false  ==>   deseleziono
		 */
		public SelezionaComando( Target target, bool accendi ) {
			this._accendi = accendi;
		}

		internal override Esito esegui( Fotografia foto ) {
			foto.isSelezionata = _accendi;
			return Esito.Ok;
		}
	}
}
