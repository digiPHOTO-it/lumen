using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.Core {

	/** Notare che i caratteri sono ANCHE in ordine alfabetico cosi si può fare un order by facilmente */
	public enum FaseDelGiorno : byte {
		Mattino		= (byte) 'M',
		Pomeriggio	= (byte) 'P',
		Sera		= (byte) 'S'
	};
		
	public class FaseDelGiornoUtil {

		public static FaseDelGiorno getFaseDelGiorno( DateTime timestamp ) {

			FaseDelGiorno fase = FaseDelGiorno.Sera;
			
			if( timestamp.Hour >= 14 && timestamp.Hour < 20)
				fase = FaseDelGiorno.Pomeriggio;
			else if( timestamp.Hour >= 5 )
				fase = FaseDelGiorno.Mattino;

			return fase;
		}

	}
}
