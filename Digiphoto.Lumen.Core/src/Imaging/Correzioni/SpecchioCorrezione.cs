using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging.Correzioni {

	public class SpecchioCorrezione : Correzione {

		public override bool isSommabile( Correzione altra ) {
			return altra is SpecchioCorrezione;
		}

		/// <summary>
		/// Se ho già uno specchio e ne applico un altro, allora si annullano.
		/// Quindi torno null in modo da segnalare che devo eliminare la correzione
		/// </summary
		public override Correzione somma( Correzione altra ) {
			if( altra is SpecchioCorrezione )
				return null;
			else
				throw new InvalidOperationException( "Non posso sommare " + altra.GetType() );
		}
	}
}
