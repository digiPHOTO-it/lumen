using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Eventi {


	/// <summary>
	/// Questo messaggio mi indica che sono state aggiunte o modificate o cancellate
	/// </summary>
	public class EntityCambiataMsg : Messaggio {

		public EntityCambiataMsg( object sender ) : base( sender ) {
		}

		public Type type {
			get;
			set;
		}

		/// <summary>
		/// Questa property è facoltativa
		/// </summary>
		public object entita {
			get;
			set;
		}

	}
}
