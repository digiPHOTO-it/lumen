using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;

namespace Digiphoto.Lumen.Imaging {

	public interface IGestoreImmagineSrv : IServizio {

		/** Crea una immagine leggendola da disco */
		Immagine load( string fileName );

		/** Ritorna una immagine piccola (thumbnail) */
		Immagine creaProvino( Immagine immagineGrande );

		void save( Immagine immagine, string fileName );
	}
}
