using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Digiphoto.Lumen.Config {
	/// <summary>

	/// The class that holds onto each element returned by the configuration manager.

	/// </summary>

	public class ServizioElement : ConfigurationElement {

		[ConfigurationProperty( "interfaccia", DefaultValue = "", IsKey = true, IsRequired = true )]
		public string Interfaccia {

			get {
				return ((string)(base ["interfaccia"]));
			}

			set {
				base ["interfaccia"] = value;
			}

		}


		[ConfigurationProperty( "implementazione", DefaultValue = "", IsKey = false, IsRequired = false )]
		public string Implementazione {

			get {
				return ((string)(base ["implementazione"]));
			}

			set {
				base ["implementazione"] = value;
			}
		}
	}
}
