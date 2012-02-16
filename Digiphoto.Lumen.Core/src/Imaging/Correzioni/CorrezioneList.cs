using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Digiphoto.Lumen.Imaging.Correzioni {

	[XmlRoot]
	[XmlInclude(typeof(Correzione))]
	public class CorrezioniList : List<Correzione> {
	}
}
