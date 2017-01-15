using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Model {

	public partial class Carrello {

		public override bool Equals( object altro ) {
			bool uguali = false;
			if( altro != null && altro is Carrello ) {
				uguali = this.id.Equals( ((Carrello)altro).id );
			}

			return uguali;
		}

		public override int GetHashCode() {
			int hash = 7;
			hash = 31 * hash + (null == this.id ? 0 : this.id.GetHashCode());
			return hash;
		}

    }
}
