using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi;

namespace Digiphoto.Lumen.Servizi.EliminaFotoVecchie
{


	public interface IEliminaFotoVecchieSrv : IServizio
    {
        IList<String> getListaCartelleDaEliminare();

		/// <summary>
		/// Elimina una intera giornata di un singolo fotografo
		/// </summary>
		/// <param name="pathCartellaDaEliminare">La cartella contenente le foto di un giorno di un fotografo</param>
        int elimina(String pathCartellaDaEliminare);

		Fotografo diChiSonoQuesteFoto(String pathCartellaDaEliminare);

		/// <summary>
		/// Eliminazione specifica di una o più foto richiesta dall'utente.
		/// Per esempio se il cliente vuole espressamente che siano eliminate le sue
		/// foto dall'archivio.
		/// </summary>
		/// <param name="fotoVecchie"></param>
		int elimina( IEnumerable<Fotografia> fotoVecchie );

		DateTime giornoFineAnalisi {
			get;
		}
    }

}
