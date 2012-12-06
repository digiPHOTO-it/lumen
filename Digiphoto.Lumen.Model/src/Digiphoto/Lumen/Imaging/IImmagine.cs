using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging {

	public enum Orientamento {
		Orizzontale, Verticale
	}

	public interface IImmagine : IDisposable, ICloneable {

		int ww { 
			get; 
		}
		
		int hh { 
			get; 
		}
		
		Orientamento orientamento { 
			get; 
		}

		/**
		 *  E' dato dalla larghezza / altezza  
		 *  rapp = ww / hh
		 */
		float rapporto { 
			get; 
		}
	}

}
