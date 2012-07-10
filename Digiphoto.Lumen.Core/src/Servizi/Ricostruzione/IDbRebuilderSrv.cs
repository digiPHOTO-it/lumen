using System;
using Digiphoto.Lumen.Servizi;

namespace Digiphoto.Lumen.Servizi.Ricostruzione {
	
	/// <summary>
	/// Questo servizio serve per ricostruire il database partendo dalle foto presenti sul filesystem
	/// </summary>
	public interface IDbRebuilderSrv : IServizio {

		void analizzare();

		void ricostruire();

		bool necessarioRicostruire {
			get;
		}

		int contaFotoMancanti {
			get;
		}

		int contaFotografiMancanti {
			get;
		}

		int contaFotoAggiunte {
			get;
		}

		int contaFotografiAggiunti {
			get;
		}

	}
}
