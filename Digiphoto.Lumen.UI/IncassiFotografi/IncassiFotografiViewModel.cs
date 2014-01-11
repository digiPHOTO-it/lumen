using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm;

namespace Digiphoto.Lumen.UI.IncassiFotografi {

	public class IncassiFotografiViewModel : ViewModelBase {

		/// <summary>
		/// Questo sarà il titolo della finestrella. 
		/// In questo modo si può riutilizzare la stessa maschera in contesti diversi.
		/// </summary>
		public String titolo {
			get;
			set;
		}

		public IncassiFotografiViewModel() : base() {
			this.incassiFotografi = new ObservableCollectionEx<IncassoFotografo>();
		}

		public IncassiFotografiViewModel( String titolo ) : this() {
			this.titolo = titolo;
		}

		public IncassiFotografiViewModel( String titolo, ICollection<IncassoFotografo> incassi ) : this(titolo) {
			this.replace( incassi );
		}

		public void clear() {
			this.incassiFotografi.Clear();
		}

		public void replace( ICollection<IncassoFotografo> incassi ) {
			incassiFotografi.Clear();
			foreach( IncassoFotografo incasso in incassi )
				this.incassiFotografi.Add( incasso );
		}

		// lista di tutti gli eventi
		public ObservableCollection<IncassoFotografo> incassiFotografi {
			get;
			set;
		}

	}
}
