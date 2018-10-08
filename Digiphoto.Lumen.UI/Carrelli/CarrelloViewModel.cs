using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using System.ComponentModel;
using System.Windows.Data;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;
using System.Windows.Forms;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Vendere;
using System.Windows.Input;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Config;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Servizi.Masterizzare;
using Digiphoto.Lumen.Util;
using System.IO;
using Digiphoto.Lumen.UI.IncassiFotografi;
using Digiphoto.Lumen.UI.Dialogs.SelezionaStampante;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.UI.Util;
using Digiphoto.Lumen.UI.Main;
using Digiphoto.Lumen.Core.Eventi;
using Digiphoto.Lumen.Servizi.EliminaFotoVecchie;
using Digiphoto.Lumen.UI.Carrelli.Masterizzare;
using Digiphoto.Lumen.UI.Mvvm.Event;
using Digiphoto.Lumen.Core.Database;

namespace Digiphoto.Lumen.UI.Carrelli {

	public class CarrelloViewModel : ViewModelBase, IObserver<MasterizzaMsg>, IObserver<GestoreCarrelloMsg>, IObserver<StampatoMsg>, IObserver<FotoModificateMsg>, IObserver<FotoEliminateMsg>, IObserver<RefreshMsg> {

		private BackgroundWorker _bkgIdrata = null;


		public CarrelloViewModel() {
			paramCercaCarrello = new ParamCercaCarrello( true );

			if( IsInDesignMode ) {
			} else {
				IObservable<MasterizzaMsg> observable = LumenApplication.Instance.bus.Observe<MasterizzaMsg>();
				observable.Subscribe( this );

				IObservable<GestoreCarrelloMsg> observableCarrello = LumenApplication.Instance.bus.Observe<GestoreCarrelloMsg>();
				observableCarrello.Subscribe( this );

				IObservable<StampatoMsg> observableStampato = LumenApplication.Instance.bus.Observe<StampatoMsg>();
				observableStampato.Subscribe( this );

				IObservable<FotoModificateMsg> observableModificate = LumenApplication.Instance.bus.Observe<FotoModificateMsg>();
				observableModificate.Subscribe( this );

				IObservable<RefreshMsg> observableRefresh = LumenApplication.Instance.bus.Observe<RefreshMsg>();
				observableRefresh.Subscribe( this );

				IObservable<FotoEliminateMsg> observableCancellate = LumenApplication.Instance.bus.Observe<FotoEliminateMsg>();
				observableCancellate.Subscribe( this );


				// Creo due view diverse per le righe del carrello
				rinfrescaViewRighe();

				if( carrelloCorrente.giornata == null || carrelloCorrente.giornata.Equals( DateTime.MinValue ) )
					carrelloCorrente.giornata = LumenApplication.Instance.stato.giornataLavorativa;



				// Non voglio ricercare nulla fino a che l'utente non me lo chiede
				//				ricercaPoiCreaViewCarrelliSalvati();

				IsErroriMasterizzazione = false;

				// Creo il worker che mi serve per idratare le immagini delle righe del carrello
				_bkgIdrata = new BackgroundWorker();
				_bkgIdrata.WorkerReportsProgress = false;  // per ora non mi complico la vita
				_bkgIdrata.WorkerSupportsCancellation = true; // per ora non mi complico la vita
				_bkgIdrata.DoWork += new DoWorkEventHandler( bkgIdrata_DoWork );

				// Creo il modello anche dei componenti di cui mi servo.
				incassiFotografiViewModel = new IncassiFotografiViewModel( "Dettaglio incassi/fotografo per il carrello" );

				copiaFotoRigaRadio = true;
				spostaFotoRigaSingolaRadio = true;
			}
		}

		/// <summary>
		/// Eseguo la ricerca dei carrelli salvati e poi creo la collectionView che serve ad alimentare la lista che sta a video.
		/// </summary>
		private void ricercaPoiCreaViewCarrelliSalvati() {
			carrelloExplorerSrv.cercaCarrelli( paramCercaCarrello );
			CarrelliSalvatiCv = CollectionViewSource.GetDefaultView( carrelloExplorerSrv.carrelli );
			OnPropertyChanged( "CarrelliSalvatiCv" );
		}

		private int indexMasterizzate = 0;
		private int indexStampate = 0;
		/// <summary>
		/// Creo le viste sulle collezioni di righe che rappresentano il carrello.
		/// </summary>
		private void rinfrescaViewRighe() {
			if( IsErroriMasterizzazione ) {
				RiCaFotoStampateCv = new ListCollectionView( new List<Fotografia>() );
			} else {
				if( RiCaFotoStampateCv != null && !RiCaFotoStampateCv.IsEmpty )
					indexStampate = RiCaFotoStampateCv.IndexOf( rigaCarrelloStampataSelezionata );
				// Creo la CollectionView delle rige stampate
				RiCaFotoStampateCv = new ListCollectionView( carrelloCorrente.righeCarrello.ToList() );
				RiCaFotoStampateCv.Filter = f => {
					return ((RigaCarrello)f).isTipoStampa;
				};
				if( indexStampate > -1 && RiCaFotoStampateCv.Count > indexStampate )
					rigaCarrelloStampataSelezionata = (RigaCarrello)RiCaFotoStampateCv.GetItemAt( indexStampate );
				else if( RiCaFotoStampateCv.Count > 0 )
					rigaCarrelloStampataSelezionata = (RigaCarrello)RiCaFotoStampateCv.GetItemAt( RiCaFotoStampateCv.Count - 1 );
			}

			if( RiCaFotoMasterizzateCv != null && !RiCaFotoMasterizzateCv.IsEmpty )
				indexMasterizzate = RiCaFotoMasterizzateCv.IndexOf( rigaCarrelloMasterizzataSelezionata );
			// Creo la CollectionView delle rige masterizzate
			RiCaFotoMasterizzateCv = new ListCollectionView( carrelloCorrente.righeCarrello.ToList() );
			RiCaFotoMasterizzateCv.Filter = f => {
				return ((RigaCarrello)f).discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA;
			};
			if( indexMasterizzate > -1 && RiCaFotoMasterizzateCv.Count > indexMasterizzate )
				rigaCarrelloMasterizzataSelezionata = (RigaCarrello)RiCaFotoMasterizzateCv.GetItemAt( indexMasterizzate );
			else if( RiCaFotoMasterizzateCv.Count > 0 )
				rigaCarrelloMasterizzataSelezionata = (RigaCarrello)RiCaFotoMasterizzateCv.GetItemAt( RiCaFotoMasterizzateCv.Count - 1 );


			OnPropertyChanged( "RiCaFotoStampateCv" );
			OnPropertyChanged( "RiCaFotoMasterizzateCv" );
		}

		#region Proprietà

		public IncassiFotografiViewModel incassiFotografiViewModel {
			get;
			private set;
		}

		/// <summary>
		/// E' il carrello su cui sto aggiungendo le foto
		/// </summary>
		public Digiphoto.Lumen.Model.Carrello carrelloCorrente {
			get {
				return venditoreSrv == null ? null : venditoreSrv.carrello;
			}
		}

		/// <summary>
		///  Uso questa particolare collectionView perché voglio tenere traccia nel ViewModel degli N-elementi selezionati.
		///  Sottolineo N perché non c'è supporto nativo per questo. Vedere README.txt nel package di questa classe.
		/// </summary>
		public ListCollectionView RiCaFotoStampateCv {
			get;
			private set;
		}

		public ListCollectionView RiCaFotoMasterizzateCv {
			get;
			private set;
		}

		public ICollectionView CarrelliSalvatiCv {
			get;
			private set;
		}

		/// <summary>
		/// Questa è la la riga corrente della lista di sinistra (foto stampate)
		/// </summary>
		public RigaCarrello _rigaCarrelloStampataSelezionata;
		public RigaCarrello rigaCarrelloStampataSelezionata {
			get {
				return _rigaCarrelloStampataSelezionata;
			}

			set {
				if( _rigaCarrelloStampataSelezionata != value ) {
					_rigaCarrelloStampataSelezionata = value;
					OnPropertyChanged( "rigaCarrelloStampataSelezionata" );
				}
			}
		}

		/// <summary>
		/// Questa è la la riga corrente della lista di destra (foto masterizzate)
		/// </summary>
		public RigaCarrello _rigaCarrelloMasterizzataSelezionata;
		public RigaCarrello rigaCarrelloMasterizzataSelezionata {
			get {
				return _rigaCarrelloMasterizzataSelezionata;
			}

			set {
				if( _rigaCarrelloMasterizzataSelezionata != value ) {
					_rigaCarrelloMasterizzataSelezionata = value;
					OnPropertyChanged( "rigaCarrelloMasterizzataSelezionata" );
				}
			}
		}

