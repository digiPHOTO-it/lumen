using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Digiphoto.Lumen.Model;
using System.Linq.Expressions;

namespace Digiphoto.Lumen.UI.DataEntry.DEGiornata {

	public class DataEntryGiornataViewModel : DataEntryViewModel<Giornata> {

/*
		public Nullable<Decimal> squadratura {
			get {

				Giornata g = this.collectionView.CurrentItem as Giornata;
				if( g == null )
					return null;
				else {
					return g.incassoPrevisto - g.incassoDichiarato;
				}

			}
		}
*/
		protected override void passoPreparaAddNew( Giornata giornata ) {
			giornata.id = DateTime.Today;
			giornata.orologio = DateTime.Now;
		}

		protected override void passoPrimaDiSalvare( Giornata giornata ) {
			// Qui si possono fare delle sistemazioni del caso.
		}

		protected override object passoCaricaDati() {
			/*
			Expression<Func<Giornata, Boolean>> filtro = gg => (gg.incassoDichiarato > 100);
			return entityRepositorySrv.Query( filtro );
			*/
			// TODO si potrebbe fermare ad una settimana prima ?

			IQueryable<Giornata> q = entityRepositorySrv.Query();
			return q.OrderByDescending( gg => gg.id );
		}
	}

}
