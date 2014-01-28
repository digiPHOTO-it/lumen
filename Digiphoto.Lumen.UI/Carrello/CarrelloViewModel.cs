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
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.Core.Database;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Servizi.Masterizzare;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Database;
using System.IO;
using Digiphoto.Lumen.UI.IncassiFotografi;
using Digiphoto.Lumen.UI.Dialogs.SelezionaStampante;
using Digiphoto.Lumen.Servizi.Ritoccare;

namespace Digiphoto.Lumen.UI
{
	public class CarrelloViewModel : ViewModelBase, IObserver<MasterizzaMsg>, IObserver<GestoreCarrelloMsg>, IObserver<StampatoMsg>, IObserver<FotoModificateMsg>
    {
		private Digiphoto.Lumen.Servizi.Masterizzare.Fase StatoMasterizzazione = Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa;

		private BackgroundWorker _bkgIdrata = null;


        public CarrelloViewModel()
        {
			paramCercaCarrello = new ParamCercaCarrello();

            if (IsInDesignMode)
            {
            }
            else
			{
				IObservable<MasterizzaMsg> observable = LumenApplication.Instance.bus.Observe<MasterizzaMsg>();
				observable.Subscribe(this);

				IObservable<GestoreCarrelloMsg> observableCarrello = LumenApplication.Instance.bus.Observe<GestoreCarrelloMsg>();
				observableCarrello.Subscribe(this);

				IObservable<StampatoMsg> observableStampato = LumenApplication.Instance.bus.Observe<StampatoMsg>();
				observableStampato.Subscribe(this);

				IObservable<FotoModificateMsg> observableModificate = LumenApplication.Instance.bus.Observe<FotoModificateMsg>();
				observableModificate.Subscribe( this );

				// Creo due view diverse per le righe del carrello
				rinfrescaViewRighe();
				
				if (carrelloCorrente.giornata == null || carrelloCorrente.giornata.Equals(DateTime.MinValue))
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
			}
		}

		/// <summary>
		/// Eseguo la ricerca dei carrelli salvati e poi creo la collectionView che serve ad alimentare la lista che sta a video.
		/// </summary>
		private void ricercaPoiCreaViewCarrelliSalvati() {
			carrelloExplorerSrv.cercaCarrelli(paramCercaCarrello);
			CarrelliSalvatiCv = CollectionViewSource.GetDefaultView(carrelloExplorerSrv.carrelli);
			OnPropertyChanged("CarrelliSalvatiCv");
		}


		

		/// <summary>
		/// Creo le viste sulle collezioni di righe che rappresentano il carrello.
		/// </summary>
		private void rinfrescaViewRighe() {
			if (IsErroriMasterizzazione)
			{
				RiCaFotoStampateCv = null;
			}
			else
			{
				// Creo la CollectionView delle rige stampate
				RiCaFotoStampateCv = new ListCollectionView( carrelloCorrente.righeCarrello.ToList() );
				RiCaFotoStampateCv.Filter = f => {
					return ((RigaCarrello)f).discriminator == Carrello.TIPORIGA_STAMPA;
				};

				// Creo la CollectionView delle rige masterizzate
				RiCaFotoMasterizzateCv = new ListCollectionView( carrelloCorrente.righeCarrello.ToList() );
				RiCaFotoMasterizzateCv.Filter = f => {
					return ((RigaCarrello)f).discriminator == Carrello.TIPORIGA_MASTERIZZATA;
				};
			}
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
		private Digiphoto.Lumen.Model.Carrello _carrelloCorrente;
		public Digiphoto.Lumen.Model.Carrello carrelloCorrente {
			get {
				_carrelloCorrente = venditoreSrv.carrello;
				_carrelloCorrente.PropertyChanged += _carrelloCorrente_PropertyChanged;
				return venditoreSrv.carrello;
			}
		}

		void _carrelloCorrente_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "prezzoDischetto" ||
				e.PropertyName == "totaleAPagare")
			{
				OnPropertyChanged("prezzoNettoTotale");
				OnPropertyChanged("ScontoApplicato");
			}
		}

		/// <summary>
		///  Uso questa particolare collectionView perché voglio tenere traccia nel ViewModel degli N-elementi selezionati.
		///  Sottolineo N perché non c'è supporto nativo per questo. Vedere README.txt nel package di questa classe.
		/// </summary>
		public ListCollectionView RiCaFotoStampateCv
		{
			get;
			private set;
		}

