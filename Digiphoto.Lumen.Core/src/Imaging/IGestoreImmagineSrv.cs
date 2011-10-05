using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;

namespace Digiphoto.Lumen.Imaging {

	public interface IGestoreImmagineSrv : IServizio {

		/** Crea una immagine leggendola da disco */
		Immagine load( string fileName );

		/** Crea una immagine piccola (thumbnail) riducendo quella grande passata per parametro */
		Immagine creaProvino( Immagine immagineGrande );

		/** Salva l'immagine indicata sul filesystem */
		void save( Immagine immagine, string fileName );
	}
}
