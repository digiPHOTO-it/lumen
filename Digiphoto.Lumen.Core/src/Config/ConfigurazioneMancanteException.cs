using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Digiphoto.Lumen.Config {

	public class ConfigurazioneMancanteException : ConfigurationErrorsException {

		public ConfigurazioneMancanteException() {
		}

		public ConfigurazioneMancanteException( string sMessage, Exception innerException )
			: base( sMessage, innerException ) {
		}

		public ConfigurazioneMancanteException (string sMessage)
			: base(sMessage) { 
		}
	}
}
