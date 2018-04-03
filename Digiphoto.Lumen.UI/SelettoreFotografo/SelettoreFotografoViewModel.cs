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
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Core.Collections;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;

namespace Digiphoto.Lumen.UI {

	public class SelettoreFotografoViewModel : ViewModelBase, ISelettore<Fotografo>, IObserver<EntityCambiataMsg> {

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

		private MultiSelectCollectionView<Fotografo> _fotografiCW;
		public MultiSelectCollectionView<Fotografo> fotografiCW
		{
			get
			{
				return _fotografiCW;
			}
			set
			{
				if( _fotografiCW != value ) {
					_fotografiCW = value;
					OnPropertyChanged( "fotografiCW" );
				}
			}
		}

		/// <summary>
		/// Il fotografo attualmente selezionato
		/// </summary>
		public IEnumerable<Fotografo> fotografiSelezionati {
			get {
				return fotografiCW == null ? null : fotografiCW.SelectedItems;
			}
		}

		public Fotografo fotografoSelezionato {
			get {
				if( fotografiCW == null || fotografiCW.SelectedItems.Count == 0 )
					return null;

				// Se ho solo un elemento, ritorno quello
				if( fotografiCW.SelectedItems.Count == 1 )
					return fotografiCW.SelectedItems[0];

				// Se ne ho piu di uno, può essere che ci siano dei NULL nella collezione
				if( fotografiCW.SelectedItems.Count > 1 ) {

					Fotografo selezionato = null;
					foreach( var f in fotografiCW.SelectedItems )
						if( f != null )
							if( selezionato == null )
								selezionato = f;
							else
								throw new InvalidOperationException( "Usare la selezione singola, oppure usare fotografiSelezionati" );
					return selezionato;
				}

				return null;  // qui non dovrebbe mai arrivare
			}

			set {
				if( value != null )
				{
					fotografiCW.seleziona( value );
				}
			}
		}

		public void forzaRefresh() {
			fotografiCW.Refresh();
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
			
			foreach( Fotografo f in listaF ){
				if (f.attivo)
				{
				fotografi.Add( f );
				}
			}
			// Costriusco anche la collection view per la selezione multipla
			fotografiCW = new MultiSelectCollectionView<Fotografo>( fotografi );

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

		public event SelezioneCambiataEventHandler selezioneCambiata;

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
			if( value.type == typeof( Fotografo ) ) {

//				App.Current.Dispatcher.BeginInvoke(
//					new Action( () => {
						rileggereFotografiCommand.Execute( false );
//					}
//				) );

			}
		}

		#region interfaccia ISelettore
		
		public void deselezionareTutto() {
			if( fotografiCW != null )
				fotografiCW.deselezionaTutto();
		}

		public IEnumerator<Fotografo> getEnumeratorElementiSelezionati() {
			return this.fotografiCW.SelectedItems.GetEnumerator();
		}

		public void deselezionareSingola( Fotografo elem ) {
			fotografiCW.SelectedItems.Clear();
		}

		public IEnumerable<Fotografo> getElementiSelezionati() {
			return fotografiCW.SelectedItems;
		}

		public int countElementiSelezionati {
			get {
				return fotografiCW == null ? 0 : fotografiCW.SelectedItems.Count;
			}
		}

		public IEnumerator<Fotografo> getEnumeratorElementiTutti() {
			return fotografi.GetEnumerator();
		}

		public IEnumerable<Fotografo> getElementiTutti() {
			return fotografi;
		}

		public int countElementiTotali {
			get {
				return this.fotografi == null ? 0 : this.fotografi.Count;
			}
		}

		public bool isAlmenoUnElementoSelezionato {
			get {
				return countElementiSelezionati > 0;
			}
		}

		public Fotografo ultimoElementoSelezionato {
			get {
				return fotografoSelezionato;
			}
		}
		#endregion interfaccia ISelettore
	}
}
