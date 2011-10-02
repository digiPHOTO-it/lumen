using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Config {

	class ConfigurazioneNonValidaException : Exception {

		public ConfigurazioneNonValidaException( string msg ) : base( msg ) {
		}

		public ConfigurazioneNonValidaException( string msg, Exception inner ) : base( msg, inner ) {
		}

	}
}
