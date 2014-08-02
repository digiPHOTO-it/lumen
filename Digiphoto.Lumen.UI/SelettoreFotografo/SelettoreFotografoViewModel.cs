using System.Collections.Generic;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Input;
using Digiphoto.Lumen.Core.DatiDiEsempio;
using Digiphoto.Lumen.Database;
using System;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.UI {

	public class SelettoreFotografoViewModel : ViewModelBase, IObserver<EntityCambiataMsg> {

		public SelettoreFotografoViewModel() {

			this.DisplayName = "Selettore Fotografo";

			// istanzio la lista vuota
			fotografi = new ObservableCollection<Fotografo>();

			IObservable<EntityCambiataMsg> observable = LumenApplication.Instance.bus.Observe<EntityCambiataMsg>();
			observable.Subscribe( this );

			rileggereFotografi();
			istanziaNuovoFotografo();
		}

		#region Proprietà
		
		public Fotografo nuovoFotografo {
			get;
			set;
		}

		public string cognomeNomeFotogafoNew {
			get {
				return nuovoFotografo.cognomeNome;
			}
			set {
				nuovoFotografo.cognomeNome = value;
			}
		}

		/// <summary>
		/// Tutti i fotografi da visualizzare
		/// </summary>
		public ObservableCollection<Fotografo> fotografi {
			get;
			set;
		}

		/// <summary>
		/// Il fotografo attualmente selezionato
		/// </summary>
		Fotografo _fotografoSelezionato;
		public Fotografo fotografoSelezionato {
			get {
				return _fotografoSelezionato;
			}
			set {
				if( value != _fotografoSelezionato ) {
					_fotografoSelezionato = value;
					OnPropertyChanged( "fotografoSelezionato" );
				}
			}
		}


		public IEntityRepositorySrv<Fotografo> fotografiReporitorySrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografo>>();
			}
		}
		#endregion

		#region Metodi

		private void rileggereFotografi() {
			rileggereFotografi( false );
		}

		private void rileggereFotografi( object param ) {

			// Decido se devo dare un avviso all'utente
			Boolean avvisami = false;

			if( param != null ) {
				if( param is Boolean )
					avvisami = (Boolean)param;
				if( param is string )
					Boolean.TryParse( param.ToString(), out avvisami );
			}
			// ---

			IEnumerable<Fotografo> listaF = null;
			if( IsInDesignMode ) {

				// genero dei dati casuali
				DataGen<Fotografo> dg = new DataGen<Fotografo>();
				listaF = dg.generaMolti( 5 );

			} else {
				listaF = fotografiReporitorySrv.getAll();
			}

			// purtoppo pare che rimpiazzare il reference con uno nuovo, causa dei problemi.
			// Non posso istanziare nuovamente la lista, ma la devo svuotare e ripopolare.
			fotografi.Clear();
			foreach( Fotografo f in listaF )
				fotografi.Add( f );

			if( avvisami && dialogProvider != null )
				dialogProvider.ShowMessage( "Riletti " + fotografi.Count + " fotografi", "Successo" );
		}

		private void creareNuovoFotografo() {

			try {

				if( "*".Equals( nuovoFotografo.id ) )
					nuovoFotografo.id = (string)fotografiReporitorySrv.getNextId();

				// Salvo nel database
				fotografiReporitorySrv.addNew( nuovoFotografo );

				fotografiReporitorySrv.saveChanges();

				// Non c'è più bisogno perché mi rinfresco sulla ui tramite un messaggio
				// Aggiungo alla collezione visuale (per non dover rifare la query)
				//	fotografi.Add( nuovoFotografo );

				// Svuoto per nuova creazione
				istanziaNuovoFotografo();


			} catch( Exception ee ) {
				// probabilmente sono state inserite le iniziali doppie (not unique)
				fotografiReporitorySrv.delete( nuovoFotografo );
				dialogProvider.ShowError( ErroriUtil.estraiMessage( ee ), "Salva Fotografo", null );
			}

		}

		/// <summary>
		///  istanzia un oggetto di tipo Fotografo, pronto per essere utilizzato nella creazione
		///  di un nuovo fotografoSelezionato, in caso nell'elenco mancasse.
		/// </summary>
		private void istanziaNuovoFotografo() {

			// Questo è d'appoggio per la creazione nomeCartellaRecente un nuovo fotografoSelezionato al volo
			nuovoFotografo = new Fotografo();
			nuovoFotografo.attivo = true;
			nuovoFotografo.umano = true;
			nuovoFotografo.cognomeNome = "";
			nuovoFotografo.id = "*";

			OnPropertyChanged( "cognomeNomeFotogafoNew" );
			OnPropertyChanged( "nuovoFotografo" );
		}

		#endregion

		#region Comandi

		private RelayCommand _creareNuovoCommand;
		public ICommand creareNuovoCommand {
			get {
				if( _creareNuovoCommand == null ) {
					_creareNuovoCommand = new RelayCommand( param => this.creareNuovoFotografo(),
															param => this.possoCreareNuovoFotografo,
															true );
				}
				return _creareNuovoCommand;
			}
		}


		private RelayCommand _rileggereFotografiCommand;
		public ICommand rileggereFotografiCommand {
			get {
				if( _rileggereFotografiCommand == null ) {
					_rileggereFotografiCommand = new RelayCommand( param => this.rileggereFotografi( param ), null, false );
				}
				return _rileggereFotografiCommand;
			}
		}

		private bool possoCreareNuovoFotografo {
			
			get {
				return nuovoFotografo != null && OrmUtil.isValido( nuovoFotografo );
			}
		}

		#endregion



		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( EntityCambiataMsg value ) {

			// Qualcuno ha spataccato nella tabella dei fotografi. Rileggo tutto
			if( value.type == typeof( Fotografo ) )
				rileggereFotografiCommand.Execute( false );
		}
	}
}
