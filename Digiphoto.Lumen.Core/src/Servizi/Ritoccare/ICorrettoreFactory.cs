using System;
using Digiphoto.Lumen.Imaging.Correzioni;

namespace Digiphoto.Lumen.Servizi.Ritoccare {

	public interface ICorrettoreFactory {

		Correttore creaCorrettore( TipoCorrezione tipoCorrezione );

		Correttore creaCorrettore( Type tipoCorrezione );

		Correttore creaCorrettore<T>() where T : Correzione;
	}
}
