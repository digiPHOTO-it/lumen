using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Digiphoto.Lumen.Model;
using System.Linq.Expressions;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Vendere;
using Digiphoto.Lumen.UI.IncassiFotografi;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Input;

namespace Digiphoto.Lumen.UI.DataEntry.DEGiornata {

	public class DataEntryGiornataViewModel : DataEntryViewModel<Giornata> {

		#region Proprietà

		public IncassiFotografiViewModel incassiFotografiViewModel {
			get;
			private set;
		}

		private IVenditoreSrv venditoreSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();
			}
		}

		#endregion Proprietà

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
			ricalcolareGiornata( giornata );
		}

		/// <summary>
		/// Eseguo i ricalcoli per la giornata indicata. Valorizzo l'incasso previsto sul bean
		/// </summary>
		/// <param name="giornata"></param>
		private void ricalcolareGiornata( Giornata giornata ) {

			// Ricavo l'incasso previsto per la giornata
			// NO: lo faccio sempre : Solo in inserimento ricalcolo l'incasso previsto
//			if( 1==1 || this.status == DataEntryStatus.New )
				giornata.incassoPrevisto = calcolaIncassoPrevisto( giornata.id );

			calcolaIncassiFotografiGiorno( giornata.id );
		}

		private Decimal calcolaIncassoPrevisto( DateTime giorno ) {
			if (giorno == DateTime.MinValue)
				return 0;
			else
				return venditoreSrv.calcolaIncassoPrevisto( giorno );
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
			ricalcolareGiornata( giornata );
			collectionView.Refresh();
		}


		private void calcolaIncassiFotografiGiorno( DateTime giorno ) {

			IList<IncassoFotografo> incassiFotografiGiorno = venditoreSrv.calcolaIncassiFotografiPrevisti( giorno );
			incassiFotografiViewModel = new IncassiFotografiViewModel( "Provvigioni fotografi del giorno " + giorno.ToString("d"), incassiFotografiGiorno );
			OnPropertyChanged( "incassiFotografiViewModel" );
		}

		void ricalcolareGiorno() {

			bool svuota = false;

			// Questo capita quando rinuncio all'inserimento di un record
			if (entitaCorrente == null)
				svuota = true;
			else
				if (entitaCorrente.id == DateTime.MinValue) {
					svuota = true;
					entitaCorrente.incassoPrevisto = 0;
				}

			if ( svuota ) {
				incassiFotografiViewModel = null;
                return;
			} else

				ricalcolareGiornata( entitaCorrente );
		}

		private RelayCommand _ricalcolareGiornoCommand;
		public ICommand ricalcolareGiornoCommand {
			get {
				if( _ricalcolareGiornoCommand == null ) {
					_ricalcolareGiornoCommand = new RelayCommand( param => ricalcolareGiorno(),
						                                          param => true,
															      false );
				}
				return _ricalcolareGiornoCommand;
			}
		}


	}
}