		public ListCollectionView RiCaFotoMasterizzateCv {
			get;
			private set;
		}

		public ICollectionView CarrelliSalvatiCv
		{
			get;
			private set;
		}

		/// <summary>
		/// Questa è la la riga corrente della lista di sinistra (foto stampate)
		/// </summary>
		public RigaCarrello _rigaCarrelloStampataSelezionata;
		public RigaCarrello rigaCarrelloStampataSelezionata
		{
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

		#region Ricerca

		private string _intestazioneSearch;
		public string IntestazioneSearch
		{
			get
			{
				return _intestazioneSearch;
			}
			set
			{
				if (_intestazioneSearch !=value)
				{
					_intestazioneSearch = value;
					OnPropertyChanged("IntestazioneSearch");
				}
			}
		}

		public bool? IsVendutoChecked
		{
			get
			{
				return (paramCercaCarrello.isVenduto);
			}
			set
			{
				paramCercaCarrello.isVenduto = value;
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

		#endregion Proprietà

		#region Servizi
		/// <summary>
		/// Ritorno la giornata lavorativa corrente
		/// </summary>
		public DateTime oggi
		{
			get
			{
				return LumenApplication.Instance.stato.giornataLavorativa;
			}
		}

		ICarrelloExplorerSrv carrelloExplorerSrv
		{
			get
			{
				return LumenApplication.Instance.getServizioAvviato<ICarrelloExplorerSrv>();
			}
		}

		private IVenditoreSrv venditoreSrv
		{
			get
			{
				return (IVenditoreSrv)LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();
			}
		}

		#endregion

		private ParamCercaCarrello _paramCercaCarrello;
		public ParamCercaCarrello paramCercaCarrello
		{
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
		public bool IsErroriMasterizzazione
		{
			get
			{
				return _isErroriMasterizzazione;
			}
			set
			{
				_isErroriMasterizzazione = value;
				OnPropertyChanged("IsErroriMasterizzazione");
			}
		}

		private String _masterizzazionePorgress;
		public String MasterizzazionePorgress
		{
			get
			{
				return _masterizzazionePorgress;
			}
			set
			{
				if (value != _masterizzazionePorgress)
				{
					_masterizzazionePorgress = value;
					OnPropertyChanged("MasterizzazionePorgress");
				}
			}
		}

		public string ScontoApplicato
		{
			get
			{
				return venditoreSrv.scontoApplicato == null ? null :  Convert.ToString( venditoreSrv.scontoApplicato ) + "%";
			}
		}

		public decimal prezzoNettoTotale {
			get {
				return venditoreSrv.prezzoNettoTotale;
			}
		}

		public short? QuantitaRigaSelezionata
		{
			get
			{
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


		#endregion

		#region Controlli

		public bool abilitaOperazioniCarrello
		{
			get
			{
				if (IsInDesignMode)
					return true;

				bool posso = true;

				//Verifico che il carrello non sia stato venduto
				if(posso && carrelloCorrente.venduto)
					posso = false;

				// Verifico che i dati minimi siano stati indicati
				if( posso && RiCaFotoStampateCv.IsEmpty && venditoreSrv.sommatoriaFotoDaMasterizzare <= 0 )
					posso = false;

				// Ce un errore nella masterizzazione 
				if (posso && IsErroriMasterizzazione)
					posso = false;

				return posso;
			}
		}

		public bool abilitaEliminaDischetto
		{
			get
			{
				if (IsInDesignMode)
					return true;

				bool posso = true;

				//Verifico che il carrello non sia stato venduto
				if (posso && carrelloCorrente.venduto)
					posso = false;

				// Verifico che i dati minimi siano stati indicati
				if( posso && venditoreSrv.sommatoriaFotoDaMasterizzare == 0 )
					posso = false;

				// Ce un errore nella masterizzazione 
				if (posso && IsErroriMasterizzazione)
					posso = false;

				return posso;
			}
		}

		public bool possoAggiornareQuantitaRiga( short delta ) {

			if( ! abilitaEliminaRigaFoto(Carrello.TIPORIGA_STAMPA) )
				return false;

			if( rigaCarrelloStampataSelezionata.quantita + delta > 0 )
				return true;
			else
				return false;
		}

		public bool possoSpostareFotoRiga(string discriminator)
		{
			if (Carrello.TIPORIGA_MASTERIZZATA.Equals(discriminator))
				//Se voglio spostare le foto nelle masterizzate devo avere qualcosa nelle stampate
				return RiCaFotoStampateCv != null && RiCaFotoStampateCv.CurrentItem != null;

			if (Carrello.TIPORIGA_STAMPA.Equals(discriminator))
				//Se voglio spostare le foto nelle stampate devo avere qualcosa nelle masterizzate
				return RiCaFotoMasterizzateCv != null && RiCaFotoMasterizzateCv.CurrentItem != null;

			else
				return false;
		}

		public bool possoVisualizzareIncassiFotografi {
			get {
				return carrelloCorrente != null && carrelloCorrente.righeCarrello.Any();
			} 
		}

		public bool abilitaEliminaRigaFoto(String discriminator)
		{
			if (IsInDesignMode)
				return true;

			bool posso = true;

			//Verifico che il carrello non sia stato venduto
			if (posso && carrelloCorrente.venduto)
				posso = false;

			if(posso && discriminator == Carrello.TIPORIGA_STAMPA){
				// Verifico che i dati minimi siano stati indicati
				if (posso && RiCaFotoStampateCv.IsEmpty)
					posso = false;

				if (posso && rigaCarrelloStampataSelezionata == null)
					posso = false;
			}

			if(posso && discriminator == Carrello.TIPORIGA_MASTERIZZATA){
				if (posso && RiCaFotoMasterizzateCv.IsEmpty)
					posso = false;
				
				if (posso && rigaCarrelloMasterizzataSelezionata == null)
					posso = false;
			}

			// Ce un errore nella masterizzazione 
			if (posso && IsErroriMasterizzazione)
				posso = false;

			return posso;
		}

		public bool abilitaEliminaCarrello
		{
			get
			{
				if (IsInDesignMode)
					return true;

				bool posso = true;

				//Verifico che il carrello non sia stato venduto
				if (posso && carrelloCorrente.venduto)
					posso = false;

				// Verifico che i dati minimi siano stati indicati
				// Elimino solo se il carrello è stato caricato in caso contrario è transiente e quindi
				//posso fare svuota
				if (posso && RiCaFotoStampateCv.IsEmpty )
					posso = false;

				// Elimino solo i carrelli Salvati
				if(posso && ! venditoreSrv.isStatoModifica)
					posso = false;

				// Ce un errore nella masterizzazione 
				if (posso && IsErroriMasterizzazione)
					posso = false;

				return posso;
			}
		}

		public bool possoCaricareCarrelloSalvatoSelezionato
		{
			get
			{
				if (IsInDesignMode)
					return true;

				if( CarrelliSalvatiCv != null && CarrelliSalvatiCv.IsEmpty && !venditoreSrv.isStatoModifica )
					return false;

				if( CarrelloSalvatoSelezionato == null )
					return false;

				// Ce un errore nella masterizzazione 
				if (IsErroriMasterizzazione)
					return false;

				return true;
			}
		}

		public BitmapSource StatusStatoMasterizzazioneImage
		{
			get
			{
				// Decido qual'è la giusta icona da caricare per mostrare lo stato dello slide show (Running, Pause, Empty)

				// Non so perchè ma se metto il percorso senza il pack, non funziona. boh eppure sono nello stesso assembly.
				string uriTemplate = @"pack://application:,,,/Resources/##-16x16.png";

				Uri uri = null;

				if (StatoMasterizzazione == Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa)
				{
					uri = new Uri(uriTemplate.Replace("##", "ssCompleate"));
					IsErroriMasterizzazione = false;
				}
				if (StatoMasterizzazione == Digiphoto.Lumen.Servizi.Masterizzare.Fase.ErroreMedia)
				{
					uri = new Uri(uriTemplate.Replace("##", "ssErroreMedia"));
					IsErroriMasterizzazione = true;
					trayIconProvider.showError("Avviso", "Errore Media", 5000);
				}
				if (StatoMasterizzazione == Digiphoto.Lumen.Servizi.Masterizzare.Fase.InizioCopia)
				{
					uri = new Uri(uriTemplate.Replace("##", "ssBurn"));
					IsErroriMasterizzazione = false;
					trayIconProvider.showInfo("Avviso", "Inizio Copia", 5000);
				}
				if (StatoMasterizzazione == Digiphoto.Lumen.Servizi.Masterizzare.Fase.CopiaCompletata)
				{
					uri = new Uri(uriTemplate.Replace("##", "ssCompleate"));
					IsErroriMasterizzazione = false;
					trayIconProvider.showInfo("Avviso", "CopiaCompletata", 5000);
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
			if(!_bkgIdrata.IsBusy)
				_bkgIdrata.RunWorkerAsync();

			OnPropertyChanged( "carrelloCorrente" );
			OnPropertyChanged( "prezzoNettoTotale" );
			OnPropertyChanged( "ScontoApplicato" );
			OnPropertyChanged( "sommatoriaFotoDaMasterizzare" );
			OnPropertyChanged( "sommatoriaQtaFotoDaStampare" );
			OnPropertyChanged( "sommatoriaPrezziFotoDaStampare" );
			OnPropertyChanged( "possoVisualizzareIncassiFotografi" );
		}


		private bool verificaChiediConfermaSalvataggioTotaleAPagareZero()
		{
			bool procediPure = true;
			//Se ho un totale a pagare diverso da zero ritorno true e non chiedo conferma
			//altrimenti chiedo se voglio prosegiure
			if (venditoreSrv.carrello.totaleAPagare != 0)
				return procediPure;

			if (venditoreSrv.carrello.totaleAPagare == 0 && prezzoNettoTotale > 0)
			{
				procediPure = false;
				StringBuilder msg = new StringBuilder("Attenzione: stai per regalare il carrello.\nConfermi?");
				dialogProvider.ShowConfirmation(msg.ToString(), "Richiesta conferma",
					(confermato) =>
					{
						procediPure = confermato;
					});
			}

			return procediPure;
		}

        /// <summary>
        /// Devo mandare in stampa le foto selezionate
        /// Nel parametro mi arriva l'oggetto StampanteAbbinata che mi da tutte le indicazioni
        /// per la stampa: il formato carta e la stampante
        /// </summary>
        private void vendere()
        {
			_giornale.Debug( "Mi preparo a vendere questo carrello" );


			if( !venditoreSrv.isPossibileSalvareCarrello ) {
				string msg = venditoreSrv.msgValidaCarrello;
				if( msg == null )
					msg = "Il carrello non è validato.";
				dialogProvider.ShowError( msg, "Impossibile salvare il carrello", null );
				return;
			}

			//Verifico se ho un prezzo totaleAPagare(Forfettario) uguale a ZERO in caso notifico l'operatore che sta per regalare il carrello
			if (verificaChiediConfermaSalvataggioTotaleAPagareZero() == false)
				return;
		
			if (richiediDoveMasterizzare() == false)
				return;



			_giornale.Debug( "Sono pronto per vendere il carrello. Tot a pagare = " + venditoreSrv.carrello.totaleAPagare );




			if(venditoreSrv.vendereCarrello())
			{
				
				//Controllo se ci sono stati errori nella masterizzazione/copia su chiavetta
				if (!IsErroriMasterizzazione)
				{
					short totaleFotoMasterizzate = 0;
					short totoleFotoStampate = 0;
					short totoleErrori = 0;

					_giornale.Debug( "Non ci sono errori durante la masterizzazione" );

					foreach( RigaCarrello r in carrelloCorrente.righeCarrello ) {

						if (r.discriminator == Carrello.TIPORIGA_STAMPA) {
							totoleFotoStampate += (short)r.quantita;
						}

						if( r.discriminator == Carrello.TIPORIGA_MASTERIZZATA ) {
							totaleFotoMasterizzate += (short)r.quantita;  // Deve essere sempre 1.
						}
					}

					string msg = "Carrello venduto:" +
												"\nTotale: " + venditoreSrv.carrello.totaleAPagare +
												"\nN° Fotografie: " + totoleFotoStampate +
												"\nN° Foto Masterizzate: "+ totaleFotoMasterizzate +
												"\nN° ErroriUtil: " + totoleErrori;

					dialogProvider.ShowMessage( msg, "Avviso");
					_giornale.Info( msg );

					//Creo un nuovo carrello
					creaNuovoCarrello();
				}
				else
				{
					_giornale.Warn( "Riscontrati errori durante la masterizzazione" );
					GestoreCarrelloMsg msg = new GestoreCarrelloMsg(this);
					msg.fase = Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.ErroreMasterizzazione;
					LumenApplication.Instance.bus.Publish(msg);
				}
			}
			else
			{
				_giornale.Warn( "il carrello" + carrelloCorrente + " + non è stato salvato correttamente" );
				dialogProvider.ShowError("Attenzione: Il carrello non è stato salvato correttamente\nsegnalare l'anomalia", "ERRORE", null);
			}

        }

		private bool richiediDoveMasterizzare() {

			// Se non ho nessuna foto da masterizzare, esco subito.
			if( sommatoriaFotoDaMasterizzare <= 0 )
				return true;


			//Testo se devo masterizzare su CD o su Chiavetta
			if( Configurazione.UserConfigLumen.masterizzaDirettamente ) {

				bool procediPure = true;
				dialogProvider.ShowConfirmation( "Voi continuare la masterizzazione sul CD/DVD ?",
					"Richiesta conferma",
					  ( confermato ) => {
						  procediPure = confermato;
					  } );

				#region cartella
				if( !procediPure ) {

					string chiavettaPath = scegliCartellaDoveMasterizzare();
					if( chiavettaPath == null ) {
						return false;
						}

					venditoreSrv.setDatiDischetto( TipoDestinazione.CARTELLA, chiavettaPath );
					_giornale.Debug( "Masterizzo i files su : " + chiavettaPath );

				}
				#endregion cartella

			} else {

				string chiavettaPath = Configurazione.UserConfigLumen.defaultChiavetta;

				// cartella
				if( !testCartellaMasterizza( chiavettaPath ) ) {
					chiavettaPath = scegliCartellaDoveMasterizzare();
					if( chiavettaPath == null ) {
						return false;
					}
				}

				_giornale.Debug("Masterizzo i files su : " + chiavettaPath );
				venditoreSrv.setDatiDischetto( TipoDestinazione.CARTELLA, chiavettaPath );
			}

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
				chiavettaPath = PathUtil.scegliCartella();
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
		private void rimasterizza()
		{
			faseOld = Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa;
			venditoreSrv.rimasterizza();
        }

        private void calcolaTotali()
        {
			venditoreSrv.ricalcolaTotaleCarrello();
        }

		private void salvaCarrello()
		{
			//Verifico se ho un prezzo totaleAPagare(Forfettario) uguale a ZERO in caso notifico l'operatore che sta per regalare il carrello
			if (verificaChiediConfermaSalvataggioTotaleAPagareZero() == false)
				return;

			if( ! venditoreSrv.isPossibileSalvareCarrello ) {
				string msg = venditoreSrv.msgValidaCarrello;
				if( msg == null )
					msg = "Il carrello non è validato.";
				dialogProvider.ShowError( msg, "Impossibile salvare il carrello", null );
				return;
			}

			if (String.IsNullOrEmpty(carrelloCorrente.intestazione))
			{
				dialogProvider.ShowError("Non è possibile salvare il carrello senza Intestazione", "Errore", null );
				return;
			}

			if( venditoreSrv.salvaCarrello() ) {
				dialogProvider.ShowMessage( "Carrello Salvato Correttamente", "Avviso" );
				creaNuovoCarrello();
			} else
				dialogProvider.ShowError( "Salvataggio carrello fallito\nsegnalare l'anomalia allegando il log", "ERRORE", null );
		}

		private void eliminaCarrello()
		{
			bool procediPure = false;

			dialogProvider.ShowConfirmation( "Confermi la cancellazione del carrello:\n " + CarrelloSalvatoSelezionato.intestazione,
				                             "Cancellazione carrello",	(confermato) =>	{
												 procediPure = confermato;
											 });
			if( !procediPure )
				return;

			venditoreSrv.removeCarrello( CarrelloSalvatoSelezionato );
			creaNuovoCarrello();
		}

		private void eliminaRiga(String discriminator)
		{
			if(discriminator == Carrello.TIPORIGA_STAMPA)
				venditoreSrv.removeRigaCarrello(rigaCarrelloStampataSelezionata);
			else if (discriminator == Carrello.TIPORIGA_MASTERIZZATA)
				venditoreSrv.removeRigaCarrello(rigaCarrelloMasterizzataSelezionata);
		}

		private void eliminaDischetto()
		{
			if( venditoreSrv.sommatoriaFotoDaMasterizzare <= 0 )
				return;
			if (chiediConfermaEliminazioneDischetto() == false)
				return;
			venditoreSrv.removeRigheCarrello( Carrello.TIPORIGA_MASTERIZZATA );
		}

		private bool chiediConfermaEliminazioneDischetto()
		{

			StringBuilder msg = new StringBuilder("Questa operazione rimuove il dischetto dal carrello:\n L'operazione è irreversibile.\nConfermi");
			bool procediPure = false;
			dialogProvider.ShowConfirmation(msg.ToString(), "Richiesta conferma",
				(confermato) =>
				{
					procediPure = confermato;
				});

			return procediPure;
		}


		private void caricareCarrelloSalvatoSelezionato()
		{
			// Eventualmente stoppo lavoro precedente.
			if( _bkgIdrata.WorkerSupportsCancellation && _bkgIdrata.IsBusy )
				_bkgIdrata.CancelAsync();

			venditoreSrv.caricaCarrello( CarrelloSalvatoSelezionato );

			if (!_bkgIdrata.IsBusy)
				_bkgIdrata.RunWorkerAsync();
			// Non mettere altro codice qui sotto. Questa deve essere l'ultima operazione di questo metodo
		}

		private void bkgIdrata_DoWork(object sender, DoWorkEventArgs e)
		{
			System.Threading.Thread.Sleep( 50 );  // Perdo un attimo di tempo per permettere al thread principale di rientrare e quindi di chiudere la sua UnitOfWork.
			BackgroundWorker worker = sender as BackgroundWorker;


//			using( new UnitOfWorkScope() ) {

				Carrello c = carrelloCorrente;
//				OrmUtil.forseAttacca<Carrello>( "Carrelli", ref c );

				foreach( RigaCarrello ric in carrelloCorrente.righeCarrello ) {
					if( 1==1 ) {
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

		private void creaNuovoCarrello()
		{
			venditoreSrv.creaNuovoCarrello();
			MasterizzazionePorgress = "";
			StatoMasterizzazione = Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa;
			OnPropertyChanged("StatusStatoMasterizzazioneImage");
			incassiFotografiViewModel.clear();
		}

		private void svuotaCarrello()
		{
			// ELimino tutte le righe ma tengo buona la testata
			venditoreSrv.removeRigheCarrello( Carrello.TIPORIGA_STAMPA );
			venditoreSrv.removeRigheCarrello( Carrello.TIPORIGA_MASTERIZZATA );
			calcolaTotali();

			MasterizzazionePorgress = "";
			StatoMasterizzazione = Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa;
			OnPropertyChanged("StatusStatoMasterizzazioneImage");
			incassiFotografiViewModel.incassiFotografi.Clear();
		}


			 
		private void azzeraParamRicerca()
		{
			paramCercaCarrello = new ParamCercaCarrello();
		}

		/// <summary>
		/// Chiamo il servizio che esegue la query sul database
		/// </summary>
		private void eseguireRicerca()
		{

			completaParametriRicerca();

			ricercaPoiCreaViewCarrelliSalvati();
			
			// Se non ho trovato nulla, allora avviso l'utente
			if (CarrelliSalvatiCv.IsEmpty)
				dialogProvider.ShowMessage("Nessun Carrello trovato con questi filtri di ricerca", "AVVISO");
		}

		private void completaParametriRicerca()
		{

			paramCercaCarrello.idratareImmagini = false;

			// Aggiungo eventuale parametro l'intestazione
			if (!String.IsNullOrEmpty(IntestazioneSearch))
			{
				paramCercaCarrello.intestazione = IntestazioneSearch;
			}
			else
			{
				paramCercaCarrello.intestazione = null;
			}

			// Aggiungo eventuale parametro l'intestazione
			if (!String.IsNullOrEmpty(IntestazioneSearch))
			{
				paramCercaCarrello.intestazione = IntestazioneSearch;
			}
			else
			{
				paramCercaCarrello.intestazione = null;
			}
		}

		private void aggiornareQuantitaRiga( short delta )
		{
			if( rigaCarrelloStampataSelezionata.quantita + delta < 1 )
				rigaCarrelloStampataSelezionata.quantita = 1;
			else
				rigaCarrelloStampataSelezionata.quantita += delta;

			calcolaTotali();
		}

		private void spostaFotoRiga(string discriminator)
		{
			if (Carrello.TIPORIGA_MASTERIZZATA.Equals(discriminator))
				venditoreSrv.spostaRigaCarrello(rigaCarrelloStampataSelezionata);

			if (Carrello.TIPORIGA_STAMPA.Equals(discriminator))
			{
				SelezionaStampanteDialog d = new SelezionaStampanteDialog();
				bool? esito = d.ShowDialog();

				if (esito == true)
				{
					//associo il nuovo formato carta alla riga
					rigaCarrelloMasterizzataSelezionata.formatoCarta = d.formatoCarta;
					venditoreSrv.spostaRigaCarrello(rigaCarrelloMasterizzataSelezionata);
					rinfrescaViewRighe();
				}

				d.Close();
			}
		}

		private void visualizzareIncassiFotografi() {

			// Aggiorno la collezione: non la ricreo perché è già bindata
			venditoreSrv.ricalcolaProvvigioni();
			incassiFotografiViewModel.replace( carrelloCorrente.incassiFotografi );
		}


        #endregion

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
					_salvaCarrelloCommand = new RelayCommand( param => salvaCarrello(), 
					                                          param => abilitaOperazioniCarrello, 
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
					_rimasterizzaCommand = new RelayCommand(param => rimasterizza(), param => IsErroriMasterizzazione, false);
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

		private RelayCommand _spostaFotoRigaCommand;
		public ICommand SpostaFotoRigaCommand
		{
			get
			{
				if (_spostaFotoRigaCommand == null)
				{
					_spostaFotoRigaCommand = new RelayCommand(param => spostaFotoRiga((String)param),
															   param => possoSpostareFotoRiga((String)param), false);
				}
				return _spostaFotoRigaCommand;
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

        #endregion

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
			
			System.Diagnostics.Trace.WriteLine("");
			System.Diagnostics.Trace.WriteLine("[TotFotoNonAggiunte]: " + msg.totFotoNonAggiunte);
			System.Diagnostics.Trace.WriteLine("[TotFotoAggiunte]: " + msg.totFotoAggiunte);
			System.Diagnostics.Trace.WriteLine("[Esito]: " + msg.esito);
			System.Diagnostics.Trace.WriteLine("[FotoAggiunta]: " + msg.fotoAggiunta);
			System.Diagnostics.Trace.WriteLine("[Fase]: " + msg.fase);
			System.Diagnostics.Trace.WriteLine("[Result]: " + msg.result);
			System.Diagnostics.Trace.WriteLine("[Progress]: " + msg.progress);
			 

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

			updateGUI();

			// Qui cambiano soltanto gli attributi con il totale del carrello
			if(msg.fase== Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.UpdateCarrello){
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
		}

		public void OnNext(StampatoMsg value)
		{
			if (value.lavoroDiStampa.esitostampa == EsitoStampa.Errore)
			{
				dialogProvider.ShowError("Stampa non Eseguita Correttamente", "Errore", null);
			}
		}

		// E' stata modificata una o più foto
		public void OnNext( FotoModificateMsg msg ) {
			
			Digiphoto.Lumen.Servizi.EntityRepository.IEntityRepositorySrv<Fotografia> er = LumenApplication.Instance.getServizioAvviato<Digiphoto.Lumen.Servizi.EntityRepository.IEntityRepositorySrv<Fotografia>>();

			// Devo controllare se nel carrello sono presenti delle foto che hanno subito delle modifiche.
			// In tal caso devo aggiornarmi
			foreach( Fotografia fMod in msg.fotos ) {

				// Faccio il controllo con l'id numerico perché l'equals non mi da sicurezze
				RigaCarrello riga = carrelloCorrente.righeCarrello.SingleOrDefault( r => r.fotografia.id == fMod.id );
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

		#endregion

	}
}
