using System;
using System.Collections.Generic;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Model;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Core.DatiDiEsempio;
using System.Windows.Input;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Servizi.Ricerca;

namespace Digiphoto.Lumen.UI {

	public class SelettoreEventoViewModel : ViewModelBase, IObserver<EntityCambiataMsg> {

		public SelettoreEventoViewModel() {

			eventi = new ObservableCollectionEx<Evento>();

			IObservable<EntityCambiataMsg> observable = LumenApplication.Instance.bus.Observe<EntityCambiataMsg>();
			observable.Subscribe( this );

			refreshEventi();

			istanziaNuovoEvento();
		}

		#region Proprietà

		public Evento nuovoEvento {
			get;
			set;
		}

		// lista di tutti gli eventi
		public ObservableCollection<Evento> eventi {
			get;
			set;
		}

		private Evento _eventoSelezionato;
		public Evento eventoSelezionato {
			get {
				return _eventoSelezionato;
			}
			set {
				if( value != _eventoSelezionato ) {
					_eventoSelezionato = value;
					OnPropertyChanged( "eventoSelezionato" );

                    SvuotaFiltriMsg svuotaFiltriMsg = new SvuotaFiltriMsg(this);
                    LumenApplication.Instance.bus.Publish(svuotaFiltriMsg);
                }
			}
		}

		private IEntityRepositorySrv<Evento> eventiReporitorySrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Evento>>();
			}
		}
		#endregion

		#region Metodi
		private void refreshEventi() {
			refreshEventi( false );
		}

		private void refreshEventi( object param ) {

			// Decido se devo dare un avviso all'utente
			Boolean avvisami = false;

			if( param != null ) {
				if( param is Boolean )
					avvisami = (Boolean)param;
				if( param is string )
					Boolean.TryParse( param.ToString(), out avvisami );
			}
			// ---

			IEnumerable<Evento> lista;
			if( IsInDesignMode ) {
				DataGen<Evento> dataGen = new DataGen<Evento>();
				lista = dataGen.generaMolti( 4 );
			} else {
				lista = eventiReporitorySrv.getAll();
			}

			// Ho notato che è meglio non ri-istanziare le collezione.
			eventi.Clear();
			foreach( Evento ev in lista )
			{
				if(ev.attivo){
				eventi.Add( ev );
				}
			}
			if( avvisami && dialogProvider != null )
				dialogProvider.ShowMessage( "Ricaricati " + eventi.Count + " elementi", "Successo" );
		}

		private void creareNuovoEvento() {

			// Salvo nel database
			eventiReporitorySrv.addNew( nuovoEvento );

			eventiReporitorySrv.saveChanges();


			// Prima di azzerare l'oggetto, mi prendo il messaggio da visualizzare
			string testoMsg = "Creato nuovo evento: " + nuovoEvento.descrizione;
	
			// Svuoto per nuova creazione
			istanziaNuovoEvento();

			// Avviso l'utente
			if( dialogProvider != null )
				dialogProvider.ShowMessage( testoMsg, "Successo" );

			// Invio un messaggio di conferma
			Messaggio confermaMsg = new Messaggio( this, testoMsg );
			confermaMsg.showInStatusBar = true;
			LumenApplication.Instance.bus.Publish( confermaMsg );
		}

		private void istanziaNuovoEvento() {
			nuovoEvento = new Evento();
			nuovoEvento.id = Guid.NewGuid();

			OnPropertyChanged( "nuovoEvento" );
		}
		#endregion

		#region Comandi

		private RelayCommand _creareNuovoEventoCommand;
		public ICommand creareNuovoEventoCommand {
			get {
				if( _creareNuovoEventoCommand == null ) {
					_creareNuovoEventoCommand = new RelayCommand( param => this.creareNuovoEvento(),
															param => this.possoCreareNuovoEvento,
															true );
				}
				return _creareNuovoEventoCommand;
			}
		}


		private RelayCommand _refreshEventiCommand;
		public ICommand refreshEventiCommand {
			get {
				if( _refreshEventiCommand == null ) {
					_refreshEventiCommand = new RelayCommand( param => this.refreshEventi( param ), null, false );
				}
				return _refreshEventiCommand;
			}
		}

		private bool possoCreareNuovoEvento {
			get {
				return nuovoEvento != null && OrmUtil.isValido( nuovoEvento );
			}
		}

		#endregion


		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( EntityCambiataMsg value ) {
			// Qualcuno ha spataccato nella tabella degli eventi. Rileggo tutto
			if( value.type == typeof( Evento ) )
				refreshEventiCommand.Execute( false );
		}
	}
}
