using System.Collections.Generic;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Input;
using Digiphoto.Lumen.Core.DatiDiEsempio;

namespace Digiphoto.Lumen.UI {

	public class SelettoreFotografoViewModel : ViewModelBase {

		public SelettoreFotografoViewModel() {

			this.DisplayName = "Selettore Fotografo";

			// istanzio la lista vuota
			fotografi = new ObservableCollection<Fotografo>();

			rileggereFotografi();
			istanziaNuovoFotografo();
		}

		#region Proprietà
		
		public Fotografo nuovoFotografo {
			get;
			set;
		}

		public string idFotografoNew {
			get {
				return nuovoFotografo.id;
			}
			set {
				nuovoFotografo.id = value;
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
		}

		private void creareNuovoFotografo() {

			// Salvo nel database
			fotografiReporitorySrv.addNew( nuovoFotografo );

			// Aggiungo alla collezione visuale (per non dover rifare la query)
			fotografi.Add( nuovoFotografo );

			// Svuoto per nuova creazione
			istanziaNuovoFotografo();

		}

		/// <summary>
		///  istanzia un oggetto nomeCartellaRecente tipo Fotografo, pronto per essere utilizzato nella creazione
		///  nomeCartellaRecente un nuovo fotografoSelezionato, in caso nell'elenco mancasse.
		/// </summary>
		private void istanziaNuovoFotografo() {

			// Questo è d'appoggio per la creazione nomeCartellaRecente un nuovo fotografoSelezionato al volo
			nuovoFotografo = new Fotografo();
			nuovoFotografo.attivo = true;
			nuovoFotografo.umano = true;

			OnPropertyChanged( "idFotografoNew" );
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
					_rileggereFotografiCommand = new RelayCommand( param => this.rileggereFotografi(), null, false );
				}
				return _rileggereFotografiCommand;
			}
		}

		private bool possoCreareNuovoFotografo {
			
			get {

				List<string> avvisi;
				List<string> errori;
				bool esito = nuovoFotografo != null && nuovoFotografo.Validate( out avvisi, out errori );

				/*
				 * Questa validazione non basta. Il PEM validator generato dal modello,
				 * è errato. Non tiene conto della lunghezza minima.
				 */
				if( esito == true ) {
					if( nuovoFotografo.id.Length < 4 )
						esito = false;
				}

				return esito;
			}
		}

		#endregion


	}
}
