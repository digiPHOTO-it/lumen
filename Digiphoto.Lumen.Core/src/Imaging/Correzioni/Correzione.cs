using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Digiphoto.Lumen.Servizi.Ritoccare;

namespace Digiphoto.Lumen.Imaging.Correzioni {

	public enum TipoCorrezione {
		BiancoNero = 1,
		Sepia = 2,
		Dominante = 3,
		Luce = 4,
		Gimp = 5,
		Ruota = 6,
		Specchio = 7,
		Zoom = 8,
		Ridimensiona = 9,
		Trasla = 10
	};

	[XmlInclude(typeof(Ruota))]
	[XmlInclude(typeof(BiancoNero))]
	[XmlInclude(typeof(Sepia))]
	[XmlInclude(typeof(Specchio ) )]
	[XmlInclude(typeof(Luce))]
	[XmlInclude(typeof(Crop))]
	[XmlInclude(typeof(Dominante))]
	[XmlInclude(typeof(Gimp))]
	[XmlInclude(typeof(Zoom))]
	[XmlInclude(typeof(Trasla))]
	[XmlInclude(typeof(Maschera))]
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
