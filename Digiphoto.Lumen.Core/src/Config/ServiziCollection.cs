using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.Config {


	/// <summary>
	/// The collection class that will store the list of each element/item that
	///        is returned back from the configuration manager.
	/// </summary>
	[ConfigurationCollection( typeof( ServizioElement ) )]
	internal class ServiziCollection : ConfigurationElementCollection {

		protected override ConfigurationElement CreateNewElement() {
			return new ServizioElement();
		}


		protected override object GetElementKey( ConfigurationElement element ) {
			return ((ServizioElement)(element)).Interfaccia;
		}



		public ServizioElement this [int idx] {
			get {
				return (ServizioElement)BaseGet( idx );
			}

		}
	}
}
