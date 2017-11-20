using System;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Ricostruzione {
	
	public class ParamRebuild {
		public DateTime giorno { get; set; }
		public Fotografo fotografo { get; set; }

		public override string ToString() {
			string ret;
			ret = "gg = " + giorno;
			if( fotografo != null )
				ret += " ; fotografo = " + fotografo.id;
			return ret;
		}
	}

	/// <summary>
	/// Questo servizio serve per ricostruire il database partendo dalle foto presenti sul filesystem
	/// </summary>
	public interface IDbRebuilderSrv : IServizio {

		void analizzare();
		void analizzare( ParamRebuild param );

		void ricostruire();

		bool necessarioRicostruire {
			get;
		}

		int contaFotoMancanti {
			get;
		}
		int contaFotoElaborate {
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

		int contaJpegMancanti {
			get;
		}
		int contaJpegElaborati {
			get;
		}

		int contaFotoEliminate {
			get;
		}

	}
}