		private Digiphoto.Lumen.Servizi.Masterizzare.Fase _statoMasterizzazione = Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa;
		public Digiphoto.Lumen.Servizi.Masterizzare.Fase StatoMasterizzazione {
			get {
				return _statoMasterizzazione;
			}
			set {
				if( _statoMasterizzazione != value ) {
					_statoMasterizzazione = value;
					OnPropertyChanged( "StatoMasterizzazione" );
				}
			}
		}

		private bool _copiaFotoRigaRadio;
		public bool copiaFotoRigaRadio {
			get {
				return _copiaFotoRigaRadio;
			}
			set {
				if( value != _copiaFotoRigaRadio ) {
					_copiaFotoRigaRadio = value;
					OnPropertyChanged( "copiaFotoRigaRadio" );
				}
			}

		}

		private bool _spostaFotoRigaSingolaRadio;
		public bool spostaFotoRigaSingolaRadio {
			get {
				return _spostaFotoRigaSingolaRadio;
			}
			set {
				if( value != _spostaFotoRigaSingolaRadio ) {
					_spostaFotoRigaSingolaRadio = value;
					OnPropertyChanged( "spostaFotoRigaSingolaRadio" );
				}
			}

		}

		#region Ricerca

		private string _intestazioneSearch;
		public string IntestazioneSearch {
			get {
				return _intestazioneSearch;
			}
			set {
				if( _intestazioneSearch != value ) {
					_intestazioneSearch = value;
					OnPropertyChanged( "IntestazioneSearch" );
				}
			}
		}

		private bool _isVendutoChecked;
		public bool IsVendutoChecked {
			get {
				return _isVendutoChecked;
			}
			set {
				if( _isVendutoChecked != value ) {
					_isVendutoChecked = value;
					paramCercaCarrello.isVenduto = _isVendutoChecked;
					OnPropertyChanged( "IsVendutoChecked" );
				}
			}
		}


		private Carrello _carrelloSalvatoSelezionato;
		public Carrello CarrelloSalvatoSelezionato {
			get {
				return _carrelloSalvatoSelezionato;
			}

			set {
				if( _carrelloSalvatoSelezionato != value ) {
					_carrelloSalvatoSelezionato = value;
					OnPropertyChanged( "CarrelloSalvatoSelezionato" );
				}
			}
		}

		public bool possoClonareCarrello {
			get {
				return venditoreSrv.isPossibileClonareCarrello;
			}
		}

		public bool possoCaricareInGallery {
			get {
				return venditoreSrv.carrello.righeCarrello.Count > 0;
			}
		}

		#endregion Proprietà

		#region Servizi
		/// <summary>
		/// Ritorno la giornata lavorativa corrente
		/// </summary>
		public DateTime oggi {
			get {
				return LumenApplication.Instance.stato.giornataLavorativa;
			}
		}

