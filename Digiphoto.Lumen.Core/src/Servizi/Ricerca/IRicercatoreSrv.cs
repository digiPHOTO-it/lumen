using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core;

namespace Digiphoto.Lumen.Servizi.Ricerca {

	public interface IRicercatoreSrv : IServizio {

		List<Fotografia> cerca( ParamCercaFoto param );

		/// <summary>
		/// Ritorna il numero totale di elementi del risultato della query.
		/// </summary>
		/// <param name="param"></param>
		/// <returns>Un numero intero positivo da 0 in poi indicante il numero di elementi selezionati dalla query</returns>
		int conta( ParamCercaFoto param );

		/// <summary>
		/// Senza modificare i filtri di ricerca, vado a vedere in quale pagina si trova la foto indicata.
		/// </summary>
		/// <param name="totPagine"></param>
		/// <param name="param"></param>
		/// <returns></returns>
		int ricercaPaginaDelFotogramma( int numFotogramma, int paginaMin, int paginaMax, ParamCercaFoto param );

		ICollection<Carrello> cerca(ParamCercaCarrello param);

		List<string> cercaNomi( ParamCercaFoto param );

	}

	public enum Ordinamento {
		Asc,
		Desc
	}
}
