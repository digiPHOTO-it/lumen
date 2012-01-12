using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Core.Database;

namespace Digiphoto.Lumen.Comandi {

	public class TornaOriginaleComando : Comando {

		public TornaOriginaleComando() {
		}

		internal override Esito esegui( Fotografia foto ) {

			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;

			foto.correzioni.Clear();

			if( foto.imgRisultante != null ) {
				foto.imgRisultante.Dispose();
				foto.imgRisultante = null;
			}

			AiutanteFoto.creaProvinoFoto( foto );

			objContext.SaveChanges();

			return Esito.Ok;
		}
	}
}
