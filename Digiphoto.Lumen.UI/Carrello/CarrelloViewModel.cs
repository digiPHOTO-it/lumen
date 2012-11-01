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

namespace Digiphoto.Lumen.UI
{
	public class CarrelloViewModel : ViewModelBase, IObserver<MasterizzaMsg>, IObserver<GestoreCarrelloMsg>, IObserver<StampatoMsg>
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

				// Creo due view diverse per le righe del carrello
				rinfrescaViewRighe();
				
				if (carrelloCorrente.giornata == null || carrelloCorrente.giornata.Equals(DateTime.MinValue))
					carrelloCorrente.giornata = LumenApplication.Instance.stato.giornataLavorativa;

				
				decimal prezzoTotaleIntero = 0;
				foreach(RiCaFotoStampata riCaFotoStampata in venditoreSrv.carrello.righeCarrello){
					prezzoTotaleIntero += riCaFotoStampata.prezzoNettoTotale;
				}

				// Non voglio ricercare nulla fino a che l'utente non me lo chiede
//				ricercaPoiCreaViewCarrelliSalvati();

				PrezzoTotaleIntero = prezzoTotaleIntero;
				ScontoApplicato = "0%";

				IsErroriMasterizzazione = false;

				// Creo il worker che mi serve per idratare le immagini delle righe del carrello
				_bkgIdrata = new BackgroundWorker();
				_bkgIdrata.WorkerReportsProgress = false;  // per ora non mi complico la vita
				_bkgIdrata.WorkerSupportsCancellation = true; // per ora non mi complico la vita
				_bkgIdrata.DoWork += new DoWorkEventHandler( bkgIdrata_DoWork );
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
				RiCaFotoStampateCv = CollectionViewSource.GetDefaultView(carrelloCorrente.righeCarrello.OfType<RiCaFotoStampata>());
			}
			OnPropertyChanged( "RiCaFotoStampateCv" );
		}

        #region Proprietà

		public Digiphoto.Lumen.Model.Carrello carrelloCorrente {
			get {
				return venditoreSrv.carrello;
			}
		}

		/// <summary>
		///  Uso questa particolare collectionView perché voglio tenere traccia nel ViewModel degli N-elementi selezionati.
		///  Sottolineo N perché non c'è supporto nativo per questo. Vedere README.txt nel package di questa classe.
		/// </summary>
		public ICollectionView RiCaFotoStampateCv
		{
			get;
			private set;
		}


		/// <summary>
		/// Siccome di riga disco masterizzato ne posso avere una sola, mi creo una property apposita.
		/// </summary>
		public RiCaDiscoMasterizzato ricaDiscoMasterizzato {
			get {
				if( carrelloCorrente == null )
					return null;
				else {
					return carrelloCorrente.righeCarrello.OfType<RiCaDiscoMasterizzato>().SingleOrDefault();
				}
			}
		}

		public ICollectionView CarrelliSalvatiCv
		{
			get;
			private set;
		}

		private RigaCarrello _righeCarrelloCvSelected;
		public RigaCarrello RigheCarrelloCvSelected
		{
			get
			{
				return _righeCarrelloCvSelected;
			}
		    set{
				if (_righeCarrelloCvSelected != value)
				{
					_righeCarrelloCvSelected = value;
					if (RigheCarrelloCvSelected!=null)
					{
						QuantitaRigaSelezionata = RigheCarrelloCvSelected.quantita;
					}
				}
			}
		}

		private int _righeCarrelloCvSelectedIndex;
		public int RigheCarrelloCvSelectedIndex
		{
			get
			{
				return _righeCarrelloCvSelectedIndex;
			}
		    set{
				if (_righeCarrelloCvSelectedIndex != value)
				{
					_righeCarrelloCvSelectedIndex = value;
					OnPropertyChanged("RigheCarrelloCvSelectedIndex");
					OnPropertyChanged( "abilitaEliminaRigaFoto" );
					OnPropertyChanged( "possoAggiornareQuantitaRiga" );
				}
			}
		}

		private int _carrelliSalvatiCvSelectedIndex;
		public int CarrelliSalvatiCvSelectedIndex
		{
			get
			{
				return _carrelliSalvatiCvSelectedIndex;
			}
		    set{
				if (_carrelliSalvatiCvSelectedIndex != value)
				{
					_carrelliSalvatiCvSelectedIndex = value;
					OnPropertyChanged("CarrelliSalvatiCvSelectedIndex");
				}
			}
		}

		public Digiphoto.Lumen.Model.Carrello CarrelloSelezionato
		{
			get;
			set;
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

		#endregion

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

		private decimal _prezzoTotaleIntero;
		public decimal PrezzoTotaleIntero
		{
			get
			{
				return _prezzoTotaleIntero;
			}
			set
			{
				if (value != _prezzoTotaleIntero)
				{
					_prezzoTotaleIntero = value;
					OnPropertyChanged("PrezzoTotaleIntero");
				}
			}
		}

		private decimal _prezzoTotaleForfettario;
		public decimal PrezzoTotaleForfettario
		{
			get
			{
				return _prezzoTotaleForfettario;
			}
			set
			{
				if (value != _prezzoTotaleForfettario)
				{
					_prezzoTotaleForfettario = value;

					if (PrezzoTotaleIntero != 0 && _prezzoTotaleForfettario < PrezzoTotaleIntero)
					{
						ScontoApplicato = ((PrezzoTotaleIntero - _prezzoTotaleForfettario) / PrezzoTotaleIntero).ToString("P1");
					}
					OnPropertyChanged("PrezzoTotaleForfettario");
				}
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

		private string _scontoApplicato;
		public string ScontoApplicato
		{
			get
			{
				return _scontoApplicato;
			}
			set
			{
				if (value != _scontoApplicato)
				{
					_scontoApplicato = value;
					OnPropertyChanged("ScontoApplicato");
				}
			}
		}



		private short _quantitaRigaSelezionata;
		public short QuantitaRigaSelezionata
		{
			get
			{
				return _quantitaRigaSelezionata;
			}
			set
			{
				if (value != _quantitaRigaSelezionata & value > 0)
				{
					_quantitaRigaSelezionata = value;
					OnPropertyChanged("QuantitaRigaSelezionata");
				}
			}
		}

		/// <summary>
		/// Conta quante foto sono state messe nel cd da masterizzare
		/// </summary>
		public int contaFotoMasterizzate {
			get {
				if( venditoreSrv != null && venditoreSrv.masterizzaSrv != null && venditoreSrv.masterizzaSrv.fotografie != null )
					return venditoreSrv.masterizzaSrv.fotografie.Count;
				else
					return 0;
			}
		}

		/// <summary>
		/// Somma tutte le quantità di tutte le righe di tipo "FotoStampata"
		/// </summary>
		public int sommatoriaQtaStampe {
			get {
				return carrelloCorrente.righeCarrello.OfType<RiCaFotoStampata>().Sum( r => r.quantita );
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
				if (posso && RiCaFotoStampateCv.IsEmpty && contaFotoMasterizzate <= 0 )
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
				if (posso && contaFotoMasterizzate == 0)
					posso = false;

				// Ce un errore nella masterizzazione 
				if (posso && IsErroriMasterizzazione)
					posso = false;

				return posso;
			}
		}

		public bool possoAggiornareQuantitaRiga {
			get {
				// TODO da sistemare
				return abilitaEliminaRigaFoto;
			}
		}

		public bool abilitaEliminaRigaFoto
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
				if (posso && RiCaFotoStampateCv.IsEmpty)
					posso = false;

				if( posso && RiCaFotoStampateCv.CurrentItem == null )
					posso = false;

				// Ce un errore nella masterizzazione 
				if (posso && IsErroriMasterizzazione)
					posso = false;

				return posso;
			}
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

		public bool abilitaCaricaCarrelloSelezionato
		{
			get
			{
				if (IsInDesignMode)
					return true;

				bool posso = true;

				// Verifico che i dati minimi siano stati indicati
				// Elimino solo se il carrello è stato caricato in caso contrario è transiente e quindi
				//posso fare svuota
				if( posso && CarrelliSalvatiCv != null && CarrelliSalvatiCv.IsEmpty && !venditoreSrv.isStatoModifica )
					posso = false;

				// Ce un errore nella masterizzazione 
				if (posso && IsErroriMasterizzazione)
					posso = false;

				return posso;
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

		// TODO questa è da togliere
		public void updateGUI()
		{
			OnPropertyChanged( "carrelloCorrente" );
			rinfrescaViewRighe();

// Questo mi sembra un pò esagerato. Riparte la query sul db!!! non possiamo metterla ad ogni aggiornamento delle property
//			ricercaPoiCreaViewCarrelliSalvati();

			OnPropertyChanged("PrezzoTotaleForfettario");
			OnPropertyChanged( "ricaDiscoMasterizzato" );
			OnPropertyChanged( "contaFotoMasterizzate" );
			OnPropertyChanged( "sommatoriaQtaStampe" );
			OnPropertyChanged( "abilitaEliminaRigaFoto" );
			OnPropertyChanged( "possoAggiornareQuantitaRiga" );
		}


        /// <summary>
        /// Devo mandare in stampa le foto selezionate
        /// Nel parametro mi arriva l'oggetto StampanteAbbinata che mi da tutte le indicazioni
        /// per la stampa: il formato carta e la stampante
        /// </summary>
        private void vendere()
        {
			venditoreSrv.carrello.totaleAPagare = PrezzoTotaleForfettario;
			_giornale.Debug( "Mi preparo a vendere questo carrello da " + PrezzoTotaleForfettario + " euro" );

			if (venditoreSrv.masterizzaSrv!=null)
			{
				venditoreSrv.masterizzaSrv.prezzoForfaittario = PrezzoTotaleForfettario-PrezzoTotaleIntero;
			}

			if(PrezzoTotaleForfettario==0){
				dialogProvider.ShowMessage("Si sta cercando di effetuare una vendita a prezzo uguale a 0", "Errore");
				return;
			}


			if (venditoreSrv.masterizzaSrv != null && richiediDoveMasterizzare() == false)
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

						if (r is RiCaFotoStampata)
						{
							RiCaFotoStampata rfs = (RiCaFotoStampata)r;
							totoleFotoStampate += (short)rfs.quantita;
						}

						if( r is RiCaDiscoMasterizzato ) {
							RiCaDiscoMasterizzato rdm = (RiCaDiscoMasterizzato)r;
							totaleFotoMasterizzate = (short)rdm.totFotoMasterizzate;
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
					venditoreSrv.creaNuovoCarrello();
					PrezzoTotaleForfettario = 0;
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

			updateGUI();
        }

		private bool richiediDoveMasterizzare() {

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
					string chiavettaPath = Configurazione.UserConfigLumen.defaultChiavetta;
					if( !Configurazione.isFuoriStandardCiccio ) {
						System.Windows.Forms.FolderBrowserDialog fBD = new System.Windows.Forms.FolderBrowserDialog();
						//fBD.RootFolder = Environment.SpecialFolder.Desktop;
						fBD.SelectedPath = Configurazione.UserConfigLumen.defaultChiavetta;
						DialogResult result = fBD.ShowDialog();

						if( result == DialogResult.OK ) {
							chiavettaPath = fBD.SelectedPath;
						}
					} else {
						string path = PathUtil.scegliCartella();
						if( path != null ) {
							chiavettaPath = path;
						}
					}

					if( ! testCartellaMasterizza( chiavettaPath ) ) {
						dialogProvider.ShowError( "path = " + chiavettaPath, "Cartella non scrivibile", null );
						return false;
					}

					venditoreSrv.masterizzaSrv.impostaDestinazione( TipoDestinazione.CARTELLA, chiavettaPath );
					_giornale.Debug( "Masterizzo i files su : " + chiavettaPath );

				}
				#endregion cartella

			} else {
				// cartella
				if( !testCartellaMasterizza( Configurazione.UserConfigLumen.defaultChiavetta ) ) {
					dialogProvider.ShowError( "path = " + Configurazione.UserConfigLumen.defaultChiavetta, "Cartella non scrivibile", null );
					return false;
				}

				_giornale.Debug("Masterizzo i files su : " + Configurazione.UserConfigLumen.defaultChiavetta);
				venditoreSrv.masterizzaSrv.impostaDestinazione(TipoDestinazione.CARTELLA, Configurazione.UserConfigLumen.defaultChiavetta);
			}

			return true;
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
			decimal prezzoTotaleIntero = 0;
			foreach (RigaCarrello rigaCarrello in carrelloCorrente.righeCarrello)
			{
				//Verifico che la riga sia di tipo RiCaFotoStampata
				RiCaFotoStampata riCaFotoStampata = rigaCarrello as RiCaFotoStampata;
				if (rigaCarrello != null && riCaFotoStampata!=null)
				{
					riCaFotoStampata.prezzoNettoTotale = riCaFotoStampata.quantita * riCaFotoStampata.prezzoLordoUnitario;
					prezzoTotaleIntero += riCaFotoStampata.prezzoNettoTotale;
				}
				
			}
			PrezzoTotaleIntero = prezzoTotaleIntero;
			PrezzoTotaleForfettario = prezzoTotaleIntero;
        }

		private void salvaCarrello()
		{
			if( contaFotoMasterizzate > 0 ) {
				dialogProvider.ShowMessage("Il carrello contiene un Dischetto!!!\nSi ricorda che i dischetti non potranno più essere recuperati", "Avviso");
			}

			if (String.IsNullOrEmpty(carrelloCorrente.intestazione))
			{
				dialogProvider.ShowMessage("Non è possibile salvare il carrello senza Intestazione", "Avviso");
				return;
			}

			venditoreSrv.carrello.totaleAPagare = PrezzoTotaleForfettario;

			if( venditoreSrv.salvaCarrello() ) {
				dialogProvider.ShowMessage( "Carrello Salvato Correttamente", "Avviso" );
				venditoreSrv.creaNuovoCarrello();
			} else
				dialogProvider.ShowError( "Attenzione: Il carrello non è stato salvato correttamente\nsegnalare l'anomalia", "ERRORE", null );

			updateGUI();
		}

		private void eliminaCarrello()
		{
			Digiphoto.Lumen.Model.Carrello carrello = CarrelliSalvatiCv.CurrentItem as Digiphoto.Lumen.Model.Carrello;			 
			venditoreSrv.removeCarrello(carrello);
			venditoreSrv.creaNuovoCarrello();
			updateGUI();
		}

		private void eliminaRiga()
		{
			RiCaFotoStampata riCaFotoStampata = RiCaFotoStampateCv.CurrentItem as RiCaFotoStampata;
			//Testo il cast se è riuscito allora la riga è di tipo RiCaFotoStampata altrimenti e di tipo RiCaDiscoMasterizzato
			if (riCaFotoStampata != null)
			{
				venditoreSrv.removeRigaCarrello(RiCaFotoStampateCv.CurrentItem as RiCaFotoStampata);
			}
			else
			{
				venditoreSrv.removeRigaCarrello(RiCaFotoStampateCv.CurrentItem as RiCaDiscoMasterizzato);
			}
			updateGUI();
		}

		private void eliminaDischetto()
		{
			if( contaFotoMasterizzate <= 0 )
				return;
			if (chiediConfermaEliminazioneDischetto() == false)
				return;
			venditoreSrv.removeRigaCarrello( ricaDiscoMasterizzato );
			updateGUI();
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


		private void caricaCarrelloSelezionato()
		{
			GestoreCarrelloMsg msg = new GestoreCarrelloMsg(this);
			msg.fase = GestoreCarrelloMsg.Fase.LoadCarrelloSalvato;
			LumenApplication.Instance.bus.Publish(msg);

			venditoreSrv.caricaCarrello( (Carrello)CarrelliSalvatiCv.CurrentItem );

			int index = CarrelliSalvatiCvSelectedIndex;
			updateGUI();
			CarrelliSalvatiCvSelectedIndex = index;


			// Eventualmente stoppo lavoro precedente.
			if( _bkgIdrata.WorkerSupportsCancellation && _bkgIdrata.IsBusy )
				_bkgIdrata.CancelAsync();

			_bkgIdrata.RunWorkerAsync();
			// Non mettere altro codice qui sotto. Questa deve essere l'ultima operazione di questo metodo
		}

		private void bkgIdrata_DoWork(object sender, DoWorkEventArgs e)
		{
			System.Threading.Thread.Sleep( 50 );  // Perdo un attimo di tempo per permettere al thread principale di rientrare e quindi di chiudere la sua UnitOfWork.
			BackgroundWorker worker = sender as BackgroundWorker;


			using( new UnitOfWorkScope() ) {

				Carrello c = carrelloCorrente;
				OrmUtil.forseAttacca<Carrello>( "Carrelli", ref c );

				foreach( RigaCarrello ric in carrelloCorrente.righeCarrello ) {
					if( ric is RiCaFotoStampata ) {
						if( (worker.CancellationPending == true) ) {
							e.Cancel = true;
							break;
						} else {
							Fotografia foto = (ric as RiCaFotoStampata).fotografia;
							if( foto != null )
								AiutanteFoto.idrataImmaginiFoto( foto, IdrataTarget.Provino );
						}
					}
				}
			}

			// TODO
			// Non capisco perché Ma non dovrebbe servire ricreare la view. dovrebbe bastare questo:
			// OnPropertyChanged( "RiCaFotoStampateCv" );
			// l'effetto che non funziona, è che non vedo caricare i provini delle immaginette del carrello.
			// Sono costretto a ricreare tutta la collection-view. Mistero da sistemare.
			rinfrescaViewRighe();
			
		}

		private void creaNuovoCarrello()
		{
			venditoreSrv.creaNuovoCarrello();
			MasterizzazionePorgress = "";
			StatoMasterizzazione = Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa;
			OnPropertyChanged("StatusStatoMasterizzazioneImage");
			updateGUI();
		}

		private void svuotaCarrello()
		{
			venditoreSrv.abbandonaCarrello();
			MasterizzazionePorgress = "";
			StatoMasterizzazione = Digiphoto.Lumen.Servizi.Masterizzare.Fase.Attesa;
			OnPropertyChanged("StatusStatoMasterizzazioneImage");
			updateGUI();
		}

		private void updateDatiCarrelloSelected()
		{
			Digiphoto.Lumen.Model.Carrello carrelloSelezionato = CarrelliSalvatiCv.CurrentItem as Digiphoto.Lumen.Model.Carrello;
			if (carrelloSelezionato != null)
			{
				CarrelloSelezionato = carrelloSelezionato;
				OnPropertyChanged("CarrelloSelezionato");
			}
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

		private void selectedRiga()
		{
			calcolaTotali();
		}

		private void updateQuantitaCommand(object param)
		{
			String type = (string)param;
			RiCaFotoStampata rica = RiCaFotoStampateCv.CurrentItem as RiCaFotoStampata;

			int index = RigheCarrelloCvSelectedIndex;
			
			switch(type)
			{
				case "+":			
					rica.quantita = ++QuantitaRigaSelezionata;
				break;
				case "-":
					// Se voglio una quantita uguale a 0 elimino la riga
					short quantita = --QuantitaRigaSelezionata;
					if (quantita > 0)
					{
						rica.quantita = quantita;
					}
					else
					{
						rica.quantita = 1;
					}
				break;
			}

			// Ridisegno la listView con le righe aggiornate
				rinfrescaViewRighe();

			RigheCarrelloCvSelectedIndex = index;
			
			calcolaTotali();
		}

        #endregion

        #region Comandi

		private RelayCommand _updateGUICommand;
		public ICommand updateGUICommand
		{
			get
			{
				if (_updateGUICommand == null)
				{
					_updateGUICommand = new RelayCommand(param => updateGUI());
				}
				return _updateGUICommand;
			}
		}

		private RelayCommand _updateDatiCarrelloSelected;
		public ICommand UpdateDatiCarrelloSelected
		{
			get
			{
				if (_updateDatiCarrelloSelected == null)
				{
					_updateDatiCarrelloSelected = new RelayCommand(param => updateDatiCarrelloSelected());
				}
				return _updateDatiCarrelloSelected;
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
					_eliminaRigaCommand = new RelayCommand(param => eliminaRiga(), param => abilitaEliminaRigaFoto, true);
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

        private RelayCommand _calcolaTotali;
        public ICommand CalcolaTotali
        {
            get
            {
                if (_calcolaTotali == null)
                {
                    _calcolaTotali = new RelayCommand(param => calcolaTotali());
                }
                return _calcolaTotali;
            }
        }

		private RelayCommand _selectedRiga;
		public ICommand SelectedRiga
        {
            get
            {
				if (_selectedRiga == null)
                {
					_selectedRiga = new RelayCommand(param => selectedRiga());
                }
				return _selectedRiga;
            }
        }

		private RelayCommand _updateQuantitaCommand;
		public ICommand UpdateQuantitaCommand
        {
            get
            {
				if (_updateQuantitaCommand == null)
                {
					_updateQuantitaCommand = new RelayCommand(param => updateQuantitaCommand(param), p => possoAggiornareQuantitaRiga );
                }
				return _updateQuantitaCommand;
            }
        }

		private RelayCommand _caricaCarrelloSelezionatoCommand;
		public ICommand caricaCarrelloSelezionatoCommand
		{
			get
			{
				if (_caricaCarrelloSelezionatoCommand == null)
				{
					_caricaCarrelloSelezionatoCommand = new RelayCommand(param => caricaCarrelloSelezionato(), param => abilitaCaricaCarrelloSelezionato, false);
				}
				return _caricaCarrelloSelezionatoCommand;
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
					_eseguireRicercaCommand = new RelayCommand(param => eseguireRicerca());
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
			if(msg.fase== Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.UpdateCarrello){
				updateGUI();
			}
		}

		public void OnNext(StampatoMsg value)
		{
			if (value.lavoroDiStampa.esitostampa == EsitoStampa.Errore)
			{
				dialogProvider.ShowError("Stampa non Eseguita Correttamente", "Errore", null);
			}
		}

		#endregion

	}
}
