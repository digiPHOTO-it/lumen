using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Digiphoto.Lumen.Imaging.Correzioni {
	
	[XmlInclude(typeof(Ruota))]
	[XmlInclude(typeof(BiancoNero))]
	[XmlInclude(typeof(Sepia))]
	[XmlInclude(typeof(Specchio ) )]
	[XmlInclude(typeof(Luce))]
	[XmlInclude( typeof(Crop))]
	public abstract class Correzione {

		public virtual bool isSommabile( Correzione altra ) {
			return false;
		}

		public virtual Correzione somma( Correzione altra ) {
			return null;
		}

		public virtual bool isInutile {
			get {
				return false;
			}
		}
	}
}
