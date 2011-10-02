using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Digiphoto.Lumen.Config {

	internal class StartupServiziConfigSection : ConfigurationSection {

		[ConfigurationProperty( "Servizi" )]
		public ServiziCollection ServiziItems {
			get {
				return ((ServiziCollection)(base ["Servizi"]));
			}
		}
	}
}
