using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Digiphoto.Lumen.Imaging.Correzioni {
	
	[XmlInclude(typeof(RuotaCorrezione))]
	[XmlInclude(typeof(BiancoNeroCorrezione))]
	[XmlInclude(typeof(SepiaCorrezione))]
	[XmlInclude(typeof(SpecchioCorrezione ) )]
	[XmlInclude(typeof(LuminositaContrastoCorrezione))]
	public abstract class Correzione {

		public virtual bool isSommabile( Correzione altra ) {
			return false;
		}

		public virtual Correzione somma( Correzione altra ) {
			return null;
		}
	}
}
