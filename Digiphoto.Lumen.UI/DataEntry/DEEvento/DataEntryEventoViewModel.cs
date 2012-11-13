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

		protected override object passoCaricaDati() {
			IQueryable<Evento> q = entityRepositorySrv.Query();
			return q.OrderByDescending( gg => gg.id );
		}

		protected override void passoPreparaEdit( Evento evento ) {
			collectionView.Refresh();
		}
	}
}
