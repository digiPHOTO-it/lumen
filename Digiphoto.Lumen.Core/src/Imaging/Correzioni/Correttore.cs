using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Ritoccare;

namespace Digiphoto.Lumen.Imaging.Correzioni {


	public abstract class Correttore : TypeConverter, ICorrettore {

		public abstract IImmagine applica( IImmagine immagineSorgente, Correzione correzione );

	}
}
