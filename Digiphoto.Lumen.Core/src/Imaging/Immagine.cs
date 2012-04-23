using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging {
	
	public abstract class Immagine : IImmagine {

		public Orientamento orientamento {
			get {
				return (ww >= hh ? Orientamento.Orizzontale : Orientamento.Verticale);
			}
		}

		public float rapporto {
			get {
				return (float)ww / (float)hh;
			}
		}


		#region Metodi astratti dall'interfaccia
		public abstract int ww {
			get;
		}

		public abstract int hh {
			get;
		}

		public abstract void Dispose();

		#endregion
	}
}
