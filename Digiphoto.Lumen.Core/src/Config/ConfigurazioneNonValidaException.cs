using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Digiphoto.Lumen.Config {

	public class ConfigurazioneNonValidaException : ConfigurationErrorsException {

		public ConfigurazioneNonValidaException( string msg ) : base( msg ) {
		}

		public ConfigurazioneNonValidaException( string msg, Exception inner ) : base( msg, inner ) {
		}

	}
}
