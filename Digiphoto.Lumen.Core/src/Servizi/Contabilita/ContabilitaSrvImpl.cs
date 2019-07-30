using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Servizi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Core.Servizi.Contabilita {

	public class ContabilitaSrvImpl : ServizioImpl, IContabilitaSrv  {

		public List<DateTime> getListaGiorniNonChiusi() {


			// Stabilisco il range inizio fine di controllo

			// La data di inizio, la ricavo come prima data dell'anno in corso.
			DateTime inizio = DateTime.MinValue;

			var lista  = UnitOfWorkScope.currentDbContext.Giornate
				.Where( g => g.id.Year == DateTime.Today.Year )
				.Select( g => g.id );

			if( lista.Any() ) {
				inizio = lista.Min();
			} else
				return new List<DateTime>();


			DateTime fine = DateTime.Today;

			// Creo lista di tutte le date da inzio stagione ad oggi
			var dates = new List<DateTime>();
			for( var dt = inizio; dt <= fine; dt = dt.AddDays( 1 ) )
				dates.Add( dt );

			// Quersta è la lista dei giorni lavorati e chiusi
			var listaGiorniChiusi = UnitOfWorkScope.currentDbContext.Giornate
				.Where( g => g.id >= inizio && g.id <= fine )
				.Select( g => g.id );

			// Questa è la lista di tutte le date tranne quelle lavorate
			var listaGiorniMancanti = dates
				.Except( listaGiorniChiusi )
				.OrderBy( g => g );

			// Questa è la lista delle giornate mancanti
			return listaGiorniMancanti.ToList();
		}
	}
}
