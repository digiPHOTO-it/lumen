using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Digiphoto.Lumen.Imaging.Correzioni {
	
	[XmlInclude(typeof(RuotaCorrezione))]
	[XmlInclude(typeof(BiancoNeroCorrezione))]
	public abstract class Correzione {
	}
}
