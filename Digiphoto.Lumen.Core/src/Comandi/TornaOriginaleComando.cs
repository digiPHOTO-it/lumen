using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Imaging;

namespace Digiphoto.Lumen.Comandi {

	public class TornaOriginaleComando : Comando {

		internal override Esito esegui( Fotografia foto ) {
			foto.correzioni = null;
			RitoccoUtil.creaProvinoFoto( foto );
			return Esito.Ok;
		}
	}
}
