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

			this.DisplayName = "Scelta Fotografo";

			rileggiFotografi();
			istanziaNuovoFotografo();
		}

		#region Properties
		
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

		public ObservableCollection<Fotografo> fotografi {
			get;
			private set;
		}

		//public Fotografo fotografoSelezionato {
		//    get;
		//    set;
		//}


		public IEntityRepositorySrv<Fotografo> fotografiReporitorySrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografo>>();
			}
		}
		#endregion


		/// <summary>
		///  istanzia un oggetto di tipo Fotografo, pronto per essere utilizzato nella creazione
		///  di un nuovo fotografoSelezionato, in caso nell'elenco mancasse.
		/// </summary>
		private void istanziaNuovoFotografo() {

			// Questo è d'appoggio per la creazione di un nuovo fotografoSelezionato al volo
			nuovoFotografo = new Fotografo();
			nuovoFotografo.attivo = true;
			nuovoFotografo.umano = true;

			OnPropertyChanged( "idFotografoNew" );
			OnPropertyChanged( "nuovoFotografo" );
		}

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
					_rileggereFotografiCommand = new RelayCommand( param => this.rileggiFotografi(), null, false );
				}
				return _rileggereFotografiCommand;
			}
		}

		private bool possoCreareNuovoFotografo {
			get {
				List<string> avvisi;
				List<string> errori;
				return nuovoFotografo != null && nuovoFotografo.Validate( out avvisi, out errori );
			}
		}

		#endregion

		#region Azioni dei comandi
		private void rileggiFotografi() {

			IEnumerable<Fotografo> listaF = null;
			if( IsInDesignMode ) {

				// genero dei dati casuali
				DataGen<Fotografo> dg = new DataGen<Fotografo>();
				listaF = dg.generaMolti( 5 );
				
			} else {
				listaF = fotografiReporitorySrv.getAll();
			}
			fotografi = new ObservableCollection<Fotografo>( listaF );
//			OnPropertyChanged( "fotografi" );  // In teoria non dovrebbe servire perchè la collezione è già osservabile. Invece se non lo metto, mi rinfresca la lisa soltanto la prima volta.... boh!
		}

		private void creareNuovoFotografo() {

			// Salvo nel database
			fotografiReporitorySrv.addNew( nuovoFotografo );
				
			// Aggiungo alla collezione visuale (per non dover rifare la query)
			fotografi.Add( nuovoFotografo );
				
			// Svuoto per nuova creazione
			istanziaNuovoFotografo();

		}
		#endregion

	}
}
