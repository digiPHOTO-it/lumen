using System.Collections.Generic;

namespace Digiphoto.Lumen.Core.Collections {

	public interface ISelettore<T> {
        
		/// <summary>
		/// Serve per spegnere la selezione. L'elemento o gli elementi selezionati vengono spenti
		/// </summary>
		void deselezionareTutto();

		int countSelezionati {
			get;
		}

		IEnumerator<T> getEnumeratorSelezionati();
	}
}
