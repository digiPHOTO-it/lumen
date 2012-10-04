using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Digiphoto.Lumen.Model;
using System.Linq.Expressions;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Vendere;

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

			// Ricavo l'incasso previsto per la giornata
			giornata.incassoPrevisto = calcolaIncassoPrevisto( giornata.id );
		}

		private Decimal calcolaIncassoPrevisto( DateTime giorno ) {
			return LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>().calcolaIncassoPrevisto( giorno );
		}

		protected override void passoPrimaDiSalvare( Giornata giornata ) {
			// Ribadisco per possibile cambio
			giornata.incassoPrevisto = calcolaIncassoPrevisto( giornata.id );
			collectionView.Refresh();
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

		protected override void passoPreparaEdit( Giornata giornata ) {
			giornata.incassoPrevisto = calcolaIncassoPrevisto( giornata.id );
			collectionView.Refresh();
		}
	}
}
