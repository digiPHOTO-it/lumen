﻿using System;
using System.Collections.Generic;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Model;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Core.DatiDiEsempio;
using System.Windows.Input;

namespace Digiphoto.Lumen.UI {
	
	public class SelettoreEventoViewModel : ViewModelBase {

		public SelettoreEventoViewModel() {

			eventi = new ObservableCollection<Evento>();

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
				eventi.Add( ev );
		}

		private void creareNuovoEvento() {

			// Salvo nel database
			eventiReporitorySrv.addNew( nuovoEvento );
				
			// Aggiungo alla collezione visuale (per non dover rifare la query)
			eventi.Add( nuovoEvento );
				
			// Svuoto per nuova creazione
			istanziaNuovoEvento();
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
					_refreshEventiCommand = new RelayCommand( param => this.refreshEventi(), null, false );
				}
				return _refreshEventiCommand;
			}
		}

		private bool possoCreareNuovoEvento {
			get {
				List<string> avvisi;
				List<string> errori;

				bool esito = nuovoEvento != null && nuovoEvento.Validate( out avvisi, out errori );
				if( esito == true ) {

					// la dimensione minima non viene testata correttamente
					if( nuovoEvento.descrizione.Length < 4 )
						esito = false;
				}

				return esito;
			}
		}

		#endregion

	}
}