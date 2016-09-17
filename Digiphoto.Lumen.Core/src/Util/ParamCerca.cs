using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Digiphoto.Lumen.Util {

	[Serializable]
	public class ParamCerca {

		/** Numero di record di ampiezza della paginazione. Se NULL allora ninente */
		public Paginazione paginazione { get; set; }

	}
}
