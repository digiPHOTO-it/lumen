using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Digiphoto.Lumen.Model;
using System.Linq.Expressions;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Vendere;

namespace Digiphoto.Lumen.UI.DataEntry.DEEvento {

	public class DataEntryEventoViewModel : DataEntryViewModel<Evento> {

		protected override void passoPreparaAddNew( Evento evento ) {
			evento.id = Guid.NewGuid();
			evento.attivo = true;
		}

		protected override void passoPrimaDiSalvare( Evento fotografo ) {
			collectionView.Refresh();
		}

		protected override IEnumerable<Evento> passoCaricaDati() {
			IQueryable<Evento> q = entityRepositorySrv.Query();
			IOrderedQueryable<Evento> rr = q.OrderByDescending( gg => gg.id );
			return rr;
		}

		protected override void passoPreparaEdit( Evento evento ) {
			collectionView.Refresh();
		}
	}
}
