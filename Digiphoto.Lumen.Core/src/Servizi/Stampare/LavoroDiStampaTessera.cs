using System.Collections.Generic;
using System.Linq;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Stampare {

	public class LavoroDiStampaTessera : LavoroDiStampaFoto {

		public LavoroDiStampaTessera( Fotografia fotografia, ParamStampaTessera param ) : base( fotografia, param ) {
		}

		public ParamStampaTessera paramStampaTessera {
			get {
				return (ParamStampaTessera)this.param;
			}
		}

		public override string ToString() {
			ParamStampaTessera paramST = (ParamStampaTessera)this.param;
			return string.Format( "Job Stampa Tessera " + paramST.numColonne + "x" + paramST.numRighe );
		}

	}
}