		ICarrelloExplorerSrv carrelloExplorerSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<ICarrelloExplorerSrv>();
			}
		}

		private IVenditoreSrv venditoreSrv {
			get {
				return (IVenditoreSrv)LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();
			}
		}

		#endregion

		private ParamCercaCarrello _paramCercaCarrello;
		public ParamCercaCarrello paramCercaCarrello {
			get {
				return _paramCercaCarrello;
			}
			set {
				if( _paramCercaCarrello != value ) {
					_paramCercaCarrello = value;
					OnPropertyChanged( "paramCercaCarrello" );
				}
			}
		}

		bool _isErroriMasterizzazione;
		public bool IsErroriMasterizzazione {
			get {
				return _isErroriMasterizzazione;
			}
			set {
				if( _isErroriMasterizzazione != value ) {
					_isErroriMasterizzazione = value;
					OnPropertyChanged( "IsErroriMasterizzazione" );
				}
			}
		}

		private String _masterizzazionePorgress;
		public String MasterizzazionePorgress {
			get {
				return _masterizzazionePorgress;
			}
			set {
				if( value != _masterizzazionePorgress ) {
					_masterizzazionePorgress = value;
					OnPropertyChanged( "MasterizzazionePorgress" );
				}
			}
		}

		public string ScontoApplicato {
			get {
				return venditoreSrv.scontoApplicato == null ? null : Convert.ToString( venditoreSrv.scontoApplicato ) + "%";
			}
		}

		public decimal prezzoNettoTotale {
			get {
				return venditoreSrv.prezzoNettoTotale;
			}
		}

		public short? QuantitaRigaSelezionata {
			get {
				if( rigaCarrelloStampataSelezionata != null )
					return rigaCarrelloStampataSelezionata.quantita;
				else
					return null;
			}
		}

		public int sommatoriaFotoDaMasterizzare {
			get {
				return venditoreSrv.sommatoriaFotoDaMasterizzare;
			}
		}

		public int sommatoriaQtaFotoDaStampare {
			get {
				return venditoreSrv.sommatoriaQtaFotoDaStampare;
			}
		}

		public decimal sommatoriaPrezziFotoDaStampare {
			get {
				return venditoreSrv.sommatoriaPrezziFotoDaStampare;
			}
		}

		public string spazioFotoDaMasterizzate {
			get {
				return venditoreSrv.spazioFotoDaMasterizzate;
			}
		}

		public bool possovisualizzareQRcodeSelfService {
			get {
				return carrelloCorrente != null && carrelloCorrente.venduto == true && carrelloCorrente.visibileSelfService == true;
			}
		}

		#endregion

		#region Controlli

		public bool possoSalvareCarrello {
			get {

				if( IsInDesignMode )
					return true;
				bool posso = true;

				// Verifico che i dati minimi siano stati indicati
				if( posso && RiCaFotoStampateCv.IsEmpty && venditoreSrv.sommatoriaFotoDaMasterizzare <= 0 )
					posso = false;

				// Ce un errore nella masterizzazione 
				if( posso && IsErroriMasterizzazione )
					posso = false;

				return posso;
			}
		}

		public bool abilitaOperazioniCarrello {
			get {
				if( IsInDesignMode )
					return true;

				bool posso = true;

				//Verifico che il carrello non sia stato venduto
				if( posso && carrelloCorrente.venduto )
					posso = false;

				// Verifico che i dati minimi siano stati indicati
				if( posso && RiCaFotoStampateCv.IsEmpty && venditoreSrv.sommatoriaFotoDaMasterizzare <= 0 )
					posso = false;

				// Ce un errore nella masterizzazione 
				if( posso && IsErroriMasterizzazione )
					posso = false;

				return posso;
			}
		}

		public bool abilitaEliminaDischetto {
			get {
				if( IsInDesignMode )
					return true;

				bool posso = true;

				//Verifico che il carrello non sia stato venduto
				if( posso && carrelloCorrente.venduto )
					posso = false;

				// Verifico che i dati minimi siano stati indicati
				if( posso && venditoreSrv.sommatoriaFotoDaMasterizzare == 0 )
					posso = false;

				// Ce un errore nella masterizzazione 
				if( posso && IsErroriMasterizzazione )
					posso = false;

				return posso;
			}
		}

		public bool possoAggiornareQuantitaRiga( short delta ) {

			if( !abilitaEliminaRigaFoto( RigaCarrello.TIPORIGA_STAMPA ) )
				return false;

			if( rigaCarrelloStampataSelezionata.quantita + delta > 0 )
				return true;
			else
				return false;
		}

		public bool possoSpostareFotoRiga( object paramGenerico ) {

			if( possoEditareCarrello ) {

				// Controllo sulla riga corrente selezionata
				Type pType = paramGenerico.GetType();
				if( pType == typeof( String ) )
					return possoSpostareFotoRiga( (string)paramGenerico );

				// Controllo con la riga che mi arriva nel parametro
				Type rType = typeof( RigaCarrello );
				if( pType == rType || rType.IsAssignableFrom( pType ) )
					return true;
			}

			return false;
		}

		public bool possoSpostareFotoRiga( string discriminator ) {
			if( RigaCarrello.TIPORIGA_MASTERIZZATA.Equals( discriminator ) ) {
				//Se voglio spostare le foto nelle masterizzate devo avere qualcosa nelle stampate
				return rigaCarrelloStampataSelezionata != null;
			}

			if( RigaCarrello.TIPORIGA_STAMPA.Equals( discriminator ) ) {
				//Se voglio spostare le foto nelle stampate devo avere qualcosa nelle masterizzate
				return rigaCarrelloMasterizzataSelezionata != null;
			}

			return false;
		}

		public bool possoVisualizzareIncassiFotografi {
			get {
				return carrelloCorrente != null && carrelloCorrente.righeCarrello.Any();
			}
		}

		/// <summary>
		/// Mi dice se posso modificare i controlli del carrello
		/// </summary>
		public bool possoEditareCarrello {
			get {
				return carrelloCorrente != null && carrelloCorrente.venduto == false;
			}
		}

		/// <summary>
		/// Decido se e quando posso rimasterizzare un cd.
		/// Se il carrello è venduto e ci sono righe masterizzate
		/// </summary>
		public bool possoRimasterizzare {
			get {
				return (carrelloCorrente != null
						&& carrelloCorrente.venduto
						&& RiCaFotoMasterizzateCv.IsEmpty == false);
			}
		}

		/// <summary>
		/// Posso effetture operazioni sul cd
		/// </summary>
		public bool operazioniCd {
			get {
				return (carrelloCorrente != null
						&& carrelloCorrente.venduto == false
						&& RiCaFotoMasterizzateCv.IsEmpty == false);
			}
		}

		public bool abilitaEliminaRigaFoto( String discriminator ) {
			if( IsInDesignMode )
				return true;

			bool posso = true;

			//Verifico che il carrello non sia stato venduto
			if( posso && carrelloCorrente.venduto && !IsErroriMasterizzazione )
				posso = false;

			if( posso && discriminator == RigaCarrello.TIPORIGA_STAMPA ) {
				// Verifico che i dati minimi siano stati indicati
				if( posso && RiCaFotoStampateCv.IsEmpty )
					posso = false;

				if( posso && rigaCarrelloStampataSelezionata == null )
					posso = false;
			}

			if( posso && discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA ) {
				if( posso && RiCaFotoMasterizzateCv.IsEmpty )
					posso = false;

				if( posso && rigaCarrelloMasterizzataSelezionata == null )
					posso = false;
			}

			// Ce un errore nella masterizzazione 
			//if (posso && IsErroriMasterizzazione)
			//	posso = false;

			return posso;
		}

		/// <summary>
		/// Il carrello corrente deve essere modificabile,
		/// ed inoltre deve avere almeno una foto senza il cuore. 
		/// </summary>
		public bool possoeliminareRigheSenzaCuore {
			get {
			
				bool posso = true;
			
				if( ! IsInDesignMode )
					posso = (possoEditareCarrello && carrelloCorrente.righeCarrello.Any( r => r.isTipoStampa && r.fotografia != null && r.fotografia.miPiace != true ));

				return posso;
			}
		}

		public bool abilitaEliminaTutteFoto {
			get {
				if( IsInDesignMode )
					return true;

				bool posso = true;

				//Verifico che il carrello non sia stato venduto
				if( posso && carrelloCorrente.venduto && !IsErroriMasterizzazione )
					posso = false;

				// Verifico che i dati minimi siano stati indicati
				if( posso && RiCaFotoStampateCv.IsEmpty )
					posso = false;

				if( posso && rigaCarrelloStampataSelezionata == null )
					posso = false;

				return posso;
			}
		}

		public bool abilitaEliminaCarrello {
			get {
				if( IsInDesignMode )
					return true;

				bool posso = true;

				//Verifico che il carrello non sia stato venduto
				if( posso && carrelloCorrente.venduto )
					posso = false;

				// Verifico che i dati minimi siano stati indicati
				// Elimino solo se il carrello è stato caricato in caso contrario è transiente e quindi
				//posso fare svuota
				if( posso && RiCaFotoStampateCv.IsEmpty && RiCaFotoMasterizzateCv.IsEmpty )
					posso = false;

				// Elimino solo i carrelli Salvati
				if( posso && !venditoreSrv.isStatoModifica )
					posso = false;

				// Ce un errore nella masterizzazione 
				if( posso && IsErroriMasterizzazione )
					posso = false;

				return posso;
			}
		}

		public bool possoCaricareCarrelloSalvatoSelezionato {
			get {
				if( IsInDesignMode )
					return true;

				if( CarrelliSalvatiCv != null && CarrelliSalvatiCv.IsEmpty && !venditoreSrv.isStatoModifica )
					return false;

				if( CarrelloSalvatoSelezionato == null )
					return false;

				// Ce un errore nella masterizzazione 
				if( IsErroriMasterizzazione )
					return false;

				return true;
			}
		}

		public BitmapSource StatusStatoMasterizzazioneImage {
			get {
				// Decido qual'è la giusta icona da caricare per mostrare lo stato dello slide show (Running, Pause, Empty)

				// Non so perchè ma se metto il percorso senza il pack, non funziona. boh eppure sono nello stesso assembly.
				string uriTemplate = @"pack://application:,,,/Resources/##-16x16.png";

				Uri uri = null;

				if( StatoMasterizzazione == Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa ) {
					uri = new Uri( uriTemplate.Replace( "##", "ssCompleate" ) );
					IsErroriMasterizzazione = false;
				}
				if( StatoMasterizzazione == Digiphoto.Lumen.Servizi.Masterizzare.Fase.ErroreMedia ) {
					uri = new Uri( uriTemplate.Replace( "##", "ssErroreMedia" ) );
					IsErroriMasterizzazione = true;
					trayIconProvider.showError( "Avviso", "Errore Media\nVerifica il Cd e riprova!!", 5000 );
				}
				if( StatoMasterizzazione == Digiphoto.Lumen.Servizi.Masterizzare.Fase.ErroreSpazioDisco ) {
					uri = new Uri( uriTemplate.Replace( "##", "ssErroreMedia" ) );
					IsErroriMasterizzazione = true;
					trayIconProvider.showError( "Avviso", "Capacita del disco superata!!!", 5000 );
				}
				if( StatoMasterizzazione == Digiphoto.Lumen.Servizi.Masterizzare.Fase.InizioCopia ) {
					uri = new Uri( uriTemplate.Replace( "##", "ssBurn" ) );
					IsErroriMasterizzazione = false;
					trayIconProvider.showInfo( "Avviso", "Inizio Copia", 5000 );
				}
				if( StatoMasterizzazione == Digiphoto.Lumen.Servizi.Masterizzare.Fase.CopiaCompletata ) {
					uri = new Uri( uriTemplate.Replace( "##", "ssCompleate" ) );
					IsErroriMasterizzazione = false;
					trayIconProvider.showInfo( "Avviso", "CopiaCompletata", 5000 );
				}

				// TODO : questa bitmap viene creata un sacco di volte. Se metti un breakpoint qui, si ferma troppo spesso.
				return new BitmapImage( uri );
			}
		}

		#endregion

		#region Metodi

		/// <summary>
		/// Provoco la rilettura delle property per fare aggiornare i controlli grafici.
		/// Questo metodo non deve essere chiamato direttamente, ma deve essere subito da un cambio di stato del carrello
		/// pilotato dal VenditoreSrv. (Risponde al messaggio onNext(GestoreCarrelloMsg) 
		/// </summary>
		private void updateGUI() {
			if( !_bkgIdrata.IsBusy )
				_bkgIdrata.RunWorkerAsync();

			OnPropertyChanged( "carrelloCorrente" );
			OnPropertyChanged( "prezzoNettoTotale" );
			OnPropertyChanged( "ScontoApplicato" );
			OnPropertyChanged( "sommatoriaFotoDaMasterizzare" );
			OnPropertyChanged( "sommatoriaQtaFotoDaStampare" );
			OnPropertyChanged( "sommatoriaPrezziFotoDaStampare" );
			OnPropertyChanged( "possoVisualizzareIncassiFotografi" );
			OnPropertyChanged( "spazioFotoDaMasterizzate" );
			OnPropertyChanged( "operazioniCd" );
			OnPropertyChanged( "possoRimasterizzare" );
			OnPropertyChanged( "possoClonareCarrello" );
		}


		private bool verificaChiediConfermaSalvataggioTotaleAPagareZero() {
			bool procediPure = true;
			//Se ho un totale a pagare diverso da zero ritorno true e non chiedo conferma
			//altrimenti chiedo se voglio prosegiure
			if( venditoreSrv.carrello.totaleAPagare != 0 )
				return procediPure;

			if( venditoreSrv.carrello.totaleAPagare == 0 && prezzoNettoTotale > 0 ) {
				procediPure = false;
				StringBuilder msg = new StringBuilder( "Attenzione: stai per regalare il carrello.\nConfermi?" );
				dialogProvider.ShowConfirmation( msg.ToString(), "Richiesta conferma",
					( confermato ) => {
						procediPure = confermato;
					} );
			}

			return procediPure;
		}

		/// <summary>
		/// Devo mandare in stampa le foto selezionate
		/// Nel parametro mi arriva l'oggetto StampanteAbbinata che mi da tutte le indicazioni
		/// per la stampa: il formato carta e la stampante
		/// </summary>
		private void vendere() {
			_giornale.Debug( "Mi preparo a vendere questo carrello" );


			if( !venditoreSrv.isPossibileSalvareCarrello ) {
				string msg = venditoreSrv.msgValidaCarrello;
				if( msg == null )
					msg = "Il carrello non è validato.";
				dialogProvider.ShowError( msg, "Impossibile salvare il carrello", null );
				return;
			}

			//Verifico se ho un prezzo totaleAPagare(Forfettario) uguale a ZERO in caso notifico l'operatore che sta per regalare il carrello
			if( verificaChiediConfermaSalvataggioTotaleAPagareZero() == false )
				return;

			if( richiediDoveMasterizzare() == false )
				return;


			_giornale.Debug( "Sono pronto per vendere il carrello" );


			string msgErrore = venditoreSrv.vendereCarrello();
			if( msgErrore == null ) {

				//Controllo se ci sono stati errori nella masterizzazione/copia su chiavetta
				if( !IsErroriMasterizzazione ) {
					short totaleFotoMasterizzate = 0;
					short totoleFotoStampate = 0;
					short totoleErrori = 0;

					_giornale.Debug( "Non ci sono errori durante la masterizzazione" );

					foreach( RigaCarrello r in carrelloCorrente.righeCarrello ) {

						if( r.discriminator == RigaCarrello.TIPORIGA_STAMPA ) {
							totoleFotoStampate += (short)r.quantita;
						}

						if( r.discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA ) {
							totaleFotoMasterizzate += (short)r.quantita;  // Deve essere sempre 1.
						}
					}

					string msg = "Carrello venduto:" +
												"\nTotale: " + venditoreSrv.carrello.totaleAPagare +
												"\nN° Fotografie: " + totoleFotoStampate +
												"\nN° Foto Masterizzate: " + totaleFotoMasterizzate +
												"\nN° Errori: " + totoleErrori;

					if( venditoreSrv.carrello.idCortoSelfService != null ) {
						msg += "\n\nCod. Self-Service : " + venditoreSrv.carrello.idCortoSelfService;
					}

					dialogProvider.ShowMessage( msg, "Avviso" );

					_giornale.Info( msg );

					//Creo un nuovo carrello
					creaNuovoCarrello();
				} else {
					_giornale.Warn( "Riscontrati errori durante la masterizzazione" );
					GestoreCarrelloMsg msg = new GestoreCarrelloMsg( this );
					msg.fase = Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.ErroreMasterizzazione;
					msg.descrizione = "Verificare il Cd e riprovare a Masterizzare";
					LumenApplication.Instance.bus.Publish( msg );
				}
			} else {
				_giornale.Error( "carrello non è stato salvato correttamente: " + msgErrore );
				dialogProvider.ShowError( "Attenzione: Il carrello non è stato salvato correttamente\r\n" + msgErrore, "ERRORE", null );
			}

		}

		private bool richiediDoveMasterizzare() {

			bool ret = true;

			// Se non ho nessuna foto da masterizzare, esco subito.
			if( sommatoriaFotoDaMasterizzare > 0 ) {

				// Creo il ViewModel per la popup
				using( ScegliMasterizzaTargetViewModel scegliMasterizzaTargetViewModel = new ScegliMasterizzaTargetViewModel() ) {

					// Apro la popup lanciando un evento
					var ea = new OpenPopupRequestEventArgs {
						requestName = "ScegliMasterizzaTargetPopup",
						viewModel = scegliMasterizzaTargetViewModel
					};

					RaisePopupDialogRequest( ea );

					if( ea.mioDialogResult == true ) {

						// Ricavo la cartella ed eventualmente il tipo di drive
						string folder = scegliMasterizzaTargetViewModel.cartella;
						DriveType driveType = scegliMasterizzaTargetViewModel.getDriveType( folder );

						if( driveType == DriveType.CDRom )
							ret = masterizzaSulMasterizzatore( folder );
						else
							ret = masterizzaSuCartella( folder );

					}
				}

/*
				switch( Configurazione.UserConfigLumen.masterizzaTarget ) {

					case MasterizzaTarget.Masterizzatore:
						ret = masterizzaSulMasterizzatore();
						break;

					case MasterizzaTarget.Cartella:
						ret = masterizzaSuCartella();
						break;
				}
*/
				
			}

			return ret;
		}

		private bool masterizzaSulMasterizzatore() {
			return masterizzaSulMasterizzatore( Configurazione.UserConfigLumen.defaultMasterizzatore );
        }

		private bool masterizzaSulMasterizzatore( string nomeCartella ) {

			bool procediPure = true;
			venditoreSrv.setDatiDischetto( MasterizzaTarget.Masterizzatore, nomeCartella );

			dialogProvider.ShowConfirmation( "Voi continuare la masterizzazione sul CD/DVD ?",
				"Richiesta conferma",
					( confermato ) => {
						procediPure = confermato;
					} );

			if( !procediPure ) {

				string chiavettaPath = scegliCartellaDoveMasterizzare();
				if( chiavettaPath == null ) {
					return false;
				}

				venditoreSrv.setDatiDischetto( MasterizzaTarget.Cartella, chiavettaPath );
				_giornale.Debug( "Masterizzo i files su : " + chiavettaPath );

			}

			return true;
		}

		private bool masterizzaSuCartella() {
			return masterizzaSuCartella( Configurazione.UserConfigLumen.defaultChiavetta );
		}

		private bool masterizzaSuCartella( string nomeCartella ) {

			// cartella
			if( !testCartellaMasterizza( nomeCartella ) ) {
				nomeCartella = scegliCartellaDoveMasterizzare();
				if( nomeCartella == null ) {
					return false;
				}
			}

			_giornale.Debug( "Masterizzo i files su : " + nomeCartella );
			venditoreSrv.setDatiDischetto( MasterizzaTarget.Cartella, nomeCartella );

			return true;
		}
		

		String scegliCartellaDoveMasterizzare() {

			string chiavettaPath = Configurazione.UserConfigLumen.defaultChiavetta;

			if( !Configurazione.isFuoriStandardCiccio ) {
				System.Windows.Forms.FolderBrowserDialog fBD = new System.Windows.Forms.FolderBrowserDialog();
				fBD.Description = "Scegliere la cartella dove masterizzare le foto";
				//fBD.RootFolder = Environment.SpecialFolder.Desktop;
				fBD.SelectedPath = Configurazione.UserConfigLumen.defaultChiavetta;
				DialogResult result = fBD.ShowDialog();

				chiavettaPath = result == DialogResult.OK ? fBD.SelectedPath : null;

			} else {
				chiavettaPath = UtilWinForm.scegliCartella();
			}


			if( !testCartellaMasterizza( chiavettaPath ) ) {
				if( chiavettaPath != null )  // se ho scelto qualcosa e non va bene, allora segnalo l'errore.
					dialogProvider.ShowError( "Percorso di masterizzazione non valido!\n" + chiavettaPath, "Cartella non valilda", null );
				return null;
			}
			return chiavettaPath;
		}


		/// <summary>
		/// Controllo che la cartella indicata sia valida, esiste ed è scrivibile.
		/// </summary>
		/// <param name="nomeCartella"></param>
		/// <returns>true se tutto ok.</returns>
		bool testCartellaMasterizza( string nomeCartella ) {

			if( nomeCartella == null )
				return false;

			if( !Directory.Exists( nomeCartella ) )
				return false;

			if( !PathUtil.isCartellaScrivibile( nomeCartella ) )
				return false;

			return true;
		}

		/// <summary>
		/// Devo mandare in stampa le foto selezionate
		/// Nel parametro mi arriva l'oggetto StampanteAbbinata che mi da tutte le indicazioni
		/// per la stampa: il formato carta e la stampante
		private void rimasterizza() {
			faseOld = Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa;
			venditoreSrv.rimasterizza();
		}

		private void calcolaTotali() {
			venditoreSrv.ricalcolaTotaleCarrello();
		}

		private void salvareCarrello() {
			//Verifico se ho un prezzo totaleAPagare(Forfettario) uguale a ZERO in caso notifico l'operatore che sta per regalare il carrello
			if( verificaChiediConfermaSalvataggioTotaleAPagareZero() == false )
				return;

			if( !venditoreSrv.isPossibileSalvareCarrello ) {
				string msg = venditoreSrv.msgValidaCarrello;
				if( msg == null )
					msg = "Il carrello non è validato.";
				dialogProvider.ShowError( msg, "Impossibile salvare il carrello", null );
				return;
			}

			if( String.IsNullOrEmpty( carrelloCorrente.intestazione ) ) {
				dialogProvider.ShowError( "Non è possibile salvare il carrello senza Intestazione", "Errore", null );
				return;
			}

			string msgErrore = venditoreSrv.salvareCarrello();

			if( msgErrore == null ) {
				dialogProvider.ShowMessage( "Carrello Salvato Correttamente", "Avviso" );

				creaNuovoCarrello();
			} else
				dialogProvider.ShowError( "Salvataggio carrello fallito\r\n" + msgErrore, "ERRORE", null );


		}

		private void eliminaCarrello() {
			bool procediPure = false;

			dialogProvider.ShowConfirmation( "Confermi la cancellazione del carrello:\n " + CarrelloSalvatoSelezionato.intestazione,
											 "Cancellazione carrello", ( confermato ) => {
												 procediPure = confermato;
											 } );
			if( !procediPure )
				return;

			Carrello carrelloDacanc = carrelloCorrente;

			bool rinfrescareLista = CarrelliSalvatiCv.Contains( carrelloDacanc );

			venditoreSrv.eliminareCarrello( carrelloDacanc );

			// aggiorno la lista rieseguendo la ricerca.
			if( rinfrescareLista )
				eseguireRicerca();

			creaNuovoCarrello();
		}

		private void eliminaRiga( String discriminator ) {
			if( chiediConfermaEliminazioneRiga( discriminator ) == false ) {
				return;
			}

			if( discriminator == RigaCarrello.TIPORIGA_STAMPA )
				venditoreSrv.eliminareRigaCarrello( rigaCarrelloStampataSelezionata );
			else if( discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA )
				venditoreSrv.eliminareRigaCarrello( rigaCarrelloMasterizzataSelezionata );

			// Quando elimino una riga, deseleziono la lista. Non voglio avere foto correnti
			rigaCarrelloStampataSelezionata = null;
		}

		private bool chiediConfermaEliminazioneRiga( String discriminator ) {
			StringBuilder msg = new StringBuilder( "Confermi rimozione riga dal carrello ?" );
			if( venditoreSrv.isStatoModifica )
				msg.Append( "\r\nL'operazione sarà definitiva solo se verrà salvato o venduto il carrello." );

			bool procediPure = false;
			dialogProvider.ShowConfirmation( msg.ToString(), "Rimozione foto da " + (discriminator == RigaCarrello.TIPORIGA_STAMPA ? "stampare" : "masterizzare") + " dal carrello",
				( confermato ) => {
					procediPure = confermato;
				} );

			return procediPure;
		}

		/// <summary>
		/// Dal carrello corrente, elimino tutte le righe di tipo STAMPA che non sono marcate con il cuore del LIKE (che ha messo il cliente con il tablet)
		/// </summary>
		private void eliminareRigheSenzaCuore() {

			if( chiediConfermaEliminazioneRigheSenzaCuore() == false )
				return;

			bool restaQui = true;
			int quante = 0;
			do {

				var rigaDacanc = carrelloCorrente.righeCarrello.FirstOrDefault( r => r.isTipoStampa && r.fotografia.miPiace != true );
				restaQui = rigaDacanc != null;
				if( rigaDacanc != null ) {
					venditoreSrv.eliminareRigaCarrello( rigaDacanc );
					++quante;
				}

			} while( restaQui );

			if( quante > 0 )
				dialogProvider.ShowMessage( "Rimosse " + quante + " foto dal carrello.\nRicordarsi di salvare il carrello", "Informazione" );
		}


		private bool chiediConfermaEliminazioneRigheSenzaCuore() {

			bool procediPure = false;
			int quante = carrelloCorrente.righeCarrello.Count( r => r.isTipoStampa && r.fotografia.miPiace != true );
			if( quante > 0 ) {

				StringBuilder msg = new StringBuilder( "Confermi rimozione di " + quante + " righe senza LIKE dal carrello ?" );
				if( venditoreSrv.isStatoModifica )
					msg.Append( "\r\nL'operazione sarà definitiva solo se verrà salvato o venduto il carrello." );

				dialogProvider.ShowConfirmation( msg.ToString(), "Rimozione foto da stampare dal carrello",
					( confermato ) => {
						procediPure = confermato;
					} );
			}


			return procediPure;
		}

		private void eliminaDischetto() {
			if( venditoreSrv.sommatoriaFotoDaMasterizzare <= 0 )
				return;
			if( chiediConfermaEliminazioneDischetto() == false )
				return;
			venditoreSrv.eliminareRigheCarrello( RigaCarrello.TIPORIGA_MASTERIZZATA );
		}

		private bool chiediConfermaEliminazioneDischetto() {

			StringBuilder msg = new StringBuilder( "Questa operazione rimuove il dischetto dal carrello:\n L'operazione è irreversibile.\nConfermi" );
			bool procediPure = false;
			dialogProvider.ShowConfirmation( msg.ToString(), "Richiesta conferma",
				( confermato ) => {
					procediPure = confermato;
				} );

			return procediPure;
		}

		private void eliminaTutteFotoCarrello() {
			if( chiediConfermaEliminazioneTutteFoto() == false )
				return;
			venditoreSrv.eliminareRigheCarrello( RigaCarrello.TIPORIGA_STAMPA );
		}

		private bool chiediConfermaEliminazioneTutteFoto() {

			StringBuilder msg = new StringBuilder( "Questa operazione rimuove tutte le foto dal carrello:\n L'operazione è irreversibile.\nConfermi" );
			bool procediPure = false;
			dialogProvider.ShowConfirmation( msg.ToString(), "Richiesta conferma",
				( confermato ) => {
					procediPure = confermato;
				} );

			return procediPure;
		}

		private void caricareCarrelloSalvatoSelezionato() {
			// Eventualmente stoppo lavoro precedente.
			if( _bkgIdrata.WorkerSupportsCancellation && _bkgIdrata.IsBusy )
				_bkgIdrata.CancelAsync();

			venditoreSrv.caricareCarrello( CarrelloSalvatoSelezionato );

			if( !_bkgIdrata.IsBusy )
				_bkgIdrata.RunWorkerAsync();
			// Non mettere altro codice qui sotto. Questa deve essere l'ultima operazione di questo metodo
		}

		private void bkgIdrata_DoWork( object sender, DoWorkEventArgs e ) {
			System.Threading.Thread.Sleep( 50 );  // Perdo un attimo di tempo per permettere al thread principale di rientrare e quindi di chiudere la sua UnitOfWork.
			BackgroundWorker worker = sender as BackgroundWorker;


			//			using( new UnitOfWorkScope() ) {

			Carrello c = carrelloCorrente;
			//				OrmUtil.forseAttacca<Carrello>( "Carrelli", ref c );

			if( carrelloCorrente != null )
				foreach( RigaCarrello ric in carrelloCorrente.righeCarrello ) {
					if( 1 == 1 ) {
						if( (worker.CancellationPending == true) ) {
							e.Cancel = true;
							break;
						} else {
							Fotografia foto = ric.fotografia;
							if( foto != null )
								AiutanteFoto.idrataImmaginiFoto( foto, IdrataTarget.Provino );
						}
					}
				}
			//			}

			// TODO
			// Non capisco perché Ma non dovrebbe servire ricreare la view. dovrebbe bastare questo:
			// OnPropertyChanged( "RiCaFotoStampateCv" );
			// l'effetto che non funziona, è che non vedo caricare i provini delle immaginette del carrello.
			// Sono costretto a ricreare tutta la collection-view. Mistero da sistemare.
			// BLUCA			rinfrescaViewRighe();

		}

		private void creaNuovoCarrello() {


			// Se ho delle righe ancora da salvare.. chiedo conferma
			if( venditoreSrv.possoChiudere() == false ) {
				bool procediPure = false;
				StringBuilder msg = new StringBuilder( "Eventuali modifiche al carrello corrente\nnon saranno salvate. Confermi?" );
				dialogProvider.ShowConfirmation( msg.ToString(), "Abbandonare il carrello",
					( confermato ) => {
						procediPure = confermato;
					} );

				if( !procediPure )
					return;
			}

			//Se ho avuto un errore di masterizzazione devo rinfrescarea anche le righe!!!
			//Mah
			if( IsErroriMasterizzazione ) {
				RiCaFotoStampateCv = new ListCollectionView( new List<Fotografia>() );
				RiCaFotoMasterizzateCv = new ListCollectionView( new List<Fotografia>() );
				rinfrescaViewRighe();
			}
			venditoreSrv.creareNuovoCarrello();
			MasterizzazionePorgress = "";
			StatoMasterizzazione = Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa;
			OnPropertyChanged( "StatusStatoMasterizzazioneImage" );
			incassiFotografiViewModel.clear();

			copiaFotoRigaRadio = true;
			spostaFotoRigaSingolaRadio = true;
		}

		private void svuotaCarrello() {
			// ELimino tutte le righe ma tengo buona la testata
			venditoreSrv.eliminareRigheCarrello( RigaCarrello.TIPORIGA_STAMPA );
			venditoreSrv.eliminareRigheCarrello( RigaCarrello.TIPORIGA_MASTERIZZATA );
			calcolaTotali();

			MasterizzazionePorgress = "";
			StatoMasterizzazione = Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa;
			OnPropertyChanged( "StatusStatoMasterizzazioneImage" );
			incassiFotografiViewModel.incassiFotografi.Clear();

			copiaFotoRigaRadio = true;
			spostaFotoRigaSingolaRadio = true;
		}



		private void azzeraParamRicerca() {
			paramCercaCarrello = new ParamCercaCarrello( true );
			IsVendutoChecked = false;
		}

		/// <summary>
		/// Chiamo il servizio che esegue la query sul database
		/// </summary>
		private void eseguireRicerca() {

			completaParametriRicerca();

			ricercaPoiCreaViewCarrelliSalvati();

			// Se non ho trovato nulla, allora avviso l'utente
			if( CarrelliSalvatiCv.IsEmpty )
				dialogProvider.ShowMessage( "Nessun Carrello trovato con questi filtri di ricerca", "AVVISO" );
		}

		private void completaParametriRicerca() {

			paramCercaCarrello.idratareImmagini = false;

			// Aggiungo eventuale parametro l'intestazione
			if( !String.IsNullOrEmpty( IntestazioneSearch ) ) {
				paramCercaCarrello.intestazione = IntestazioneSearch;
			} else {
				paramCercaCarrello.intestazione = null;
			}
		}

		private void aggiornareQuantitaRiga( short delta ) {
			if( rigaCarrelloStampataSelezionata.quantita + delta < 1 )
				rigaCarrelloStampataSelezionata.quantita = 1;
			else
				rigaCarrelloStampataSelezionata.quantita += delta;

			calcolaTotali();
		}

		private void spostaFotoRigaDxSx( object paramGenerico ) {
			Type pType = paramGenerico.GetType();

			if( pType == typeof( String ) )
				spostaFotoRigaDxSx( (string)paramGenerico );

			else {
				Type rType = typeof( RigaCarrello );
				if( pType == rType || rType.IsAssignableFrom( pType ) )
					spostaFotoRigaDxSx( (RigaCarrello)paramGenerico );
			}
        }

		private void spostaFotoRigaDxSx( RigaCarrello rigaCarrello ) {
			spostaFotoRighe( rigaCarrello );
        }

        private void spostaFotoRigaDxSx( string discriminator ) {

			bool onlySelected = spostaFotoRigaSingolaRadio;
			RigaCarrello qualeRiga = null;
			if( onlySelected )
				qualeRiga = discriminator == RigaCarrello.TIPORIGA_STAMPA ? rigaCarrelloMasterizzataSelezionata : rigaCarrelloStampataSelezionata;

			if( copiaFotoRigaRadio ) {
				copiaSpostaFotoRighe( discriminator, qualeRiga );
			} else {
				spostaFotoRighe( discriminator, qualeRiga );
			}
		}

		private void spostaFotoRighe( object paramGenerico ) {

			Type pType = paramGenerico.GetType();

			if( pType == typeof( String ) )
				spostaFotoRighe( (string)paramGenerico );

			else {
				Type rType = typeof( RigaCarrello );
				if( pType == rType || rType.IsAssignableFrom( pType ) ) {
					RigaCarrello r = (RigaCarrello)paramGenerico;
					var newDiscrim = r.getDiscriminatorOpposto();
					spostaFotoRighe( newDiscrim, r );
				}
			}
		}


		/// <summary>
		/// Sposto le foto dalla sezione masterizzate a quella stampe e viceversa.
		/// </summary>
		/// <param name="newDiscriminator">destinazione</param>
		/// <param name="rigaCarrello">Se indicata lavoro solo su quella. Se null, allora tutte</param>
		private void spostaFotoRighe( string newDiscriminator, RigaCarrello rigaCarrello )
        {
			// Sposto da STAMPE -> MASTERIZZATE
            if( newDiscriminator == RigaCarrello.TIPORIGA_MASTERIZZATA ) {
                if( rigaCarrello != null )
					venditoreSrv.spostareRigaCarrello( rigaCarrello );
				else
					venditoreSrv.spostareTutteRigheCarrello( RigaCarrello.getDiscriminatorOpposto(newDiscriminator), null);
            }

			// Sposto da MASTERIZZATE -> STAMPE
			if( newDiscriminator == RigaCarrello.TIPORIGA_STAMPA )
            {
                SelezionaStampanteDialog d = new SelezionaStampanteDialog();
                bool? esito = d.ShowDialog();

                if (esito == true)
                {
                    ParametriDiStampa parametriDiStampa = new ParametriDiStampa();
                    parametriDiStampa.FormatoCarta = d.formatoCarta;
                    parametriDiStampa.NomeStampante = d.nomeStampante;
                    parametriDiStampa.Quantita = 1;
                    parametriDiStampa.PrezzoLordoUnitario = d.formatoCarta.prezzo;
                    parametriDiStampa.PrezzoNettoTotale = d.formatoCarta.prezzo;

                    if ( rigaCarrello != null)
                    {
                        //associo il nuovo formato carta alla riga ed anche la stampante
                        rigaCarrello.formatoCarta = parametriDiStampa.FormatoCarta;
                        rigaCarrello.nomeStampante = parametriDiStampa.NomeStampante;
                        rigaCarrello.quantita = parametriDiStampa.Quantita;
                        rigaCarrello.prezzoLordoUnitario = parametriDiStampa.PrezzoLordoUnitario;
                        rigaCarrello.prezzoNettoTotale = parametriDiStampa.PrezzoNettoTotale;

                        venditoreSrv.spostareRigaCarrello(rigaCarrello);
                    }    
                    else
                    {
                        venditoreSrv.spostareTutteRigheCarrello( RigaCarrello.getDiscriminatorOpposto(newDiscriminator), parametriDiStampa);
                    }

                    rinfrescaViewRighe();
                }

                d.Close();
            }
        }
		
        private void copiaSpostaFotoRighe( string newDiscriminator, RigaCarrello qualeRiga )
        {
            // CopioSposto da STAMPE -> MASTERIZZATE
            if (newDiscriminator == RigaCarrello.TIPORIGA_MASTERIZZATA) {
                if( qualeRiga != null )
					venditoreSrv.copiaSpostaRigaCarrello( qualeRiga, null );
				else
                    venditoreSrv.copiaSpostaTutteRigheCarrello( RigaCarrello.getDiscriminatorOpposto(newDiscriminator), null);
            }

            // CopioSposto da MASTERIZZATE -> STAMPE
            if (newDiscriminator == RigaCarrello.TIPORIGA_STAMPA)
            {
                SelezionaStampanteDialog d = new SelezionaStampanteDialog();
                bool? esito = d.ShowDialog();

                if (esito == true)
                {
                    ParametriDiStampa parametriDiStampa = new ParametriDiStampa();
                    parametriDiStampa.FormatoCarta = d.formatoCarta;
                    parametriDiStampa.NomeStampante = d.nomeStampante;
                    parametriDiStampa.Quantita = 1;
                    parametriDiStampa.PrezzoLordoUnitario = d.formatoCarta.prezzo;
                    parametriDiStampa.PrezzoNettoTotale = d.formatoCarta.prezzo;
					parametriDiStampa.BordiBianchi = (!Configurazione.UserConfigLumen.autoZoomNoBordiBianchi);

					if( qualeRiga != null)
                    {
                        venditoreSrv.copiaSpostaRigaCarrello( qualeRiga, parametriDiStampa );
                    }else
                    {
                        venditoreSrv.copiaSpostaTutteRigheCarrello( RigaCarrello.getDiscriminatorOpposto(newDiscriminator), parametriDiStampa);
                    } 
                }

                d.Close();
            }
        }

		private void copiaSpostaFotoRiga( object paramGenerico ) {

			Type pType = paramGenerico.GetType();

			if( pType == typeof( String ) )
				copiaSpostaFotoRiga( (string)paramGenerico );

			else {
				Type rType = typeof( RigaCarrello );
				if( pType == rType || rType.IsAssignableFrom( pType ) ) {
					RigaCarrello riga = (RigaCarrello)paramGenerico;
					String newDiscriminator = RigaCarrello.getDiscriminatorOpposto( riga.discriminator );
					copiaSpostaFotoRighe( newDiscriminator, riga );
				}
			}
		}

		private void visualizzareIncassiFotografi() {

			// Aggiorno la collezione: non la ricreo perché è già bindata
			venditoreSrv.ricalcolaProvvigioni();
			incassiFotografiViewModel.replace( carrelloCorrente.incassiFotografi );
		}

		public void clonareCarrello() {

			// Spengo le selezioni sulle righe
			rigaCarrelloStampataSelezionata = null;
			rigaCarrelloMasterizzataSelezionata = null;

			venditoreSrv.clonareCarrello();

			updateGUI();

			rinfrescaViewRighe();

			dialogProvider.ShowMessage( "Carrello Clonato.\nOra stai lavorando sulla copia.", "Informazione" );
		}

		public void caricareInGallery() {

			// TODO migliorare qui. Se la gallery è vuota, non chiedere conferma.			
/*
			bool procediPure = false;
			String testo = "Svuotare la gallery e caricare le foto presenti nel carrello.\nConfermi?";
			dialogProvider.ShowConfirmation( testo, "Richiesta conferma",
				( confermato ) => {
					procediPure = confermato;
				} );

			if( !procediPure )
				return;
*/
			// L'operazione di modifica gallery deve farla la gallery stessa. Pubblico un messaggio
			GestoreCarrelloMsg msg = new GestoreCarrelloMsg( this );
			msg.fase = Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.VisualizzareInGallery;
			// msg.descrizione = "Verificare il Cd e riprovare a Masterizzare";
			LumenApplication.Instance.bus.Publish( msg );


			// Pubblico un messaggio di richiesta cambio pagina. Voglio tornare sulla gallery
			CambioPaginaMsg cambioPaginaMsg = new CambioPaginaMsg( this );
			cambioPaginaMsg.nuovaPag = "GalleryPag";
			LumenApplication.Instance.bus.Publish( cambioPaginaMsg );
		}

		void visualizzareQRcodeSelfService() {

			string prefix = UnitOfWorkScope.currentDbContext.InfosFisse.First().urlPrefixSelfServiceWeb;
			
			string url = prefix + "/Carrello/Details/" + carrelloCorrente.id;

			// Apro la popup lanciando un evento
			var ea = new OpenPopupRequestEventArgs {
				requestName = "QRcodeSelfServicePopup",
				param = url
			};

			RaisePopupDialogRequest( ea );

			if( ea.mioDialogResult == true ) {
			}

		}

		#endregion Metodi

		#region Comandi

		private RelayCommand _visualizzareIncassiFotografiCommand;
		public ICommand visualizzareIncassiFotografiCommand
		{
			get
			{
				if (_visualizzareIncassiFotografiCommand == null)
				{
					_visualizzareIncassiFotografiCommand = new RelayCommand( param => visualizzareIncassiFotografi(),
					                                                         param => possoVisualizzareIncassiFotografi );
				}
				return _visualizzareIncassiFotografiCommand;
			}
		}

		private RelayCommand _creaNuovoCarrelloCommand;
		public ICommand creaNuovoCarrelloCommand
		{
			get
			{
				if (_creaNuovoCarrelloCommand == null)
				{
					_creaNuovoCarrelloCommand = new RelayCommand(param => creaNuovoCarrello(), param => true, false);
				}
				return _creaNuovoCarrelloCommand;
			}
		}

		private RelayCommand _eliminaCarrelloCommand;
		public ICommand eliminaCarrelloCommand
		{
			get
			{
				if (_eliminaCarrelloCommand == null)
				{
					_eliminaCarrelloCommand = new RelayCommand(param => eliminaCarrello(), param => abilitaEliminaCarrello, true);
				}
				return _eliminaCarrelloCommand;
			}
		}

		private RelayCommand _salvaCarrelloCommand;
		public ICommand salvaCarrelloCommand
		{
			get
			{
				if (_salvaCarrelloCommand == null)
				{
					// Non salvo alla fine perchè già salvo all'interno del metodo
					_salvaCarrelloCommand = new RelayCommand( param => salvareCarrello(), 
					                                          param => possoSalvareCarrello, 
															  false);
				}
				return _salvaCarrelloCommand;
			}
		}

		private RelayCommand _svuotaCarrelloCommand;
		public ICommand svuotaCarrelloCommand
		{
			get
			{
				if (_svuotaCarrelloCommand == null)
				{
					_svuotaCarrelloCommand = new RelayCommand(param => svuotaCarrello(), param => abilitaOperazioniCarrello, true);
				}
				return _svuotaCarrelloCommand;
			}
		}

		private RelayCommand _eliminaRigaCommand;
		public ICommand eliminaRigaCommand
		{
			get
			{
				if (_eliminaRigaCommand == null)
				{
					_eliminaRigaCommand = new RelayCommand(param => eliminaRiga((String)param), param => abilitaEliminaRigaFoto((String)param), true);
				}
				return _eliminaRigaCommand;
			}
		}

		private RelayCommand _eliminareRigheSenzaCuoreCommand;
		public ICommand eliminareRigheSenzaCuoreCommand {
			get {
				if( _eliminareRigheSenzaCuoreCommand == null ) {
					_eliminareRigheSenzaCuoreCommand = new RelayCommand( param => eliminareRigheSenzaCuore(),
																		 param => possoeliminareRigheSenzaCuore );
				}
				return _eliminareRigheSenzaCuoreCommand;
			}
		}

		private RelayCommand _eliminaTutteFotoCommand;
        public ICommand eliminaTutteFotoCommand
        {
            get
            {
                if (_eliminaTutteFotoCommand == null)
                {
                    _eliminaTutteFotoCommand = new RelayCommand(param => eliminaTutteFotoCarrello(), param => abilitaEliminaTutteFoto, true);
                }
                return _eliminaTutteFotoCommand;
            }
        }

		private RelayCommand _eliminaDischettoCommand;
		public ICommand eliminaDischettoCommand
		{
			get
			{
				if (_eliminaDischettoCommand == null)
				{
					_eliminaDischettoCommand = new RelayCommand(param => eliminaDischetto(), param => abilitaEliminaDischetto, true);
				}
				return _eliminaDischettoCommand;
			}
		}
		
		private RelayCommand _vendereCommand;
		public ICommand vendereCommand
		{
			get
			{
				if (_vendereCommand == null)
				{
					_vendereCommand = new RelayCommand(param => vendere(), param => abilitaOperazioniCarrello, false);
				}
				return _vendereCommand;
			}
		}

		private RelayCommand _rimasterizzaCommand;
		public ICommand rimasterizzaCommand
		{
			get
			{
				if (_rimasterizzaCommand == null)
				{
					_rimasterizzaCommand = new RelayCommand( p => rimasterizza(),
															 p => possoRimasterizzare, false );
				}
				return _rimasterizzaCommand;
			}
		}

        private RelayCommand _calcolaTotaliCommand;
        public ICommand calcolaTotaliCommand
        {
            get
            {
                if (_calcolaTotaliCommand == null)
                {
                    _calcolaTotaliCommand = new RelayCommand(param => calcolaTotali());
                }
                return _calcolaTotaliCommand;
            }
        }

		private RelayCommand _updateQuantitaCommand;
		public ICommand UpdateQuantitaCommand
        {
            get
            {
				if (_updateQuantitaCommand == null)
                {
					_updateQuantitaCommand = new RelayCommand( param => aggiornareQuantitaRiga( Convert.ToInt16(param) ), 
						                                       param => possoAggiornareQuantitaRiga( Convert.ToInt16(param) ) );
                }
				return _updateQuantitaCommand;
            }
        }

        private RelayCommand _spostaFotoRigaDxSxCommand;
        public ICommand SpostaFotoRigaDxSxCommand
        {
            get
            {
                if (_spostaFotoRigaDxSxCommand == null)
                {
                    _spostaFotoRigaDxSxCommand = new RelayCommand(param => spostaFotoRigaDxSx( param ),
                                                               param => possoSpostareFotoRiga( param ), false);
                }
                return _spostaFotoRigaDxSxCommand;
            }
        }

		private RelayCommand _caricareCarrelloSelezionatoCommand;
		public ICommand caricareCarrelloSelezionatoCommand
		{
			get
			{
				if (_caricareCarrelloSelezionatoCommand == null)
				{
					_caricareCarrelloSelezionatoCommand = new RelayCommand(param => caricareCarrelloSalvatoSelezionato(), param => possoCaricareCarrelloSalvatoSelezionato, false);
				}
				return _caricareCarrelloSelezionatoCommand;
			}
		}

		private RelayCommand _azzeraParamRicercaCommand;
		public ICommand azzeraParamRicercaCommand {
			get {
				if( _azzeraParamRicercaCommand == null ) {
					_azzeraParamRicercaCommand = new RelayCommand( param => azzeraParamRicerca() );
				}
				return _azzeraParamRicercaCommand;
			}
		}

		private RelayCommand _eseguireRicercaCommand;
		public ICommand eseguireRicercaCommand
		{
			get
			{
				if (_eseguireRicercaCommand == null)
				{
					_eseguireRicercaCommand = new RelayCommand(param => eseguireRicerca(), p => true, false );
				}
				return _eseguireRicercaCommand;
			}
		}

		private RelayCommand _clonareCarrelloCommand;
		public ICommand clonareCarrelloCommand {
			get {
				if (_clonareCarrelloCommand == null) {
					_clonareCarrelloCommand = new RelayCommand( param => clonareCarrello(), p => possoClonareCarrello );
				}
				return _clonareCarrelloCommand;
			}
		}

		private RelayCommand _caricareInGalleryCommand;
		public ICommand caricareInGalleryCommand {
			get {
				if( _caricareInGalleryCommand == null ) {
					_caricareInGalleryCommand = new RelayCommand( param => caricareInGallery(), p => possoCaricareInGallery, false );
				}
				return _caricareInGalleryCommand;
			}
		}

		private RelayCommand _visualizzareQRcodeSelfServiceCommand;
		public ICommand visualizzareQRcodeSelfServiceCommand {
			get {
				if( _visualizzareQRcodeSelfServiceCommand == null ) {
					_visualizzareQRcodeSelfServiceCommand = new RelayCommand( param => visualizzareQRcodeSelfService(), p => possovisualizzareQRcodeSelfService, false );
				}
				return _visualizzareQRcodeSelfServiceCommand;
			}
		}

		

		#endregion Comandi

		#region MemBus

		public void OnCompleted()
		{
			throw new NotImplementedException();
		}

		public void OnError(Exception error)
		{
			throw new NotImplementedException();
		}


		private Digiphoto.Lumen.Servizi.Masterizzare.Fase faseOld = Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa;
		public void OnNext(MasterizzaMsg msg)
		{
			if( msg.fase == Servizi.Masterizzare.Fase.CopiaCompletata ) {
			
				if( msg.esito == Eventi.Esito.Ok ) {

					// Apro la cartella di destinazione cosi verifico le foto
					if( Directory.Exists( msg.cartella ) ) {
						System.Diagnostics.Process.Start( new System.Diagnostics.ProcessStartInfo() {
							FileName = msg.cartella,
							UseShellExecute = true,
							Verb = "open"
						} );
					}

				}
			}

/*
			System.Diagnostics.Trace.WriteLine("");
			System.Diagnostics.Trace.WriteLine("[TotFotoNonAggiunte]: " + msg.totFotoNonAggiunte);
			System.Diagnostics.Trace.WriteLine("[TotFotoAggiunte]: " + msg.totFotoAggiunte);
			System.Diagnostics.Trace.WriteLine("[Esito]: " + msg.esito);
			System.Diagnostics.Trace.WriteLine("[FotoAggiunta]: " + msg.fotoAggiunta);
			System.Diagnostics.Trace.WriteLine("[Fase]: " + msg.fase);
			System.Diagnostics.Trace.WriteLine("[Result]: " + msg.result);
			System.Diagnostics.Trace.WriteLine("[Progress]: " + msg.progress);
*/			 

			StatoMasterizzazione = msg.fase;
			if (faseOld != StatoMasterizzazione)
			{
				faseOld = StatoMasterizzazione;
			if (msg.result == null)
			{
				MasterizzazionePorgress = msg.progress + " %";
			}
			else
			{
				MasterizzazionePorgress = msg.result;
				
				if (msg.result.Equals("Error Media"))
				{
					StatoMasterizzazione = Digiphoto.Lumen.Servizi.Masterizzare.Fase.ErroreMedia;
				}
			}
			OnPropertyChanged("StatusStatoMasterizzazioneImage");
          }
		}

		public void OnNext(GestoreCarrelloMsg msg)
		{
			bool ricreaCV = false;

			// Qui cambiano soltanto gli attributi con il totale del carrello
			if(msg.fase== Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.UpdateCarrello){
				if( msg.descrizione == "cambiate-righe" )
					ricreaCV = true;
			}

			if( carrelloCorrente.righeCarrello.Count() != (RiCaFotoStampateCv.Count + RiCaFotoMasterizzateCv.Count)  )
				ricreaCV = true;

			if( msg.fase == Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.LoadCarrelloSalvato ||
				msg.fase == Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.CreatoNuovoCarrello )
			{
				// Qui devo ricreare la collection view delle righe
				ricreaCV = true;
			}

			if( ricreaCV )
				rinfrescaViewRighe();

			updateGUI();
		}

		public void OnNext(StampatoMsg value)
		{
			// TODO forse non serve più ascoltare questo messaggio.
			//      Ora il messaggio lo ascolta il MainWindow per dare avviso all'utente

			// TODO capire chi deve stornare l'importo dal carrello
		}

		public void OnNext( FotoEliminateMsg msg ) {

			// Se non ho un carrello, non devo fare niente.
			if( carrelloCorrente == null )
				return;

			if( carrelloCorrente.venduto ) {

				// Siccome il carrello è venduto e quindi non si può modificare, lo rileggo.
				bool trovata = false;
				// venditoreSrv.caricareCarrello( carrelloCorrente );
				foreach( Fotografia fDel in msg.listaFotoEliminate ) {
					trovata = carrelloCorrente.righeCarrello.Any( r => fDel.Equals( r.fotografia ) );
					if( trovata )
						break;
				}

				if( trovata )
					venditoreSrv.caricareCarrello( carrelloCorrente );

			} else {

				foreach( Fotografia fDel in msg.listaFotoEliminate ) {

					// Faccio il controllo con l'id numerico perché l'equals non mi da sicurezze
					RigaCarrello riga = carrelloCorrente.righeCarrello.SingleOrDefault( r => fDel.Equals( r.fotografia ) );
					if( riga != null )
						venditoreSrv.eliminareRigaCarrello( riga );
				}
			}
		}
	

		// E' stata modificata una o più foto
		public void OnNext( FotoModificateMsg msg ) {
			
			Digiphoto.Lumen.Servizi.EntityRepository.IEntityRepositorySrv<Fotografia> er = LumenApplication.Instance.getServizioAvviato<Digiphoto.Lumen.Servizi.EntityRepository.IEntityRepositorySrv<Fotografia>>();

			// Devo controllare se nel carrello sono presenti delle foto che hanno subito delle modifiche.
			// In tal caso devo aggiornarmi
			foreach( Fotografia fMod in msg.fotos ) {

				RigaCarrello riga = carrelloCorrente.righeCarrello.SingleOrDefault( r => fMod.Equals( r.fotografia ) );
				if( riga != null ) {

					// rimpiazzo la fotografia dentro la riga carrello. Lo faccio nel thread della UI altrimenti non mi si rinfresca il carrello.

					App.Current.Dispatcher.BeginInvoke(
						new Action( () => {
							venditoreSrv.rimpiazzaFotoInRiga( riga, fMod );
						}
					) );

				}
			}

		}

		public void OnNext( RefreshMsg value ) {
			updateGUI();
		}

		protected override void OnDispose() {

			this._bkgIdrata.CancelAsync();
			this._bkgIdrata.Dispose();

			base.OnDispose();
		}

		#endregion

	}
}
