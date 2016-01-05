using System.Collections.Generic;

namespace Digiphoto.Lumen.Core.Collections {

	public interface ISelettore<T> {
        
		/// <summary>
		/// Serve per spegnere la selezione. L'elemento o gli elementi selezionati vengono spenti
		/// </summary>
		void deselezionareTutto();

		/// <summary>
		/// Ritorna il numero di elementi SELEZIONATI della collezione
		/// </summary>
		int countSelezionati {
			get;
		}

		/// <summary>
		/// Ritorna il numero di elementi TOTALE della collezione
		/// </summary>
		int countTotali {
			get;
		}

		IEnumerator<T> getEnumeratorSelezionati();
	}
}
