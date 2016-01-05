using System;
using System.Collections.Generic;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Model;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Core.DatiDiEsempio;
using Digiphoto.Lumen.Core.Collections;
using System.Windows.Input;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.Servizi.Ricerca;

namespace Digiphoto.Lumen.UI {

	public class SelettoreScaricoCardViewModel : ViewModelBase, ISelettore<ScaricoCard>, IObserver<EntityCambiataMsg> {

		public SelettoreScaricoCardViewModel() {

			scarichiCards = new ObservableCollectionEx<ScaricoCard>();

			IObservable<EntityCambiataMsg> observable = LumenApplication.Instance.bus.Observe<EntityCambiataMsg>();
			observable.Subscribe( this );

			refreshScarichiCards();
		}

		#region Proprietà

		// lista di tutti gli ScaricoCard
		public ObservableCollection<ScaricoCard> scarichiCards {
			get;
			set;
		}

		private MultiSelectCollectionView<ScaricoCard> _scarichiCardsCW;
        public MultiSelectCollectionView<ScaricoCard> scarichiCardsCW {
			get {
				return _scarichiCardsCW;
			}
			set {
				if( _scarichiCardsCW != value ) {
					_scarichiCardsCW = value;
					OnPropertyChanged( "scarichiCardsCW" );
				}
			}
		}

		private IEntityRepositorySrv<ScaricoCard> scarichiCardsReporitorySrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<ScaricoCard>>();
			}
		}

		private IFotoExplorerSrv fotoExplorerSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
			}
		}

		#endregion

		#region Metodi
		private void refreshScarichiCards() {
			refreshScarichiCards( false );
		}

		private void refreshScarichiCards( object param ) {

			// Decido se devo dare un avviso all'utente
			Boolean avvisami = false;

			if( param != null ) {
				if( param is Boolean )
					avvisami = (Boolean)param;
				if( param is string )
					Boolean.TryParse( param.ToString(), out avvisami );
			}
			// ---

			IEnumerable<ScaricoCard> lista;
			if( IsInDesignMode ) {
				DataGen<ScaricoCard> dataGen = new DataGen<ScaricoCard>();
				lista = dataGen.generaMolti( 4 );
			} else {
				lista = this.fotoExplorerSrv.loadUltimiScarichiCards();
			}

			// Ho notato che è meglio non ri-istanziare le collezione. La pulisco e poi la ricarico
			scarichiCards = new ObservableCollection<ScaricoCard>( lista );
            scarichiCards.Clear();
            foreach( ScaricoCard ev in lista )
				scarichiCards.Add( ev );

			// Costriusco anche la collection view per la selezione multipla
			scarichiCardsCW = new MultiSelectCollectionView<ScaricoCard>( scarichiCards );

			if( avvisami && dialogProvider != null )
				dialogProvider.ShowMessage( "Ricaricati " + scarichiCards.Count + " elementi", "Successo" );
		}

		#endregion

		#region Comandi

		private RelayCommand _refreshScarichiCardsCommand;
		public ICommand refreshScarichiCardsCommand {
			get {
				if( _refreshScarichiCardsCommand == null ) {
					_refreshScarichiCardsCommand = new RelayCommand( param => this.refreshScarichiCards( param ), null, false );
				}
				return _refreshScarichiCardsCommand;
			}
		}

		public int countSelezionati {
			get {
				return this.scarichiCardsCW.SelectedItems.Count;
			}
		}

		public IEnumerator<ScaricoCard> getEnumeratorSelezionati() {
			return this.scarichiCardsCW.SelectedItems.GetEnumerator();
		}

		#endregion


		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( EntityCambiataMsg value ) {

			// Qualcuno ha spataccato nella tabella degli ScarichiCards. Rileggo tutto
			if( value.type == typeof( ScaricoCard ) ) {

				App.Current.Dispatcher.BeginInvoke(
					new Action( () => {
						refreshScarichiCardsCommand.Execute( false );
					}
				) );

			}
		}
		
		public void deselezionareTutto() {
			if( scarichiCardsCW != null )
				scarichiCardsCW.DeselectAll();
		}
	}
}
