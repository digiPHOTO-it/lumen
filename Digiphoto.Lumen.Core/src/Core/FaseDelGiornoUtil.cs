using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.Core {

	/** Notare che i caratteri sono ANCHE in ordine alfabetico cosi si può fare un order by facilmente */
	public enum FaseDelGiorno : short {
		Mattino = 1,
		Pomeriggio = 2,
		Sera = 3
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

		public static readonly FaseDelGiorno [] fasiDelGiorno = { FaseDelGiorno.Mattino, FaseDelGiorno.Pomeriggio, FaseDelGiorno.Sera };
		

	}


}
