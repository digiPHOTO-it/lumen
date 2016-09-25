﻿using Digiphoto.Lumen.Model;
using System.Collections.Generic;

namespace Digiphoto.Lumen.Core.Collections {

	public interface ISelettore<T> {

		/// <summary>
		/// Serve per spegnere la selezione. L'elemento o gli elementi selezionati vengono spenti
		/// </summary>
		void deselezionareTutto();

		void deselezionare( T elem );


		#region Tutti

		/// <summary>
		/// Ritorna il numero di elementi TOTALE della collezione
		/// </summary>
		int countElementiTotali {
			get;
		}
		IEnumerator<T> getEnumeratorElementiTutti();
		IEnumerable<T> getElementiTutti();

		#endregion Tutti

		#region Selezionati

		/// <summary>
		/// Ritorna il numero di elementi SELEZIONATI della collezione
		/// </summary>
		int countElementiSelezionati {
			get;
		}

		bool isAlmenoUnElementoSelezionato {
			get;
		}

		IEnumerator<T> getEnumeratorElementiSelezionati();
		IEnumerable<T> getElementiSelezionati();

		#endregion
	}
}
