using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Digiphoto.Lumen.Imaging.Correzioni {

	public class Trasla : Correzione {
		
		public double offsetX {
			get;
			set;
		}

		public double offsetY {
			get;
			set;
		}

		/// <summary>
		/// Questa è l'area di riferimento.
		/// Lo offset in pixel si riferisce a questa area.
		/// In pratica è la size della foto che si vede a video durante il fotoritocco puntuale.
		/// Servirà a riproporzionare lo spostamento sulla foto finale definitiva (che è più grande)
		/// </summary>
		public double rifW {
			get;
			set;
		}

		public double rifH {
			get;
			set;
		}


		public override bool isInutile {
			get {
				return offsetX == 0 && offsetY == 0;
			}
		}

		public override bool isSommabile( Correzione altra ) {
			return (altra is Trasla);
		}

		public override Correzione somma( Correzione altra ) {
			// TODO gestire stessa scala
			return new Trasla {
				offsetX = this.offsetX + ((Trasla)altra).offsetX,
				offsetY = this.offsetY + ((Trasla)altra).offsetY
			};
		}
	}
}
