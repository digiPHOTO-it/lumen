using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Digiphoto.Lumen.Model;
using System.Linq.Expressions;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Vendere;

namespace Digiphoto.Lumen.UI.DataEntry.DEFotografo {

	public class DataEntryFotografoViewModel : DataEntryViewModel<Fotografo> {

		protected override void passoPreparaAddNew( Fotografo fotografo ) {

			// Calcolo un codice numerico da 4 cifre
			object prox = entityRepositorySrv.getNextId();
			if( prox != null )
				fotografo.id = (string)prox;

			fotografo.attivo = true;
			fotografo.umano = true;
		}

		protected override void passoPrimaDiSalvare( Fotografo fotografo ) {
			collectionView.Refresh();
		}

		protected override object passoCaricaDati() {
			IQueryable<Fotografo> q = entityRepositorySrv.Query();
			return q.OrderByDescending( gg => gg.id );
		}

		protected override void passoPreparaEdit( Fotografo fotografo ) {
			collectionView.Refresh();
		}
	}
}
