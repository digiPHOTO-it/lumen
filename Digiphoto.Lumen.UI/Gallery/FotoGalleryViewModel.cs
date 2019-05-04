using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Model;
using System.ComponentModel;
using System.Windows.Input;
using Digiphoto.Lumen.Servizi.Vendere;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Config;
using System.Windows;
using Digiphoto.Lumen.UI.Pubblico;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.UI.Dialogs;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Digiphoto.Lumen.Servizi.Ritoccare.Clona;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.UI.Converters;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Core.Collections;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Servizi.EliminaFotoVecchie;
using Digiphoto.Lumen.Core.Eventi;
using Digiphoto.Lumen.UI.SelettoreAzioniRapide;
using Digiphoto.Lumen.Core.Database;

namespace Digiphoto.Lumen.UI.Gallery {


	// La gallery può funzionare in queste modalità
	public enum ModalitaFiltroSelez {
		Tutte,
		SoloSelezionate
	}

	public class FotoGalleryViewModel : ViewModelBase, IContenitoreGriglia, ISelettore<Fotografia>,
	                                    IObserver<FotoModificateMsg>, IObserver<NuovaFotoMsg>, IObserver<ClonaFotoMsg>, IObserver<FotoEliminateMsg>,
										IObserver<GestoreCarrelloMsg>, IObserver<RefreshMsg>, IObserver<CambioStatoMsg>, IObserver<StampatoMsg>

	{
		[Flags]
		private enum RicercaFlags {

			Niente					= 0,
			NuovaRicerca			= 1,		// parte una nuova ricerca con posizionamento all'inizio dei risultati
			ConfermaSeFiltriVuoti	= 2,
			MantenereSelezionate	= 4,		// non vengono azzerate le foto selezionate
			MantenereListaIds       = 8			// non viene azzerata la lista degli ids (quando proviene dal carrello)
		}

		/// <summary>
		/// Numero di foto prima e dopo quando viene richiesta la ricerca con intorno
		/// </summary>
		private readonly int QUANTE_FOTO_INTORNO = 300;


		#region Campi

		private BackgroundWorker _bkgIdrata;

		#endregion


		#region Costruttori

		public FotoGalleryViewModel() {

			// Devo ascoltare tutti i messaggi applicativi che girano
			IObservable<FotoModificateMsg> observableFM = LumenApplication.Instance.bus.Observe<FotoModificateMsg>();
			observableFM.Subscribe( this );

			IObservable<ClonaFotoMsg> observableClonaFoto = LumenApplication.Instance.bus.Observe<ClonaFotoMsg>();
			observableClonaFoto.Subscribe( this );

			IObservable<NuovaFotoMsg> observableNuovaFoto = LumenApplication.Instance.bus.Observe<NuovaFotoMsg>();
			observableNuovaFoto.Subscribe( this );

			IObservable<FotoEliminateMsg> observableFotoEliminate = LumenApplication.Instance.bus.Observe<FotoEliminateMsg>();
			observableFotoEliminate.Subscribe( this );

			IObservable<GestoreCarrelloMsg> observableGesCarrello = LumenApplication.Instance.bus.Observe<GestoreCarrelloMsg>();
			observableGesCarrello.Subscribe( this );

			IObservable<RefreshMsg> observableRefresh = LumenApplication.Instance.bus.Observe<RefreshMsg>();
			observableRefresh.Subscribe( this );

			IObservable<CambioStatoMsg> observableCambioStato = LumenApplication.Instance.bus.Observe<CambioStatoMsg>();
			observableCambioStato.Subscribe( this );

			IObservable<StampatoMsg> observableStampato = LumenApplication.Instance.bus.Observe<StampatoMsg>();
			observableStampato.Subscribe( this );

			// Istanzio i ViewModel dei componenti di cui io sono composto
			selettoreEventoViewModel = new SelettoreEventoViewModel();
			selettoreScaricoCardViewModel = new SelettoreScaricoCardViewModel();
			selettoreScaricoCardViewModel.scarichiCardsCW.SelectionChanged += scarichiCardsCW_SelectionChanged;

			selettoreEventoMetadato = new SelettoreEventoViewModel();
			selettoreFotografoViewModel = new SelettoreFotografoViewModel();

			selettoreAzioniRapideViewModel = new SelettoreAzioneRapidaViewModel( this );
			selettoreAzioniRapideViewModel.gestitaSelezioneMultipla = true;

			selettoreMetadatiViewModel = new SelettoreMetadatiViewModel1( this );

			selettoreAzioniAutomaticheViewModel = new SelettoreAzioniAutomaticheViewModel( this );

			azzeraParamRicerca();       // Svuoto i parametri
			azzeraFotoSelez();          // creo la collezione vuota delle foto selezionate

			// Per default lavoro con tutti i risultati
			modalitaFiltroSelez = ModalitaFiltroSelez.Tutte;


			if( IsInDesignMode ) {

			} else {

				//
				caricaStampantiAbbinate();

				_bkgIdrata = new BackgroundWorker();
				_bkgIdrata.DoWork += new DoWorkEventHandler( bkgIdrata_DoWork );
				_bkgIdrata.RunWorkerCompleted += new RunWorkerCompletedEventHandler( bkgIdrata_RunWorkerCompleted );
				_bkgIdrata.ProgressChanged += new ProgressChangedEventHandler( bkgIdrata_ProgressChanged );
				_bkgIdrata.WorkerReportsProgress = true;
				_bkgIdrata.WorkerSupportsCancellation = true;
			}

			// Imposto per default la visualizzazione a 2 stelline
			cambiarePaginazione( (short)2 );

		}

		#endregion Costruttori


		#region Proprietà


		public bool possoFiltrareSelezionate {

			get {

				// non ho neanche una foto nella gallery esco subito
				if( isAlmenoUnaFoto == false )
					return false;

				// Ora sono posizionato su tutte, quindi mi chiedo se posso passare alle selezionate
				if( modalitaFiltroSelez == ModalitaFiltroSelez.Tutte )
					if( isAlmenoUnElementoSelezionato == false )
						return false;

				// if( soloSelez == true ) // Attualmente il pulsante E' premuto. Quindi sto vedendo solo le selezionate. Devo dire se posso premere per vedere tutto.
				// posso sempre tornare in modalità : Tutte
				return true;
			}

		}

		private ModalitaFiltroSelez _modalitaFiltroSelez;
		/// <summary>
		/// Modalita di visualizzazione dei risultati.
		/// Quando uso SoloSelez, allora creo un parametro con tutti gli ID
		/// </summary>
		public ModalitaFiltroSelez modalitaFiltroSelez {
			get {
				return _modalitaFiltroSelez;
			}
			set {
				if( modalitaFiltroSelez != value ) {
					_modalitaFiltroSelez = value;
					OnPropertyChanged( "modalitaFiltroSelez" );
				}
			} 
		}


		private GestoreFinestrePubbliche gestoreFinestrePubbliche {
			get {
				return ((App)Application.Current).gestoreFinestrePubbliche;
            }
		}


		public int fotoAttualeRicerca {
			get {
				return countElementiTotali <= 0 ? 0 : paramCercaFoto.paginazione.skip + paginazioneRisultatiGallery;
			}
		}

		public int percentualePosizRicerca {
			get {
				// attuale : totale = x : 100
				return totFotoRicerca == 0 
					   ? 0 
					   : (int) (fotoAttualeRicerca * 100 / totFotoRicerca);
			}
		}

		/// <summary>
		/// Lavoriamo con il layout a griglia.
		/// </summary>
		private short _numRighePag;
		public short numRighePag {
			get {
				return _numRighePag;
			}
			set {
				if( _numRighePag != value ) {

					if( value <= 0 )
						throw new ArgumentException( "Num righe deve essere positivo" );

					// -- stato precedente HQ
					bool primaAltaQualita = isAltaQualita;

					_numRighePag = value;
					OnPropertyChanged( "numRighePag" );

					// -- stato attuale HQ
					bool dopoAltaQualita = isAltaQualita;
					if( primaAltaQualita != dopoAltaQualita ) {
						OnPropertyChanged( "isAltaQualita" );
						OnPropertyChanged( "devoVisualizzareAreaDiRispettoHQ" );
					}
				}
			}
		}

		private short _numColonnePag;
		public short numColonnePag {
			get {
				return _numColonnePag;
			}
			set {
				if( _numColonnePag != value ) {

					if( value <= 0 )
						throw new ArgumentException( "Num colonne deve essere positivo" );

					// -- stato precedente HQ
					bool primaAltaQualita = isAltaQualita;

					_numColonnePag = value;
					OnPropertyChanged( "numColonnePag" );

					// -- stato attuale HQ
					bool dopoAltaQualita = isAltaQualita;
					if( primaAltaQualita != dopoAltaQualita ) {
						OnPropertyChanged( "isAltaQualita" );
						OnPropertyChanged( "devoVisualizzareAreaDiRispettoHQ" );
					}
				}
			}
		}


		/// <summary>
		/// Prima era in configurazione, ora invece lo decide l'utente al volo.
		/// </summary>
        public int paginazioneRisultatiGallery {
			get {
				return (numRighePag * numColonnePag);
			}
		}

		/// <summary>
		/// Mi dice se la visualizzazione corrente prevede alta qualità
		/// </summary>
		public bool isAltaQualita {
			get {
				return FotoGalleryViewModel.vediAltaQualita( numRighePag, numColonnePag );
			}
		}

		public static bool vediAltaQualita( short numRighe, short numColonne ) {
			return (numRighe == 1 && (numColonne == 1 || numColonne == 2));
		}

		/// <summary>
		/// Questo attributo mi serve quando sto per passare dalla vista di molte foto alla vista di una sola foto.
		/// Se nella pagina c'è una foto selezionata, per esempio la 3za, mi devo spostare nella prossima ricerca di 3.
		/// </summary>
		private int offsetProssimoSkip {
			set;
			get;
		}

		/// <summary>
		/// Questa collezione mantiene la lista di tutte le foto selezionate, comprese quelle che non
		/// sono visibili perchè si trovato in pagine diverse da quella corrente.
		/// Tramite questa collezione sono in grado di accendere le foto quando mi sposto con la paginazione.
		/// </summary>
		private ObservableCollectionEx<Guid> idsFotografieSelez {
			get;
			set;
		}

		/// <summary>
		///  Uso questa particolare collectionView perché voglio tenere traccia nel ViewModel degli N-elementi selezionati.
		///  Sottolineo N perché non c'è supporto nativo per questo. Vedere README.txt nel package di questa classe.
		/// </summary>
		private MultiSelectCollectionView<Fotografia> _fotografieCW;
		public MultiSelectCollectionView<Fotografia> fotografieCW 
		{
			get
			{
				return _fotografieCW;
			}
			set
			{
				if (_fotografieCW != value)
				{
					_fotografieCW = value;
					OnPropertyChanged("fotografieCW");
				}
			}
		}

		/// <summary>
		/// Parametri di ricerca inseriti dall'utente
		/// </summary>
		public ParamCercaFoto paramCercaFoto {
			get;
			private set;
		}

		// Salvo informazioni per cambio modalità
		private SaveDataCambioSelezMode saveDataCambioSelezMode { get; set; }

		IFotoExplorerSrv fotoExplorerSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
			}
		}

		public IFotoRitoccoSrv fotoRitoccoSrv
		{
			get
			{
				return LumenApplication.Instance.getServizioAvviato<IFotoRitoccoSrv>();
			}
		}


		public bool isAlmenoUnElementoSelezionato {
			get {
				return countElementiSelezionati > 0;
			}
		}

		public bool isAlmenoUnaFoto {
			get {
				return fotografieCW != null && fotografieCW.Count > 0;
			}
		}

		public bool possoCaricareSlideShow( string modoAutoManuale ) {

			bool posso = false;
			if( modoAutoManuale == "AddSelez" || modoAutoManuale == "ZeroPiuSelez" )
				// Deve esserci almeno una foto selezionata
				posso = countElementiSelezionati > 0;
			else if( modoAutoManuale.Equals( "Tutte", StringComparison.CurrentCultureIgnoreCase ) ) {
				// Qui basta che ho eseguito una ricerca qualsiasi
				posso = isAlmenoUnaFoto;
			}
			return posso;
		}

		public bool possoControllareSlideShow
		{
			get
			{
				return slideShowViewModel != null && slideShowViewModel.slideShow != null;
			}
		}

		public bool possoPlayPauseSlideShow {
			get {
				return possoControllareSlideShow && slideShowViewModel.isLoaded;
			}
		}

		/// <summary>
		/// Questa proprietà mi serve per visualizzare o meno la pulsantiera della vendita con carrello
		/// </summary>
		public bool isPossibileModificareCarrello {
			get {
				return venditoreSrv.isPossibileModificareCarrello;
			}
		}

		public bool possoAggiungereAlMasterizzatore {
			get {

				bool posso = true;

				if( !IsInDesignMode ) {

					if( posso && !isAlmenoUnElementoSelezionato )
						posso = false;

					// Verifico che non abbia fatto nel carrello operazioni di 
					// stampa con errore o abbia caricato un carrello salvato
					if( posso && !venditoreSrv.possoAggiungereMasterizzate )
						posso = false;
				}

				return posso;
			}
		}

		public bool possoStampare {
			get {
				bool posso = true;

				if( !IsInDesignMode ) {

					if( posso && !isAlmenoUnElementoSelezionato )
						posso = false;

					// Verifico che non abbia fatto nel carrello operazioni di 
					// stampa con errore o abbia caricato un carrello salvato
					if( posso )
						if( modoVendita == ModoVendita.Carrello && venditoreSrv.possoAggiungereStampe == false )
							posso = false;
				}

				return posso;
			}
		}

		public bool possoApplicareMetadati {
			get {
				return isAlmenoUnElementoSelezionato;
			}
		}

		public bool possoMandareInModifica
		{
			get {
				return isAlmenoUnElementoSelezionato;
			}
		}
		
		public bool possoEliminareMetadati {
			get {
				return isAlmenoUnElementoSelezionato;
			}
		}


		public bool possoEseguireRicercaCommand
		{
			get {
				bool posso = true;

				/*
				if (Configurazione.isFuoriStandardCiccio)
				{
					if (posso && 
						paramCercaFoto.giornataIniz == null && 
						paramCercaFoto.giornataFine == null)
					{
						posso = false;
					}
				}
				*/

				return posso;
			}
		}

		private IVenditoreSrv venditoreSrv {
			get {
				return (IVenditoreSrv) LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();
			}
		}

		/// <summary>
		/// Ritorno la giornata lavorativa corrente
		/// </summary>
		public DateTime oggi {
			get {
				return LumenApplication.Instance.stato.giornataLavorativa;
			}
		}

		public SelettoreEventoViewModel selettoreEventoViewModel {
			get;
			private set;
		}

		public SelettoreScaricoCardViewModel selettoreScaricoCardViewModel {
			get;
			private set;
		}

		public SelettoreFotografoViewModel selettoreFotografoViewModel {
			get;
			private set;
		}

		public SelettoreMetadatiViewModel selettoreMetadatiViewModel {
			get;
			private set;
		}

		public SelettoreAzioniAutomaticheViewModel selettoreAzioniAutomaticheViewModel {
			get;
			private set;
		}

		public CercaFotoPopupViewModel cercaFotoPopupViewModel {
			get;
			private set;
		}

		public IList<StampanteAbbinata> stampantiAbbinate {
			get;
			private set;
		}


		#region fasi del giorno

		public bool isMattinoChecked {
			get {
				return (paramCercaFoto.fasiDelGiorno.Contains( FaseDelGiorno.Mattino ));
			}
			set {
				paramCercaFoto.setFaseGiorno( FaseDelGiorno.Mattino, value );
			}
		}

		public bool isPomeriggioChecked {
			get {
				return (paramCercaFoto.fasiDelGiorno.Contains( FaseDelGiorno.Pomeriggio ));
			}
			set {
				paramCercaFoto.setFaseGiorno( FaseDelGiorno.Pomeriggio, value );
			}
		}

		public bool isSeraChecked {
			get {
				return( paramCercaFoto.fasiDelGiorno.Contains( FaseDelGiorno.Sera ) );
			}
			set {
				paramCercaFoto.setFaseGiorno( FaseDelGiorno.Sera, value );
			}
		}

		// Questo view model lo recupero dalla application.
		private SlideShowViewModel slideShowViewModel {
			get {
				if (IsInDesignMode)
					return null;

				App myApp = (App)Application.Current;
				return myApp.gestoreFinestrePubbliche.slideShowViewModel;
			}
		}

		public FaseDelGiorno [] fasiDelGiorno {
			get {
				return FaseDelGiornoUtil.fasiDelGiorno;
			}
		}

		#endregion fasi del giorno

		#region Solo vendute
		public bool isSoloVenduteChecked {
			get {
				return (paramCercaFoto.soloVendute != null && paramCercaFoto.soloVendute == true);
			}
			set {
				if( value == true )
					paramCercaFoto.soloVendute = true;
				else
					paramCercaFoto.soloVendute = null;
				OnPropertyChanged( "isSoloVenduteChecked" );
				OnPropertyChanged( "isSoloInvenduteChecked" );
			}
		}

		public bool isSoloInvenduteChecked {
			get {
				return (paramCercaFoto.soloVendute != null && paramCercaFoto.soloVendute == false);
			}
			set {
				if( value == true )
					paramCercaFoto.soloVendute = false;
				else
					paramCercaFoto.soloVendute = null;
				OnPropertyChanged( "isSoloVenduteChecked" );
				OnPropertyChanged( "isSoloInvenduteChecked" );
			}
		}

		#endregion Solo vendute

		public bool possoSelezionareTutto {
			get {
				return fotografieCW != null && fotografieCW.Count > 0;   // Se ho almeno una foto
			}
		}

		public bool possoDeselezionareTutto {
			get {
				return countElementiSelezionati > 0;   // Se ho almeno una foto selezionata
			}
		}


		public SelettoreEventoViewModel selettoreEventoMetadato {
			get;
			private set;
		}

		public short numRigheSlideShow {
			
			get {
				return slideShowViewModel != null ? slideShowViewModel.slideShowRighe : Configurazione.LastUsedConfigLumen.slideShowNumRighe;
			}
			set {
				if( slideShowViewModel != null )
					if( slideShowViewModel.slideShowRighe != value ) {
						slideShowViewModel.slideShowRighe = value;
						OnPropertyChanged( "numRigheSlideShow" );
						Configurazione.LastUsedConfigLumen.slideShowNumRighe = value;
						Configurazione.SalvaLastUsedConfig();
					}
			}
		}

		public short numColonneSlideShow {
			get {
				return slideShowViewModel != null ? slideShowViewModel.slideShowColonne : Configurazione.LastUsedConfigLumen.slideShowNumColonne;
			}
			set {
				if( slideShowViewModel != null )
					if( slideShowViewModel.slideShowColonne != value ) {
						slideShowViewModel.slideShowColonne = value;
						OnPropertyChanged( "numColonneSlideShow" );
                        Configurazione.LastUsedConfigLumen.slideShowNumColonne = value;
                        Configurazione.SalvaLastUsedConfig();
                    }
			}
		}

		public string stringaNumeriFotogrammi {
			get {
				return paramCercaFoto.numeriFotogrammi;
			}
			set {

				if (String.IsNullOrEmpty(value))
					paramCercaFoto.numeriFotogrammi = null;
				else
				{
					if(Configurazione.UserConfigLumen.compNumFoto)
					{
						if (Regex.IsMatch(value, "^[a-zA-Z0-9]+((,^[a-zA-Z0-9]+)?)*"))
						{
							paramCercaFoto.numeriFotogrammi = value;
						}
						else if (Regex.IsMatch(value, "^\\-[a-zA-Z0-9]+((-^[a-zA-Z0-9]+)?)*"))
						{
							paramCercaFoto.numeriFotogrammi = value;
						}
						else
						{
							dialogProvider.ShowError("I numeri dei fotogrammi devono essere separati da virgola", "Formato errato", null);
						}		
					}else{
						if (Regex.IsMatch(value, "\\d+((,\\d+)?)*"))
						{
							paramCercaFoto.numeriFotogrammi = value;
						}
						else if (Regex.IsMatch(value, "\\d+((-\\d+)?)*"))
						{
							paramCercaFoto.numeriFotogrammi = value;
						}
						else
						{
							dialogProvider.ShowError("I numeri dei fotogrammi devono essere separati da virgola", "Formato errato", null);
						}
					}
					OnPropertyChanged("stringaNumeriFotogrammi");
				}
			}
		}


		public SelettoreAzioneRapidaViewModel selettoreAzioniRapideViewModel
		{
			get;
			private set;
		}


		/// <summary>
		/// Ritorno il totale delle foto selezionate comprese quelle che non sono al momento visibili
		/// nella paginata attuale.
		/// </summary>
		public int countElementiSelezionati {
			get {
				return idsFotografieSelez == null ? 0 : idsFotografieSelez.Count();
            }
		}

		/// <summary>
		/// Ritorno il totale delle foto estratte dalla ricerca
		/// (non soltanto quelle che sono a video nella paginata corrente. Tutte quante)
		/// </summary>
		public int countElementiTotali {
			get {
				return totFotoRicerca;
			}
		}


		private int _totFotoRicerca;
		public int totFotoRicerca {
			get {
				return _totFotoRicerca;
			}
			private set {
				if( _totFotoRicerca != value ) {
					_totFotoRicerca = value;
					OnPropertyChanged( "totFotoRicerca" );
					OnPropertyChanged( "countElementiTotali" );
					OnPropertyChanged( "percentualePosizRicerca" );
				}
			}
		}

		/// <summary>
		/// Se ho dei risultati torno 1. Se non ho risultati torno 0
		/// </summary>
		public int minPagineRicerca {
			get {
				return totFotoRicerca > 0 ? 1 : 0;
			}
		}
		
		/// <summary>
		/// Se ho dei risultati caricati e quindi ho anche dei parametri di ricerca,
		/// calcolo il numero totale delle pagine di paginazione
		/// </summary>
		public int totPagineRicerca {
			get {
				if( totFotoRicerca > 0 ) {
					int tot = (totFotoRicerca / paginazioneRisultatiGallery);
					if( totFotoRicerca % paginazioneRisultatiGallery != 0 )
						++tot;
					return tot;
				} else
					return 0;
			}
		}

		public int paginaAttualeRicerca {
			get {
				if( totFotoRicerca > 0 && paramCercaFoto != null ) { 
					int tot = (paramCercaFoto.paginazione.skip / paginazioneRisultatiGallery) + 1;
					return tot;
				} else
					return 0;
			}
		}


		private float _ratioAreaStampabile = 0f;
		public float ratioAreaStampabile {
			get {

				if( _ratioAreaStampabile != 0f )
					return _ratioAreaStampabile;

				// Se indicata una frazione in configurazione uso quella ...
				if( Configurazione.UserConfigLumen.imprimereAreaDiRispetto ) {
					_ratioAreaStampabile = Convert.ToSingle( CoreUtil.evaluateExpression( Configurazione.UserConfigLumen.expRatioAreaDiRispetto ) );
				} else {
					// ... altrimenti uso la prima stampante disponibile
					ISpoolStampeSrv srv = LumenApplication.Instance.getServizioAvviato<ISpoolStampeSrv>();
					_ratioAreaStampabile = (srv == null ? 0f : srv.ratioAreaStampabile);
				}

				return _ratioAreaStampabile;
			}
		}


		public int numFotoCorrenteInSlideShow {
			get {
				int nn = 0;

				if( slideShowViewModel != null )
					if( slideShowViewModel.slidesVisibili != null )
						if( slideShowViewModel.slidesVisibili.Count > 0 )
							nn = slideShowViewModel.slidesVisibili[0].numero;

				return nn;
			}
		}




		public ModoVendita modoVendita
		{
			get
			{
				return venditoreSrv.modoVendita;
			}
			set
			{
				if ( venditoreSrv.modoVendita != value)
				{
					venditoreSrv.modoVendita = value;

					OnPropertyChanged("modoVendita");
					OnPropertyChanged("stringModoVendita");
				}
			}
		}

		public String stringModoVendita
		{
			get
			{
				switch (modoVendita)
				{
					case ModoVendita.Carrello:
						return "Carrello";
					case ModoVendita.StampaDiretta:
						return "Stampa\nDiretta";
				}
				return modoVendita.ToString();
			}
		}

		/// <summary>
		/// Mi dice se devo ritagliare l'area stampabile sulle immagini ad alta qualità
		/// 
		/// </summary>
		public bool devoVisualizzareAreaDiRispettoHQ {
			get {
				return vorreiVisualizzareAreaDiRispettoHQ && isAltaQualita;
			}
		}

		/// <summary>
		/// Questa è la preferenza espressa dall'utente tramite la ui
		/// </summary>
		private bool _vorreiVisualizzareAreaDiRispettoHQ;
		public bool vorreiVisualizzareAreaDiRispettoHQ {
			get {
				return _vorreiVisualizzareAreaDiRispettoHQ;
			}
			set {
				if( _vorreiVisualizzareAreaDiRispettoHQ != value ) {
					_vorreiVisualizzareAreaDiRispettoHQ = value;
					OnPropertyChanged( "vorreiVisualizzareAreaDiRispettoHQ" );

					// rilancio anche la property che viene usata per davvero
					OnPropertyChanged( "devoVisualizzareAreaDiRispettoHQ" );
				}
			}
		}

		public SelezioneEstesa selezioneEstesa {
			get;
			set;
		}

		#endregion Proprietà


		#region Comandi

		private RelayCommand _cambiarePaginazioneCommand;
		public ICommand cambiarePaginazioneCommand {
			get {
				if( _cambiarePaginazioneCommand == null ) {
					_cambiarePaginazioneCommand = new RelayCommand( param => cambiarePaginazione( param ),
																    param => true,
																	false );
				}
				return _cambiarePaginazioneCommand;
			}
		}

		private RelayCommand _selezionareTuttoCommand;
		public ICommand selezionareTuttoCommand {
			get {
				if( _selezionareTuttoCommand == null ) {
					_selezionareTuttoCommand = new RelayCommand( onOff => accendiSpegniTutto( Convert.ToBoolean(onOff) ),
																 onOff => (Convert.ToBoolean(onOff) ? possoSelezionareTutto : possoDeselezionareTutto) );
				}
				return _selezionareTuttoCommand;
			}
		}

		private RelayCommand _aggiungereAlMasterizzatoreCommand;
		public ICommand aggiungereAlMasterizzatoreCommand {
			get {
				if( _aggiungereAlMasterizzatoreCommand == null ) {
					_aggiungereAlMasterizzatoreCommand = new RelayCommand( param => aggiungereAlMasterizzatore()
				                                                           ,param => possoAggiungereAlMasterizzatore 
																		   , false );
				}
				return _aggiungereAlMasterizzatoreCommand;
			}
		}

		private RelayCommand _stampareCommand;
		public ICommand stampareCommand {
			get {
				if( _stampareCommand == null ) {
					_stampareCommand = new RelayCommand( param => stampare( param ), 
						param => possoStampare, false );
				}
				return _stampareCommand;
			}
		}

		private RelayCommand _stampareDirettaCommand;
		public ICommand stampareDirettaCommand {
			get {
				if( _stampareDirettaCommand == null ) {
					_stampareDirettaCommand = new RelayCommand( param => stampare( param, true ),
						                                        param => possoStampare, false );
				}
				return _stampareDirettaCommand;
			}
		}

		private RelayCommand _stampareProviniCommand;
		public ICommand stampareProviniCommand
		{
			get {
				if (_stampareProviniCommand == null)
				{
					_stampareProviniCommand = new RelayCommand(param => stampareProvini(), 
						param => possoStampare, false );
				}
				return _stampareProviniCommand;
			}
		}

		private RelayCommand _filtrareSelezionateCommand;
		public ICommand filtrareSelezionateCommand {
			get {
				if( _filtrareSelezionateCommand == null ) {
					_filtrareSelezionateCommand = new RelayCommand( param => filtrareSelezionate(),
																	param => possoFiltrareSelezionate,
																	false );
				}
				return _filtrareSelezionateCommand;
			}
		}

		private RelayCommand _filtrareTutteCommand;
		public ICommand filtrareTutteCommand {
			get {
				if( _filtrareTutteCommand == null ) {
					_filtrareTutteCommand = new RelayCommand( param => filtrareTutte(),
																	param => possoFiltrareTutte(),
																	false );
				}
				return _filtrareTutteCommand;
			}
		}

		private RelayCommand _filtrareNumFotogrammaCommand;
		public ICommand filtrareNumFotogrammaCommand {
			get {
				if( _filtrareNumFotogrammaCommand == null ) {
					_filtrareNumFotogrammaCommand = new RelayCommand( param => filtrareNumFotogramma( (string)param ),
																	  param => possoFiltrareNumFotogramma( param ) );
				}
				return _filtrareNumFotogrammaCommand;
			}
		}

		private bool possoFiltrareTutte() {

			// non ho neanche una foto nella gallery esco subito
			if( isAlmenoUnaFoto == false )
				return false;

			// posso sempre tornare in modalità : Tutte
			return true;
		}

		private RelayCommand _eseguireRicercaCommand;
		public ICommand eseguireRicercaCommand {
			get {
				if( _eseguireRicercaCommand == null ) {
					_eseguireRicercaCommand = new RelayCommand( param => eseguireRicerca( param as string ), p => possoEseguireRicercaCommand, false );
				}
				return _eseguireRicercaCommand;
			}
		}


		private RelayCommand _eseguireSelezioneEstesaCommand;
		public ICommand eseguireSelezioneEstesaCommand {
			get {
				if( _eseguireSelezioneEstesaCommand == null ) {
					_eseguireSelezioneEstesaCommand = new RelayCommand( param => eseguireSelezioneEstesa( param as Fotografia ),
															  param => true,
															  false );
				}
				return _eseguireSelezioneEstesaCommand;
			}
		}

		private RelayCommand _caricareSlideShowCommand;
		public ICommand caricareSlideShowCommand {
			get {
				if( _caricareSlideShowCommand == null ) {
					_caricareSlideShowCommand =
						new RelayCommand( autoManual => caricareSlideShow( (string)autoManual ),
										  autoManual => possoCaricareSlideShow( (string)autoManual ),
                                          false,
                                          autoManual => deselezionareTutto());
				}
				return _caricareSlideShowCommand;
			}
		}

		private RelayCommand _controllareSlideShowCommand;
		public ICommand controllareSlideShowCommand {
			get {
				if( _controllareSlideShowCommand == null ) {
					_controllareSlideShowCommand = new RelayCommand( azione => controllareSlideShow( azione ),
					                                                 azione => possoControllareSlideShow);
				}
				return _controllareSlideShowCommand;
			}
		}

		private RelayCommand _playPauseSlideShowCommand;
		public ICommand playPauseSlideShowCommand {
			get {
				if( _playPauseSlideShowCommand == null ) {
					_playPauseSlideShowCommand = new RelayCommand( azione => playPauseSlideShow(),
																   azione => possoPlayPauseSlideShow );
				}
				return _playPauseSlideShowCommand;
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

		private RelayCommand _mandareInModificaCommand;
		public ICommand mandareInModificaCommand {
			get {
				if( _mandareInModificaCommand == null ) {
					_mandareInModificaCommand = new RelayCommand( param => mandareInModifica(),
																  param => possoMandareInModifica,
																  false	);
				}
				return _mandareInModificaCommand;
			}
		}

		private RelayCommand _commandSpostarePaginazione;
		public ICommand commandSpostarePaginazione {
			get {
				if( _commandSpostarePaginazione == null ) {
					_commandSpostarePaginazione = new RelayCommand( delta => spostarePaginazione( Convert.ToInt16(delta) ),
																	delta => possoSpostarePaginazione( Convert.ToInt16( delta ) ),
																    false );
				}
				return _commandSpostarePaginazione;
			}
		}

		private RelayCommand _spostarePaginazioneConNumFotoCommand;
		public ICommand spostarePaginazioneConNumFotoCommand
		{
			get
			{
				if( _spostarePaginazioneConNumFotoCommand == null ) {
					_spostarePaginazioneConNumFotoCommand = new RelayCommand( numFoto => spostarePaginazioneConNumFoto( Convert.ToInt32( numFoto ) ),
					                                                          numFoto => possoSpostarePaginazioneConNumFoto( Convert.ToInt32( numFoto ) ),
																			  false );
				}
				return _spostarePaginazioneConNumFotoCommand;
			}
		}

		private RelayCommand _commandVedereAncoraInfoImg;
		public ICommand commandVedereAncoraInfoImg {
			get {
				if( _commandVedereAncoraInfoImg == null ) {
					_commandVedereAncoraInfoImg = new RelayCommand( p => MessageBox.Show("TODO: info"),
																	p => true );
				}
				return _commandVedereAncoraInfoImg;
			}
		}

		public event SelezioneCambiataEventHandler selezioneCambiata;

		private RelayCommand _riportaOriginaleFotoSelezionateCommand;
		public ICommand riportaOriginaleFotoSelezionateCommand
		{
			get
			{
				if (_riportaOriginaleFotoSelezionateCommand == null)
				{
					_riportaOriginaleFotoSelezionateCommand = new RelayCommand(param => riportaOriginaleFotoSelezionate(),
															  param => true,
															  false);
				}
				return _riportaOriginaleFotoSelezionateCommand;
			}
		}

		private RelayCommand _aprireCercaFotoPopupCommand;
		public ICommand aprireCercaFotoPopupCommand
		{
			get
			{
				if( _aprireCercaFotoPopupCommand == null ) {
					_aprireCercaFotoPopupCommand = new RelayCommand( param => aprireCercaFotoPopup( (ModoRicercaPop)param ), 
					                                                 param => possoAprireCercaFotoPopup( (ModoRicercaPop)param ) );
				}
				return _aprireCercaFotoPopupCommand;
			}
		}

		private RelayCommand _pauseRunStampantiCommand;
		public ICommand pauseRunStampantiCommand {
			get {
				if( _pauseRunStampantiCommand == null ) {
					_pauseRunStampantiCommand = new RelayCommand( pause => pauseRunStampanti( (bool) pause ),
																  pause => true );
				}
				return _pauseRunStampantiCommand;
			}
		}


		#endregion Comandi


		#region Metodi

		public void setRapideTargetSingolaFoto( Fotografia foto ) {

			// Ho cliccato con il destro sulla singola foto.
			// Memorizzo la foto per eventuali operazioni da lanciare
			selettoreAzioniRapideViewModel.setTarget( foto );
		}

		private bool possoFiltrareNumFotogramma( object numero ) {

			if( isAlmenoUnaFoto == false )
				return false;

			return true;
		}

		protected override void OnDispose() {

			selettoreScaricoCardViewModel.scarichiCardsCW.SelectionChanged -= scarichiCardsCW_SelectionChanged;

			base.OnDispose();
		}

		private void scarichiCardsCW_SelectionChanged( object sender, SelectionChangedEventArgs e ) {

			// Se ho selezionato uno scarico card, azzero gli altri filtri.
			if( selettoreScaricoCardViewModel.countElementiSelezionati > 0 )
				azzeraParamRicerca( true );

		}

		/// <summary>
		/// Carico tutti i formati carta che sono abbinati alle stampanti installate
		/// Per ognuno di questi elementi, dovrò visualizzare un pulsante per la stampa
		/// </summary>
		private void caricaStampantiAbbinate() {

			string ss = Configurazione.UserConfigLumen.stampantiAbbinate;

			// TODO sostituire con la lista presa da spoolsrv
			this.stampantiAbbinate = StampantiAbbinateUtil.deserializza( ss );
		}

		/// <summary>
		/// eseguo il cambio di paginazione. 
		/// </summary>
		/// <param name="param">Può essere :
		///          * short se chiamato internamente
		///          * string se usato tramite binding
		///          * Fotografia se mi devo posizionare in HQ su quella
		/// </param>

		private void cambiarePaginazione( object param ) {

			short stelline = -1;
			Fotografia primaFotoSelezionata = null;

			if( param is short || param is int ) {
				stelline = (short)param;
			} else if( param is string ) {
				stelline = Convert.ToInt16( (string)param );
			}

			if( stelline == 1 )
				primaFotoSelezionata = fotografieCW == null ? null : fotografieCW.SelectedItems.FirstOrDefault();

			if( stelline < 0 && param is Fotografia ) {
				stelline = 1;
				primaFotoSelezionata = (Fotografia)param;
			}

			// Mi salvo le righe per capire se sto passando da una risoluziona bassa (tante foto) ad una risoluziona alta (una foto)	
			var saveRig = numRighePag;
			var saveCol = numColonnePag;
			int saveTot = saveRig * saveCol;


			int idx = stelline - 1;
			numRighePag = Configurazione.UserConfigLumen.prefGalleryViste[idx].numRighe;
			numColonnePag = Configurazione.UserConfigLumen.prefGalleryViste[idx].numColonne;
			int newTot = numRighePag * numColonnePag;

			// Se non è cambiato nulla, esco subito
			if( saveRig == numRighePag && saveCol == numColonnePag )
				return;


			bool cambioInHQ = false;
			if( stelline == 1 ) {
				if( saveRig > 1 || saveCol > 1 ) {

					cambioInHQ = true;


					if( primaFotoSelezionata != null ) {
						int index = fotografieCW.IndexOf( primaFotoSelezionata );
						if( Configurazione.UserConfigLumen.invertiRicerca )
							offsetProssimoSkip = index;
						else {
							offsetProssimoSkip = ( -1 * (saveTot - index) ) + saveTot;
						}		
					}
				}
			}

			// Se aumento il numero di foto, devo per forza andare a rileggerle (perché in memoria non ci sono)
			bool incrementoVisibilita = saveTot > 0 && saveTot < newTot;

			bool lancioRicerca = (cambioInHQ || incrementoVisibilita);

			// Prima ero in bassa qualità perché vedevo molte foto, ... adesso ne vedo solo una quindi passo in HQ
			if( lancioRicerca ) {
				// Ho provato diversi trucchi ma non c'è modo di farlo lato UI. 
				// Rieseguo la ricerca qui nel viewmodel
				// 			RicercaFlags flags = RicercaFlags.NuovaRicerca | RicercaFlags.MantenereSelezionate | RicercaFlags.MantenereListaIds;
				eseguireRicerca( RicercaFlags.Niente );
			} else {

				OnPropertyChanged( "minPagineRicerca" );
				OnPropertyChanged( "totPagineRicerca" );
				OnPropertyChanged( "paginaAttualeRicerca" );
			}

		}

		private void filtrareNumFotogramma( string nnn ) {

			// Alcune collezioni non sono filtrabili, per esempio la IEnumerable
			if( fotografieCW.CanFilter == false )
				return;

			if( String.IsNullOrEmpty(nnn) == false ) {

				int numDaric = Int32.Parse( nnn.ToString() );

				Predicate<object> mioFinder = obj => {
					return ((Fotografia)obj).numero == numDaric;
				};
				fotografieCW.Filter = mioFinder;
			} else {
				fotografieCW.Filter = null;
			}
		}

		/// <summary>
		/// E' il contrario di filtrareTutte()
		/// </summary>
		private void filtrareSelezionate() {

			modalitaFiltroSelez = ModalitaFiltroSelez.SoloSelezionate;

			// Mi metto in modalita di "solo selezionate"
			saveDataCambioSelezMode = new SaveDataCambioSelezMode( paramCercaFoto.deepCopy<ParamCercaFoto>() );  //Vedi formule magiche
			saveDataCambioSelezMode.numRighePag = numRighePag;
			saveDataCambioSelezMode.numColonnePag = numColonnePag;

			// Devo rieseguire la query con dei parametri opportunamente creati per lo scopo
			paramCercaFoto = new ParamCercaFoto();
			paramCercaFoto.idsFotografie = idsFotografieSelez.ToArray<Guid>();

			idsFotografieSelez = new ObservableCollectionEx<Guid>( paramCercaFoto.idsFotografie );

			RicercaFlags flags = RicercaFlags.NuovaRicerca | RicercaFlags.MantenereSelezionate | RicercaFlags.MantenereListaIds;
			eseguireRicerca( flags );

			// Controllo se ho un solo risultato, allora mi posiziono con una sola stellina (alta risoluzione)
			if( idsFotografieSelez.Count == 1 ) {
				cambiarePaginazione( (short)1 );
			}

		}

		/// <summary>
		/// E' il contrario di filtrareSelezionate()
		/// </summary>
		void filtrareTutte() {

			// Se non ho i dati per tornare indietro, esco, perché significa che è già partita un'altra ricerca che sta resettando tutto.
			if( saveDataCambioSelezMode == null )
				return;

			// Mi metto in modalita di "tutte"
			modalitaFiltroSelez = ModalitaFiltroSelez.Tutte;

			// Questi erano gli id selezionati prima di entrare in modalita solo selez
			// var saveIds = paramCercaFoto.idsFotografie;
			// Questi sono invece gli id delle foto selezionate adesso
			Guid[] saveIds = new Guid[idsFotografieSelez.Count];
			idsFotografieSelez.CopyTo( saveIds, 0 );

			// Rimetto a posto i parametri e ricerco di nuovo
			paramCercaFoto = saveDataCambioSelezMode.paramCercaFoto.deepCopy();

			numRighePag = saveDataCambioSelezMode.numRighePag;
			numColonnePag = saveDataCambioSelezMode.numColonnePag;

			// Ricarico la lista dei selezionati --------------------------------------che nel frattempo si è svuotata
			idsFotografieSelez = new ObservableCollectionEx<Guid>( saveIds );

			eseguireRicerca( RicercaFlags.NuovaRicerca | RicercaFlags.MantenereSelezionate );


			// Mi sposto sulla pagina da cui ero partito
			short deltaPagine = (short)(saveDataCambioSelezMode.paramCercaFoto.paginazione.skip / saveDataCambioSelezMode.paramCercaFoto.paginazione.take);
			spostarePaginazione( deltaPagine );

		}


		/// <summary>
		/// Aggiungo le immagini selezionate al masterizzatore
		/// </summary>
		/// <returns></returns>
		public void aggiungereAlMasterizzatore() {

			IEnumerable<Fotografia> listaSelez = creaListaFotoSelezionate();
			venditoreSrv.aggiungereMasterizzate( listaSelez );
			deselezionareTutto();
		}


		/// <summary>
		/// Crea una lista con tutte le fotografie della query, senza paginazione. Occhio che possono essere tante.
		/// </summary>
		/// <returns></returns>
		private IList<Fotografia> creaListaTutteFotoRicerca() {

			ParamCercaFoto paramAperti = paramCercaFoto.deepCopy();
			paramAperti.paginazione = null;
			paramAperti.idratareImmagini = false;
			return fotoExplorerSrv.cercaFotoTutte( paramAperti );
        }

		/// <summary>
		/// Crea una lista con tutte le Fotografie selezionate
		/// </summary>
		/// <returns></returns>
		private IList<Fotografia> creaListaFotoSelezionate() {

			List<Fotografia> lista = new List<Fotografia>( idsFotografieSelez.Count );

			foreach( Guid guid in idsFotografieSelez ) {

				Fotografia f = getFotoById( guid );
				if( f != null ) {

					try {
						AiutanteFoto.idrataImmaginiFoto( f, IdrataTarget.Provino );
					} catch( Exception ee ) {
						_giornale.Warn( "Ho fallito di idratare il provino: " + guid, ee );
					}

					lista.Add( f );
				} else {
					_giornale.Warn( "foto non trovata: " + guid + " . Come mai?" );
				}
			}

			return lista;
		}

		private IEnumerable<Fotografia> creaListaFotoTutte() {

			List<Fotografia> lista = null;

			using( IFotoExplorerSrv fotoExplorerSrv2 = LumenApplication.Instance.creaServizio<IFotoExplorerSrv>() ) {

				// Devo rieseguire la query completa : tolto la paginazione
				ParamCercaFoto paramTutti = paramCercaFoto.deepCopy();
				paramTutti.paginazione = null;

				fotoExplorerSrv2.cercaFoto( paramTutti );
				lista = fotoExplorerSrv2.fotografie;
			}

			return lista;
		}
		
		public void selezionareMolte( IEnumerable<Fotografia> listaFoto ) {

			foreach( var foto in listaFoto )
				selezionareSingola( foto, false, false );

			// Aggiorno la UI				
			fotografieCW.RefreshSelectedItemWithMemory();

			// Notifico eventuali altri componenti della applicazione
			notificaSelezioneCambiata();
		}

		public void selezionareSingola( Fotografia foto ) {
			selezionareSingola( foto, true, true );
		}

		public void selezionareSingola( Fotografia foto, bool forzareRefreshMem, bool notificareSelezioneCambiata ) {

			if( foto == null ) {
				_giornale.Warn( "selezionare foto nulla" );
				return;
			}

			bool cambiata = false;

			// Prima lavoro sulla lista in memoria che mi serve per i cambi pagina
			if( idsFotografieSelez != null && ! idsFotografieSelez.Contains( foto.id ) ) {
				idsFotografieSelez.Add( foto.id );
				cambiata = true;
			}
				
			// Poi lavoro sulla collezione visuale della pagina
			if( fotografieCW != null ) {
				if( fotografieCW.SourceCollection.OfType<Fotografia>().Contains( foto ) ) {
					if( ! fotografieCW.SelectedItems.Contains( foto ) ) {
						fotografieCW.SelectedItems.Add( foto );
						cambiata = true;

						// Questo metodo non dovrebbe essere pubblico
						// Inoltre non ci dovrebbe essere il bisogno di chiamarlo
						// Ma senza di questo, non funziona la selezione con il tasto destro dalla gallery
						// TODO vedere se si può eliminare
						if( forzareRefreshMem )
							fotografieCW.RefreshSelectedItemWithMemory();
					}
				}
			}

			if( cambiata && notificareSelezioneCambiata )
				notificaSelezioneCambiata();

		}

		private void notificaSelezioneCambiata() {
			raiseSelezioneCambiataEvent();
			raisePropertyChangeSelezionate();
		}

		public void deselezionareSingola( Fotografia foto ) {

			bool cambiata = false;

			// Prima lavoro sulla lista in memoria che mi serve per i cambi pagina
			if( idsFotografieSelez != null && idsFotografieSelez.Contains( foto.id ) ) { 
				idsFotografieSelez.Remove( foto.id );
				cambiata = true;
			}

			// Poi lavoro sulla collezione visuale della pagina
			if( fotografieCW != null && fotografieCW.SelectedItems.Contains( foto ) ) {
				fotografieCW.SelectedItems.Remove( foto );
				cambiata = true;
			}

			if( cambiata )
				notificaSelezioneCambiata();
		}

		private void selezionareTutto() {
			accendiSpegniTutto( true );
		}

		public void deselezionareTutto() {
			accendiSpegniTutto( false );
		}

		/// <summary>
		/// Accendo o Spengo tutte le selezioni
		/// </summary>
		private void accendiSpegniTutto( bool selez ) {

			// Prima lavoro sulla collezione globale
			if( idsFotografieSelez != null ) {

				if( selez ) {
					// Devo caricare tutti gli ID risultato della riceca
					// TODO : non è facile. devo rieseguire la query che mi torni tutti gli ID (ma senza paginazione questa volta)
				} else
					azzeraFotoSelez();
			}

			// Poi lavoro sulla collezione visuale della pagina
			if( fotografieCW != null ) {
				if( selez )
					fotografieCW.selezionaTutto();
				else
					fotografieCW.deselezionaTutto();
			}

			// Avviso del cambio di selezione
			notificaSelezioneCambiata();
		}

		private void stampare( object objStampanteAbbinata ) {
			stampare( objStampanteAbbinata, modoVendita == ModoVendita.StampaDiretta );
			//stampare( objStampanteAbbinata, Configurazione.UserConfigLumen.modoVendita == ModoVendita.StampaDiretta );
		}

		
		/// <summary>
		/// Devo mandare in stampa le foto selezionate
		/// Nel parametro mi arriva l'oggetto StampanteAbbinata che mi da tutte le indicazioni
		/// per la stampa: il formato carta e la stampante
		/// </summary>
		private void stampare( object objStampanteAbbinata, bool stampaDiretta ) {
			
			StampanteAbbinata stampanteAbbinata = (StampanteAbbinata)objStampanteAbbinata;

			IList<Fotografia> listaSelez = creaListaFotoSelezionate();

			// Se ho selezionato più di una foto, e lavoro in stampa diretta, allora chiedo conferma
			bool procediPure = true;
			int quante = listaSelez.Count;
			if ( stampaDiretta &&
				Configurazione.UserConfigLumen.sogliaNumFotoConfermaInStampaRapida > 0 &&
				quante >= Configurazione.UserConfigLumen.sogliaNumFotoConfermaInStampaRapida )
			{
				dialogProvider.ShowConfirmation( "Confermi la stampa  di " + quante + " foto ?", "Stampa diretta senza carrello",
				  (confermato) => {
					  procediPure = confermato;
				  } );
			}

			if( procediPure ) {
				if( stampaDiretta ){
					using( IVenditoreSrv venditoreStampaDiretta = LumenApplication.Instance.creaServizio<IVenditoreSrv>() ) 
					{
						venditoreStampaDiretta.start();
						venditoreSrv.modoVendita = ModoVendita.StampaDiretta;
						venditoreStampaDiretta.creareNuovoCarrello();
						venditoreStampaDiretta.carrello.intestazione = VenditoreSrvImpl.INTESTAZIONE_STAMPA_RAPIDA;
						venditoreStampaDiretta.aggiungereStampe(listaSelez, creaParamStampaFoto(stampanteAbbinata));

						// La vendita con stampa diretta, non la gestisco con il self service
						venditoreStampaDiretta.carrello.visibileSelfService = false;

						string msgErrore = venditoreStampaDiretta.vendereCarrello();



						bool esitoOk = (msgErrore == null);
                        if( esitoOk )
						{
							this.trayIconProvider.showInfo( "Vendita ok", "Incassare " + venditoreStampaDiretta.carrello.totaleAPagare + " euro", 3000 );
						}
						else
						{
							dialogProvider.ShowError("Errore inserimento carrello nella cassa","Errore", null);
						}

						venditoreStampaDiretta.stop();
					}
				}else{
					venditoreSrv.aggiungereStampe( listaSelez, creaParamStampaFoto( stampanteAbbinata ) );
				}
				// Spengo tutto
				deselezionareTutto();
			}
		}

		private void stampareProvini()
		{
			StampaProviniDialog d = new StampaProviniDialog();
			d.totaleFotoSelezionate = countElementiSelezionati;
			d.totoleFotoGallery = countElementiTotali;
			bool? esito = d.ShowDialog();

			if (esito == true)
			{
				IList<Fotografia> listaFotos;

				if( d.stampaSoloSelezionate )
					listaFotos = creaListaFotoSelezionate();
				else
					listaFotos = creaListaTutteFotoRicerca();


				// Riordino i Provini per data acquisizione foto + numero foto (Prima quelli più vecchi)
				IEnumerable<Fotografia> sortedEnum = listaFotos.OrderBy(f => f.dataOraAcquisizione).OrderBy(f => f.numero);
				listaFotos = sortedEnum.ToList();
	
// non capisco a cosa servisse clonare i parametri			
//				venditoreSrv.aggiungereStampe(listaSelez, creaParamStampaProvini(d.paramStampaProvini));
				venditoreSrv.aggiungereStampe( listaFotos, d.paramStampaProvini );

			}

			d.Close();

			// Spengo tutto
			deselezionareTutto();

		}
		
		/// <summary>
		/// Creo i parametri di stampa, mixando un pò di informazioni prese
		/// dalla configurazione, dallo stato dell'applicazione, e dalla scelta dell'utente.
		/// </summary>
		/// <param name="stampanteAbbinata"></param>
		/// <returns></returns>
		private ParamStampaFoto creaParamStampaFoto( StampanteAbbinata stampanteAbbinata ) {

			ParamStampaFoto p = venditoreSrv.creaParamStampaFoto();

			p.nomeStampante = stampanteAbbinata.StampanteInstallata.NomeStampante;
			p.formatoCarta = stampanteAbbinata.FormatoCarta;
			// TODO per ora il nome della Porta a cui è collegata la stampante non lo uso. Non so cosa farci.

			return p;
		}

		private void eseguireRicerca( string param ) {

			// Prima cambio modalità ...
			modalitaFiltroSelez = ModalitaFiltroSelez.Tutte;

			// ... poi eseguo la ricerca
			RicercaFlags flags = RicercaFlags.NuovaRicerca | RicercaFlags.ConfermaSeFiltriVuoti;
			eseguireRicerca( flags );
		}


		/// <summary>
		/// Eseguo una ricerca sul database. Quindi aggiorno la UI
		/// </summary>
		/// <param name="nuovaRicerca">Se true significa che ho premuto il tasto RICERCA e quindi devo ripartire da capo.
		/// Se invece è FALSE significa che sto paginando su di una ricerca già effettuata in precedenza
		/// </param>
		private void eseguireRicerca( RicercaFlags flags ) {

			bool nuovaRicerca = flags.HasFlag( RicercaFlags.NuovaRicerca );
			bool chiediConfermaSeFiltriVuoti = flags.HasFlag( RicercaFlags.ConfermaSeFiltriVuoti );

			completaParametriRicercaWithOrder( true );

			// Se eseguo una nuova ricerca, parto la paginazione da zero
			if( nuovaRicerca )
				paramCercaFoto.paginazione.skip = 0;


			// Dopo aver completato tutti i parametri di ricerca...
			// verifico se ho impostato almeno un parametro
			if( chiediConfermaSeFiltriVuoti )
				if (verificaChiediConfermaRicercaSenzaParametri() == false)
					return;

			// Solo quando premo tasto di ricerca (e non durante la paginazione).
			if( nuovaRicerca ) {

				// Se non mi viene proibito esplicitamente, in una nuova ricerca devo azzerare la lista delle foto selezionate

				if( flags.HasFlag( RicercaFlags.MantenereSelezionate ) == false )
					azzeraFotoSelez();

				// Se non mi viene proibito esplicitamente, in una nuova ricerca devo azzerare la lista degli ids perché non è modificabile dall'utente
				if( flags.HasFlag( RicercaFlags.MantenereListaIds ) == false )
					paramCercaFoto.idsFotografie = null;

				contaTotFotoRicerca();
			}

			// Eseguo la ricerca nel database
			fotoExplorerSrv.cercaFoto( paramCercaFoto );

			azioniPostRicerca( flags );

			// Non mettere piu niente qui perché sta idratando le foto in background (o cmq fare attenzione)
		}

		/// <summary>
		/// In base a quante foto ha trovato il servizio,
		/// imposto le property di paginazione posizionandomi all'inizio.
		/// Quindi questo avviene solo quando premo il tasto "Ricerca" e non quando sto paginando
		/// </summary>
		private void contaTotFotoRicerca() {

			totFotoRicerca = fotoExplorerSrv.contaFoto( this.paramCercaFoto );
		}


		/// <summary>
		/// Ricreo la collectionview con le fotografie da visualizzare
		/// lancio in background la idratazione dei provini
		/// </summary>
		private void azioniPostRicerca( RicercaFlags flags) {

			// Se avevo un worker già attivo, allora provo a cancellarlo.
			if( _bkgIdrata.IsBusy ) {
				
				_bkgIdrata.CancelAsync();
				_giornale.Debug( "idratatore impegnato. Lo stoppo" );

				int antiloop = 0;
				while( _bkgIdrata.IsBusy && antiloop < 200 ) {  // dopo 10 secondi esco comunque altrimenti mi si ferma tutto
					System.Windows.Forms.Application.DoEvents();
					System.Threading.Thread.Sleep( 50 );
					++antiloop;
				}
			}

			// ricreo la collection-view e notifico che è cambiato il risultato. Le immagini verranno caricate poi
			ricreaCollectionViewFoto( fotoExplorerSrv.fotografie );

			// Se è una nuova ricerca....
			if( flags.HasFlag( RicercaFlags.NuovaRicerca ) ) {

				// ... e se non mi è stato indicato il contrario, ... deseleziono tutte le foto
				if( flags.HasFlag( RicercaFlags.MantenereSelezionate ) == false )
					deselezionareTutto();

			}

			// Se non ho trovato nulla, allora avviso l'utente
			if( fotografieCW.Count <= 0 )
				dialogProvider.ShowMessage( "Nessuna fotografia trovata con questi filtri di ricerca", "AVVISO" );

			OnPropertyChanged( "isAlmenoUnaFoto" );
			OnPropertyChanged( "fotoAttualeRicerca" );
			OnPropertyChanged( "percentualePosizRicerca" );

			// Paginazione
			OnPropertyChanged( "minPagineRicerca" );
			OnPropertyChanged( "totPagineRicerca" );
			OnPropertyChanged( "paginaAttualeRicerca" );
			// Paginazione

			raisePropertyChangeSelezionate();

			// Ora ci penso io ad idratare le immagini, perchè devo fare questa operazione nello stesso thread della UI
			if( !_bkgIdrata.IsBusy )
				_bkgIdrata.RunWorkerAsync();
			// Lasciare come ultima cosa l'idratazione delle foto.

		}

		private bool selezionareElementiPaginaCorrente() {

			// illumino gli elementi eventualmente selezionati in precedenza
			bool eseguito = false;
            foreach( Guid id in idsFotografieSelez ) {

				Fotografia fotoSelezionata = fotografieCW.SourceCollection.OfType<Fotografia>().SingleOrDefault( f => f.id == id );
				if( fotoSelezionata != null ) {
					if( fotografieCW.seleziona( fotoSelezionata ) )
						eseguito = true;
				}
			}


			if( eseguito )
				fotografieCW.Refresh();

			return eseguito;
		}

		private bool verificaChiediConfermaRicercaSenzaParametri()
		{
			bool procediPure = true;
			
			if (!paramCercaFoto.isEmpty())
				return procediPure;
#if false
			procediPure = false;
			StringBuilder msg = new StringBuilder("Attenzione: stai eseguendo una ricerca senza parametri.\nConfermi?");
			dialogProvider.ShowConfirmation(msg.ToString(), "Richiesta conferma",
				(confermato) =>
				{
					procediPure = confermato;
				});
#endif
			return procediPure;
		}

		/// <summary>
		/// Questa è la prima foto della selezione multipla
		/// </summary>
		public Fotografia fotoCorrente {
			get {
				return fotografieCW.CurrentItem as Fotografia;
			}
		}

		/// <summary>
		/// Questa è l'elemento corrente che punta alle foto selezionate.
		/// Mi serve per spostarmi avanti e indietro.
		/// Voglio scorrere attraverso le foto selezionate.
		/// </summary>
		public Fotografia fotoCorrenteSelezionataScorrimento {
			get;
			private set;
		}

		void fotografie_selezioneCambiata( object sender, SelectionChangedEventArgs e ) {

			// Eventuali aggiunte
			foreach( var item in e.AddedItems ) {

				Fotografia fa = (Fotografia) item;
				addSelezionata( fa );

				// --

				// Gestisco la selezione estesa
				if( selezioneEstesa == null )
					selezioneEstesa = new SelezioneEstesa();
				else
					selezioneEstesa.azzera();

				selezioneEstesa.limiteA = fa;
			}

			// Eventuali rimozioni
			foreach( var item in e.RemovedItems ) {

				Fotografia fr = (Fotografia)item;
				removeSelezionata( fr );

				// --

				// Gestisco la selezione estesa
				if( selezioneEstesa != null )
					selezioneEstesa.remove( fr );
			}

			notificaSelezioneCambiata();
		}

		/// <summary>
		///   Avviso eventuali ascoltatori esterni
		/// </summary>
		private void raiseSelezioneCambiataEvent() {

			if( selezioneCambiata != null )
				selezioneCambiata( this, EventArgs.Empty );
		}

		void raisePropertyChangeSelezionate() {
			OnPropertyChanged( "isAlmenoUnElementoSelezionato" );
			OnPropertyChanged( "countElementiSelezionati" );
			OnPropertyChanged( "possoFiltrareSelezionate" );
		}


		/// <summary>
		/// In questa routine non si deve usare la collezione di foto presente nel servizio,
		/// ma occorre usare la collectionview mia interna.
		/// Questo perché durante le ricerche, il servizio può azzerare la collezione, mentre inveve la mia cw è gestita 
		/// per tenere conto delle operazioni in background
		/// </summary>
		/// <param name="sender">il background worker</param>
		/// <param name="e"></param>
		private void bkgIdrata_DoWork( object sender, DoWorkEventArgs e ) {

		
			BackgroundWorker worker = sender as BackgroundWorker;
			_giornale.Debug( "Inizio a idratare le foto in background" );


			int tot = fotografieCW.Count;
			worker.ReportProgress( 0 );


			for( int ii = 0; (ii < tot); ii++ ) {

				if( (worker.CancellationPending == true) ) {
					e.Cancel = true;
					break;
				} else {

					Fotografia foto = (Fotografia) fotografieCW.GetItemAt( ii );

					// Aggiorno la percentuale di progressi di idratazione. Esiste una ProgressBar che si abilita all'uopo.
					int perc = (ii + 1) * 100 / tot;
					worker.ReportProgress( perc, foto );

			//		System.Threading.Thread.Sleep( 200 );
                }
			}

			// Se sono arrivato in fondo, comunico il progresso massimo giusto per sicurezza.
			if( !e.Cancel )
				worker.ReportProgress( 100 );
			
		}

		void bkgIdrata_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e ) {

			_giornale.Debug( "Terminato di idratare le foto in background: abortito = " + e.Cancelled );

			if( ! e.Cancelled ) {

				// Illumino le foto eventualmente selezionate nella pagina corrente
				if( selezionareElementiPaginaCorrente() == false )
					fotografieCW.Refresh();

				RicercaModificataMessaggio msg = new RicercaModificataMessaggio( this );
				msg.abortito = e.Cancelled;
				LumenApplication.Instance.bus.Publish( msg );
			}

		}

		/// <summary>
		/// Eseguo le operazioni di idratazione immagini direttamente in questo metodo perché solo questo metodo del background worker
		/// viene eseguito nel thread della UI. In questo modo posso far visualizzare le foto man mano che vengono idratate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void bkgIdrata_ProgressChanged( object sender, ProgressChangedEventArgs e ) {

			if( e.UserState == null ) {

			} else {

				Fotografia foto = (Fotografia)e.UserState;

				try {

					// Perform a time consuming operation and report progress.
					AiutanteFoto.idrataImmaginiFoto( foto, IdrataTarget.Provino );

				} catch( Exception ) {

					// Provo a crearlo. Magari non c'è perché è stato cancellato per sbaglio, oppure c'è ma si è corrotto.
					try {

						AiutanteFoto.creaProvinoFoto( foto );
					} catch( Exception ) {

						_giornale.Debug( "Problemi nel provinare la foto: " + foto );

						// Se qualcosa va male, pazienza, a questo punto non posso fare altro che tirare avanti.
						// TODO : forse dovrei togliere la foto in esame dalla collezione della gallery ....
					}
				}

				
			}
				
		}

		private void completaParametriRicerca()
		{
			completaParametriRicercaWithOrder( false );
		}

		/// <summary>
		/// Ricerca con intorno. Esempio:    *nf*amp$
		/// nf = il numero del fotogramma
		/// amp = ampiezza dell'intorno
		/// </summary>
		private static readonly Regex regexNumIntorno1 = new Regex( @"^\*([0-9]+)\*([0-9]+)$" );
		/// <summary>
		/// Ricerca con intorno. Esempio  *nf*
		/// nf = numero fotogramma.
		/// (l'ampiezza viene presa per default)
		/// </summary>
		private static readonly Regex regexNumIntorno2 = new Regex( @"^\*([0-9]+)\*$" );

		/// <summary>
		/// Sistemo i parametri e gestisco la paginazione
		/// </summary>
		/// <param name="ordinamento asc/desc">il numero di pagina</param>
		private void completaParametriRicercaWithOrder( bool usaOrdinamentoAsc ) {

			paramCercaFoto.idratareImmagini = false;

			// Ordinamento
			if( usaOrdinamentoAsc ) {
				if( Configurazione.UserConfigLumen.invertiRicerca )
					paramCercaFoto.ordinamento = Ordinamento.Asc;
				else
					paramCercaFoto.ordinamento = Ordinamento.Desc;
			}

			// Aggiungo eventuale parametro il fotografo
			if( selettoreFotografoViewModel.countElementiSelezionati > 0 )
				paramCercaFoto.fotografi = selettoreFotografoViewModel.fotografiSelezionati.ToArray();
			else
				paramCercaFoto.fotografi = null;

			// Aggiungo eventuale parametro l'evento
			if( selettoreEventoViewModel.eventoSelezionato != null )
				paramCercaFoto.eventi = new Evento[] { selettoreEventoViewModel.eventoSelezionato };
			else
				paramCercaFoto.eventi = null;

			// Aggiungo eventuale parametro lo scarico card.
			if( selettoreScaricoCardViewModel.scarichiCardsCW != null && selettoreScaricoCardViewModel.scarichiCardsCW.SelectedItems.Count > 0 )
				paramCercaFoto.scarichiCard = selettoreScaricoCardViewModel.scarichiCardsCW.SelectedItems.ToArray();
			else
				paramCercaFoto.scarichiCard = null;

			// Ampiezza finestra di paginazione
			paramCercaFoto.paginazione.take = paginazioneRisultatiGallery;

			// Gestisco eventuale offset personalizzato per spostarmi ulteriormente.
			// Poi lo azzero perché deve ritornare la paginazione normale
			paramCercaFoto.paginazione.skip += offsetProssimoSkip;
			offsetProssimoSkip = 0;

			// Intorno.
			// Questa espressione :     *120*  significa che devo cercare la foto 120 con anche delle foto prima e dopo
			if( paramCercaFoto.numeriFotogrammi != null ) {

				int numPosiz = 0;
				int intorno = 0;

				// Vediamo se l'utente mi ha inserito sia il numero che la finestra
				Match m1 = regexNumIntorno1.Match( paramCercaFoto.numeriFotogrammi );
				if( m1.Success ) {
					numPosiz = int.Parse( m1.Groups[1].Value );
					intorno = int.Parse( m1.Groups[2].Value );
				} else { 
					Match m2 = regexNumIntorno2.Match( paramCercaFoto.numeriFotogrammi );
					if( m2.Success ) {
						numPosiz = int.Parse( m2.Groups[1].Value );
						intorno = QUANTE_FOTO_INTORNO;
					}
				}

				if( numPosiz > 0 ) {
					int dalNum = Math.Max( numPosiz - intorno, 1 );
					int alNum = numPosiz + intorno;

					paramCercaFoto.numeriFotogrammi = dalNum + "-" + alNum;
					paramCercaFoto.numeroConIntorno = numPosiz;
				}

			}
		}

		private void caricareSlideShow( string modo ) {

			// se per caso fosse chiuso, lo apro ed istanzio il ViewModel
			gestoreFinestrePubbliche.aprireFinestraSlideShow();

			if( modo == "AddSelez" )
				slideShowViewModel.add( creaListaFotoSelezionate() );
			else if( modo == "ZeroPiuSelez" ) {
				// Azzera e fa partire le selezionate
				slideShowViewModel.creaShow( creaListaFotoSelezionate() );
			} else if( modo == "Tutte" ) {

				completaParametriRicerca();

				ParamCercaFoto copiaParam = paramCercaFoto.deepCopy();

				// Nei parametri che mi passano, è indicata anche la paginazione. Nello ss non devo tenere conto della paginazione
				copiaParam.paginazione = null;

				slideShowViewModel.creaShow( copiaParam );
			} else {
				throw new ArgumentOutOfRangeException( "modo slide show" );
			}

			// L'azione di play deve essere automatica (ipse dixit)
			gestoreFinestrePubbliche.azioneAvvioSlideShow();

			OnPropertyChanged( "possoControllareSlideShow" );
		}

		private void controllareSlideShow( object param ) {

			if( param is string ) {

				string operaz = (string)param;

				switch( operaz.ToUpper() ) {

					case "START":
						gestoreFinestrePubbliche.azioneAvvioSlideShow();
						break;

					case "STOP":
						gestoreFinestrePubbliche.azioneFermaSlideShow();
						break;

					case "RESET":
						gestoreFinestrePubbliche.azioneResetSlideShow();
						break;
				}
			} else if( param is bool ) {

				// toggle button start/stop
				// il booleano indica la proprietà IsChecked del botton
				bool isChecked = (bool)param;

				if( isChecked )
					gestoreFinestrePubbliche.azioneFermaSlideShow();  // era acceso lo spengo
				else
					gestoreFinestrePubbliche.azioneAvvioSlideShow(); // era spento lo accendo
			}

			OnPropertyChanged( "isSlideShowRunning" );
		}

		public bool isSlideShowRunning { 
			get {
				return slideShowViewModel == null ? false : slideShowViewModel.isRunning;
            }
			set {
				// non faccio niente perché il command di toggle avrà già mosso lo stato
			}
		}

		private void playPauseSlideShow() {
			if( slideShowViewModel.isRunning )
				slideShowViewModel.stop();
			else if( slideShowViewModel.isPaused )
				slideShowViewModel.start();
		}

		/// <summary>
		/// Svuoto la collezione delle foto selezionate.
		/// Se non esiste la creo.
		/// </summary>
		void azzeraFotoSelez() {
			if( idsFotografieSelez == null )
				idsFotografieSelez = new ObservableCollectionEx<Guid>();
			else
				idsFotografieSelez.Clear();
			OnPropertyChanged( "countElementiSelezionati" );

			selezioneEstesa = new SelezioneEstesa();
        }

		private void setSelezioneEstesaLimiteA( Fotografia limiteA ) {
			selezioneEstesa.limiteA = limiteA;
		}

		private void azzeraParamRicerca() {
			azzeraParamRicerca( false );
			saveDataCambioSelezMode = null;

		}

		private void azzeraParamRicerca( bool tranneScarichi ) {
			paramCercaFoto = new ParamCercaFoto();

			OnPropertyChanged( "paramCercaFoto" );
			OnPropertyChanged( "stringaNumeriFotogrammi" );

			OnPropertyChanged( "isMattinoChecked" );
			OnPropertyChanged( "isPomeriggioChecked" );
			OnPropertyChanged( "isSeraChecked" );

			OnPropertyChanged( "isSoloVenduteChecked" );
			OnPropertyChanged( "isSoloInvenduteChecked" );

			// Spengo tutte le eventuali selezioni
			selettoreEventoViewModel.deselezionareTutto();
			selettoreFotografoViewModel.deselezionareTutto();
			if( !tranneScarichi )
				selettoreScaricoCardViewModel.deselezionareTutto();

		}

		/// <summary>
		/// Aggiungo alla lista delle foto da modificare, tutte le foto che sono illuminate
		/// </summary>
		void mandareInModifica() {

			// Pubblico un messaggio indicando che ci sono delle foto da modificare.
			FotoDaModificareMsg msg = new FotoDaModificareMsg( this );
			msg.fotosDaModificare.InsertRange( 0, creaListaFotoSelezionate() );

			// Per semplificare le operazioni lavoro ancora in modalità immediata altrimenti ci sono troppi tasti da premere.
			msg.immediata = true;

			LumenApplication.Instance.bus.Publish( msg );

			// Lascio selezionato
			// deselezionareTutto();
		
		}

		internal void mandareInModificaImmediata( Fotografia foto ) {
			// Pubblico un messaggio indicando che ci sono delle foto da modificare.
			FotoDaModificareMsg msg = new FotoDaModificareMsg( this );
			msg.immediata = true;
			msg.fotosDaModificare.Insert( 0, foto );
			LumenApplication.Instance.bus.Publish( msg );
		}

		/// <summary>
		/// Vado avanti o indietro di una (o più) pagine.
		/// </summary>
		/// <param name="delta"><value>+1 = una pagina avanti; -1 = una pagina indietro</value>
		/// </param>
		void spostarePaginazione( short deltaPag ) {

			int maxSkip = totFotoRicerca - 1;

			if( deltaPag == -999 )
				paramCercaFoto.paginazione.skip = 0;  // mi riposiziono sul primo
			else if( deltaPag == 999 ) {
				paramCercaFoto.paginazione.skip = totFotoRicerca - paginazioneRisultatiGallery;
            } else
				paramCercaFoto.paginazione.skip += ( deltaPag * paginazioneRisultatiGallery);
			
			// Non posso scendere sotto il minimo
			if( paramCercaFoto.paginazione.skip < 0 )
				paramCercaFoto.paginazione.skip = 0;



			// Non posso salire più del massimo
			if( paramCercaFoto.paginazione.skip > 0 ) {
				
				if( paramCercaFoto.paginazione.skip > maxSkip )
					paramCercaFoto.paginazione.skip = maxSkip;
			}

			eseguireRicerca( RicercaFlags.Niente );

			OnPropertyChanged( "percentualePosizRicerca" );
			OnPropertyChanged( "paginaAttualeRicerca" );
		}

		bool possoSpostarePaginazione( short delta ) {

			if( IsInDesignMode )
				return true;

			// Se gestisco la paginazione ed ho un risultato caricato.
			bool posso = true;

			if( posso && delta > 0 ) {
				// Voglio spostarmi avanti. Controllo di NON essere sull'ultimo risultato.
				posso = paramCercaFoto.paginazione.skip < (totFotoRicerca - paginazioneRisultatiGallery);
			}

			if( posso && delta < 0 ) {

				// Voglio spostarmi indietro. Controllo di avere sufficienti pagine precedenti.
				//				if( delta == -999 )  // Torno alla prima pagina
				posso = paramCercaFoto.paginazione.skip > 0;
				//				else
				//					posso = paramCercaFoto.paginazione.skip >= ( delta * paginazioneRisultatiGallery) > 0;
			}

			return posso;
		}

		/// <summary>
		/// Questa funzionalità prevede di mantenere gli attuali filtri di ricerca, ma occorre cercare 
		/// in quale pagina si trova il fotogramma desiderato.
		/// Quindi non va effettuata una nuova ricerca, ma occorre mantenere tutto (anche le foto selezionate devono rimanere)
		/// </summary>
		/// <param name="numFotogrammaDaric"></param>
		void spostarePaginazioneConNumFoto( int numFotogrammaDaric ) {

			IRicercatoreSrv ricercaSrv = LumenApplication.Instance.getServizioAvviato<IRicercatoreSrv>();
			
			int numeroSx = fotografieCW.SourceCollection.OfType<Fotografia>().First().numero;
			int numeroDx = fotografieCW.SourceCollection.OfType<Fotografia>().Last().numero;

			int min = Math.Min( numeroSx, numeroDx );
			int max = Math.Max( numeroSx, numeroDx );

			// Ricavo il numero di pagina su cui sono posionato adesso, ricavandolo dai parametri di ricerca
			int paginaAttuale = ((paramCercaFoto.paginazione.skip / paramCercaFoto.paginazione.take) + 1);
			int pagineTotali = totPagineRicerca;

			int paginaMin = 0;
			int paginaMax = 0;

			if( paramCercaFoto.ordinamento == Ordinamento.Asc ) {
				if( numFotogrammaDaric < min ) {
					// Sinistra
					paginaMax = (paginaAttuale - 1);
					paginaMin = 1;
				}
				if( numFotogrammaDaric > max ) {
					// Destra
					paginaMin = (paginaAttuale + 1);
					paginaMax = pagineTotali;
				}
			}

			if( paramCercaFoto.ordinamento == Ordinamento.Desc ) {
				if( numFotogrammaDaric < min ) {
					// Destra
					paginaMin = (paginaAttuale + 1);
					paginaMax = pagineTotali;
				}
				if( numFotogrammaDaric > max ) {
					// Sinistra
					paginaMax = (paginaAttuale - 1);
					paginaMin = 1;
				}
			}
	
			var pagina = ricercaSrv.ricercaPaginaDelFotogramma( numFotogrammaDaric, paginaMin, paginaMax, paramCercaFoto );

			if( pagina > 0 ) {
				// Ho trovato la pagina. eseguo lo spostamento
				this.paramCercaFoto.paginazione.skip = (int) ((pagina - 1) * this.paramCercaFoto.paginazione.take);
				eseguireRicerca( RicercaFlags.Niente );
			}

			System.Console.WriteLine( "nuova pagina = " + pagina );
		}

	

		bool possoSpostarePaginazioneConNumFoto( int numFotogramma ) {




			// Se non ho risultati attuali, esco
			if( !isAlmenoUnaFoto )
				return false;

			// Se non ho la possibilità di andare ne avanti ne indietro, significa che non ho altre pagine oltre questa.
			if( !possoSpostarePaginazione( -1 ) && !possoSpostarePaginazione( +1 ) )
				return false;

			// Ora devo fare dei ragionamenti più sofisiticati sul numero di fotogramma
			int numeroSx = fotografieCW.SourceCollection.OfType<Fotografia>().First().numero;
			int numeroDx = fotografieCW.SourceCollection.OfType<Fotografia>().Last().numero;

			int min = Math.Min( numeroSx, numeroDx );
			int max = Math.Max( numeroSx, numeroDx );

			// Controllo che il numero del fotogramma sia al di fuori della pagina attuale
			if( numFotogramma >= min && numFotogramma <= max )
				return false;


			// Passati tutti i controlli, significa che posso provare a spostarmi
			return true;
		}

		public short numRighe {
			get {
				return numRighePag;
			}
		}

		public short numColonne {
			get {
				return numColonnePag;
			}
		}

		private bool riportaOriginaleFotoSelezionate()
		{
			bool procediPure = true;

			if( countElementiSelezionati <= 0 )
				return false;

			if (countElementiSelezionati > 10)
			{
				procediPure = false;
				StringBuilder msg = new StringBuilder("Attenzione: stai per far tornare Originale più di 10 Fotografie!!!");
				dialogProvider.ShowConfirmation(msg.ToString(), "Richiesta conferma",
					(confermato) =>
					{
						procediPure = confermato;
					});
			}

			if (procediPure)
			{
				foreach( Fotografia foto in creaListaFotoSelezionate() )
					fotoRitoccoSrv.tornaOriginale( foto );

				// dialogProvider.ShowMessage("Operazione Terminata", "Info");
			}

			return true;
		}

		private Fotografia getFotoById( Guid guid ) {

			// Prima guardo se ce l'ho in pancia io
			Fotografia foto = null;
			if( fotografieCW != null && fotografieCW.Count > 0 ) {
				foto = ((IEnumerable<Fotografia>)fotografieCW.SourceCollection).SingleOrDefault( f => f.id == guid );
            }

			// Se non l'ho trovata in cache
			if( foto == null ) { 
				// Me la faccio ritornare dal servizio
				foto = fotoExplorerSrv.get( guid );
			}

			return foto;
		}

		private Fotografia ricavaFotoByNumber(int numDaric)
        {

            // Devo scorrere la lista
            Fotografia fotoTrovata = null;
            if (fotografieCW != null)
                foreach (var foto in fotografieCW.SourceCollection)
                {
                    if (((Fotografia)foto).numero == numDaric)
                    {
                        fotoTrovata = (Fotografia)foto;
                        break;
                    }
                }

            return fotoTrovata;
        }

		private void addSelezionata( Fotografia f ) {
			if( !idsFotografieSelez.Contains( f.id ) )
				idsFotografieSelez.Add( f.id );
		}

		private void removeSelezionata( Fotografia f ) {
			if( idsFotografieSelez.Contains( f.id ) )
				idsFotografieSelez.Remove( f.id );
		}

		private void ricreaCollectionViewFoto( List<Fotografia> fotos ) {

			// Non so se possa servire, ma prima di abbandonare al suo destino la vecchia collection view, libero i listener
			// Penso che questo possa dare una mano al Garbage Collector per recuperare la memoria
			if( fotografieCW != null ) {
				fotografieCW.SelectionChanged -= fotografie_selezioneCambiata;
			}

			// Creo la nuova collection view
			fotografieCW = new MultiSelectCollectionView<Fotografia>( fotos );

			// Associo ascoltatore di selezione cambiata
			fotografieCW.SelectionChanged += fotografie_selezioneCambiata;

			// notifico avvenuto cambiamento 
			// non ce ne è bisogno perché lo fa gia il set due righe sopra
			// OnPropertyChanged( "fotografieCW" );
		}


		Action _reidrataProviniAction;
		Action reidrataProviniAction {
			get {
				if( _reidrataProviniAction == null )
					_reidrataProviniAction = new Action( forseReidrataProvini );
				return _reidrataProviniAction;
			}
		}

		private void forseReidrataProvini() {

			foreach( Fotografia f in fotoExplorerSrv.fotografie ) {
				if( f != null )
					try {
						AiutanteFoto.idrataImmaginiFoto( f, IdrataTarget.Provino );
					} catch( Exception ) {
					}
			}

		}


		public IEnumerator<Fotografia> getEnumeratorElementiSelezionati() {
			return getElementiSelezionati().GetEnumerator();
		}

		public IEnumerator<Fotografia> getEnumeratorElementiTutti() {
			return getElementiTutti().GetEnumerator();
		}

		public IEnumerable<Fotografia> getElementiSelezionati() {
			return creaListaFotoSelezionate();
		}

		public IEnumerable<Fotografia> getElementiTutti() {
			return creaListaFotoTutte();
		}

		/// <summary> 
		/// Questaa inner-class mi serve per racchiudere tutte le info da salvare durante il cambio di modalità ModalitaFiltroSelez
		/// 
		/// </summary>
		private class SaveDataCambioSelezMode {

			public SaveDataCambioSelezMode( ParamCercaFoto param ) {
				this.paramCercaFoto = param;
			}

			public short numColonnePag { get; internal set; }
			public short numRighePag { get; internal set; }
			public ParamCercaFoto paramCercaFoto { get; set; }

		}

		/// <summary>
		/// Il limiteA lo setto dinamicamente ad ogni click del mouse con il metodo fotografie_selezioneCambiata
		/// </summary>
		/// <param name="fotoLimiteB">Se NON specificato, allora uso la selezioneEstesa corrente così come sta</param>
		public void eseguireSelezioneEstesa( Fotografia fotoLimiteB = null ) {

			// Spengo l'ascoltatore della selezione cambiata perché senno lo faccio impazzire
			fotografieCW.SelectionChanged -= fotografie_selezioneCambiata;

			try {

				if( fotoLimiteB != null )
					selezioneEstesa.limiteB = fotoLimiteB;

				if( ! selezioneEstesa.isCompleta )
					return;

				// Per prima cosa, controllo se i limiti della selezione estesa sono già presenti nella pagina attuale.
				// In questo caso, non sto a rieseguire la query, ma la risolvo velocemente in memoria
				var fotos = fotografieCW.SourceCollection.OfType<Fotografia>();
				if( fotos.Any( f => f.Equals( selezioneEstesa.limiteA ) ) && fotos.Any( f => f.Equals( selezioneEstesa.limiteB ) ) ) {

					// ok entrambi i limiti sono presenti prendo i numeri
					bool iniziato = false;

					foreach( var foto in fotos ) {

						bool trovata = selezioneEstesa.contains( foto );

						if( iniziato || trovata )
							selezionareSingola( foto, false, false );

						if( trovata ) {
							if( iniziato || selezioneEstesa.isSingola )
								break;
							else
								iniziato = true;
						}

					}

					// Se non eseguo questo refresh, non mi viene illuminato nulla nella maschera.
					if( iniziato ) {
						fotografieCW.RefreshSelectedItemWithMemory();
						notificaSelezioneCambiata();
					}

				} else {

					// Se i due limiti non sono nella stessa pagina corrente, allora le cose si complicano
					// perché per sapere quante e quali foto sono "comprese" nei limiti devo andare sul database.
					eseguireSelezioneEstesaConRicercaSulDb();

				}
			} finally {
				// Riaccendo l'ascoltatore della selezione cambiata che avevo spento all'inizio del metodo
				fotografieCW.SelectionChanged += fotografie_selezioneCambiata;
			}
		}

		/// <summary>
		/// A parità di filtri di ricerca correnti,
		/// devo cercare sul db quali sono le foto comprese tra i due estremi indicati
		/// </summary>
		void eseguireSelezioneEstesaConRicercaSulDb() {

			// Per prima cosa mi clono i parametri perchè devo mantenere quasi tutti i filtri esistenti
			ParamCercaFoto param2 = paramCercaFoto.deepCopy();

			// elimino la paginazione perché mi servono tutte le foto
			param2.paginazione = null;
			
			// Aggiungo però solo il range di numeri specificato
			String range = String.Format( "{0}-{1}", selezioneEstesa.numeroMinore, selezioneEstesa.numeroMaggiore );
			param2.numeriFotogrammi = range;
			// TODO
			// purtroppo questo sistema ha un piccolo bug.
			// Se uno dei numeri estremi ha dei doppi, vengono presi su per forza !!!!


			var ricercatoreSrv = LumenApplication.Instance.getServizioAvviato<IRicercatoreSrv>();
			var fotografieDaSelez = ricercatoreSrv.cerca( param2 );

			selezionareMolte( fotografieDaSelez );

		}

		private bool possoAprireCercaFotoPopup( ModoRicercaPop modoRicercaPop ) {

			// Lo spostamento di pagina sui risultanti correnti, è abilitato solo se ho delle foto nella gallery.
			if( modoRicercaPop == ModoRicercaPop.PosizionaPaginaDaNumero && totFotoRicerca <= 0 )
				return false;

			return true;
		}

		void aprireCercaFotoPopup( ModoRicercaPop modoRicercaPop ) {

			cercaFotoPopupViewModel = new CercaFotoPopupViewModel();

			cercaFotoPopupViewModel.modoRicercaPop = modoRicercaPop;

			// Non dovrebbe mai accadere, ma se mi arrivasse questa condizione non è usabile. La correggo
			// Se ho una ricerca attiva, allora permetto di ricercare per numero fotogramma.
			if( modoRicercaPop == ModoRicercaPop.PosizionaPaginaDaNumero && totFotoRicerca <= 0 ) {
				// Ricerca impossibile. la correggo
				_giornale.Warn( "Ricerca impossibile. La correggo" );
				cercaFotoPopupViewModel.modoRicercaPop = ModoRicercaPop.PosizionaPaginaDaNumero;
			}

			// Lo spostamento su pagine viene attivato solo se ho almeno due pagine di risultati
			cercaFotoPopupViewModel.possoRicercareLaPagina = (totPagineRicerca > 1);

			// Se ho qualche ascoltatore, lo invoco
			CercaFotoPopupRequestEventArgs args = new CercaFotoPopupRequestEventArgs();
			RaisePopupDialogRequest( args );

				if( cercaFotoPopupViewModel.confermata ) {

				// Se confermata la popup, agisco
				using( new UnitOfWorkScope() ) {

					int numeroFotogramma = cercaFotoPopupViewModel.numeroFotogramma;
					Nullable<DateTime> giornata = null;
					

					if( cercaFotoPopupViewModel.modoRicercaPop == ModoRicercaPop.PosizionaPaginaDaNumero ) {

						// Spostamento sulla pagina in cui si trova il fotogramma scelto
						if( spostarePaginazioneConNumFotoCommand.CanExecute( numeroFotogramma ) )
							spostarePaginazioneConNumFotoCommand.Execute( numeroFotogramma );
					}

					bool ricercareNumConIntorno = (cercaFotoPopupViewModel.modoRicercaPop == ModoRicercaPop.RicercaNumeroConIntorno);

					if( cercaFotoPopupViewModel.modoRicercaPop == ModoRicercaPop.RicercaDidascaliaConIntorno ) {

						// Cerco la prima foto con quella diascalia per scoprire la giornata
						azzeraParamRicerca();

						// Il barcode è un ean8 quindi formatto il numero da 8 con zeri davanti
						paramCercaFoto.didascalia = String.Format( "{0:00000000}", cercaFotoPopupViewModel.numeroFotogramma );

						paramCercaFoto.idratareImmagini = false;
						paramCercaFoto.paginazione.take = 1;
						paramCercaFoto.ordinamento = Ordinamento.Desc; // voglio per prima la foto più recente.

						fotoExplorerSrv.cercaFoto( paramCercaFoto );

						if( fotoExplorerSrv.fotografie.Count > 0 ) {
							// ora posso cascare nel caso della ricerca per numero, già implementata sotto.
							numeroFotogramma = fotoExplorerSrv.fotografie[0].numero;
							// memorizzo la giornata perché la ricerca dicotomica andrà solo per quel giorno.
							giornata = fotoExplorerSrv.fotografie[0].giornata;
							ricercareNumConIntorno = true;
						} else {
							System.Media.SystemSounds.Beep.Play();
						}
					}


					if( ricercareNumConIntorno ) {

						// Cerco la prima foto con quella diascalia per scoprire la giornata
	

						if( giornata == null ) {

							// Cerco la foto con il numero perché devo ricavare la giornata
							azzeraParamRicerca();
							paramCercaFoto.numeriFotogrammi = numeroFotogramma.ToString();
							paramCercaFoto.idratareImmagini = false;
							paramCercaFoto.paginazione.take = 1;
							paramCercaFoto.ordinamento = Ordinamento.Desc; // voglio per prima la foto più recente.

							fotoExplorerSrv.cercaFoto( paramCercaFoto );

							if( fotoExplorerSrv.fotografie.Count > 0 ) {
								// memorizzo la giornata perché la ricerca dicotomica andrà solo per quel giorno.
								giornata = fotoExplorerSrv.fotografie[0].giornata;
							} else
								throw new LumenException( "ricerca numerica fallita. errore interno" );
						}

						azzeraParamRicerca();
						paramCercaFoto.numeriFotogrammi = String.Format( "*{0}*", numeroFotogramma );
						paramCercaFoto.giornataIniz = giornata;
						paramCercaFoto.giornataFine = giornata;
						eseguireRicerca( RicercaFlags.NuovaRicerca );

						OnPropertyChanged( "paramCercaFoto" );
						OnPropertyChanged( "stringaNumeriFotogrammi" );
					}

					// Per ultimo controllo se ho una impronta digitale
					if( cercaFotoPopupViewModel.identificatoreImprontaViewModel != null &&
						cercaFotoPopupViewModel.identificatoreImprontaViewModel.nomeIdentificato != null ) {
						paramCercaFoto.didascalia = cercaFotoPopupViewModel.identificatoreImprontaViewModel.nomeIdentificato;
					}

				}


			} else {
				// non confermata
				if( cercaFotoPopupViewModel.filtroDidascalia != null ) {
					if( cercaFotoPopupViewModel.filtroDidascalia == FiltroDidascalia.SoloPiene )
						paramCercaFoto.didascalia = "(PIENA)";
					if( cercaFotoPopupViewModel.filtroDidascalia == FiltroDidascalia.SoloVuote )
						paramCercaFoto.didascalia = "(VUOTA)";
					if( cercaFotoPopupViewModel.filtroDidascalia == FiltroDidascalia.Impronta )
						paramCercaFoto.didascalia = cercaFotoPopupViewModel.identificatoreImprontaViewModel.nomeIdentificato;
					OnPropertyChanged( "paramCercaFoto" );
				}
			}


			// Chiudo la popup
			cercaFotoPopupViewModel.CloseCommand.Execute( null );
		}

		/// <summary>
		/// Mantenendo i parametri di ricerca attuali,
		/// dato un numero di pagina in input, ritorno il numero del primo fotogramma di quella pagina.
		/// </summary>
		/// <param name="numPagina">int 0 se non ho trovato niente, altrimenti il numero del fotogramma</param>
		/// <returns></returns>
		public int getPrimoFotogrammaPagina( int numPagina ) {

			// Clono i parametri per non sporcare quelli attuali
			ParamCercaFoto newParam = paramCercaFoto.deepCopy();
			newParam.paginazione.skip = (numPagina - 1) * paginazioneRisultatiGallery;
			newParam.paginazione.take = 1;
			newParam.idratareImmagini = false;

			// Eseguo la ricerca nel database
			IRicercatoreSrv ricercaSrv = LumenApplication.Instance.getServizioAvviato<IRicercatoreSrv>();
			var fotografie = ricercaSrv.cerca( newParam );

			int numFotogramma = 0;
			if( fotografie != null && fotografie.Count > 0 ) {
				numFotogramma = fotografie.First().numero;
			}

			return numFotogramma;
		}


		/// <summary>
		/// Quando muovo lo slider ho bisogno di eseguire delle query sul db
		/// e quindi mi serve una UnitOfWork tutta mia.
		/// </summary>
		UnitOfWorkScope _unitOfWorkSlider = null;

		public void startUnitOkWork() {
			_unitOfWorkSlider = new UnitOfWorkScope();
		}
		public void stopUnitOkWork() {
			if( _unitOfWorkSlider != null ) {
				_unitOfWorkSlider.Dispose();
				_unitOfWorkSlider = null;
			}
		}

		void pauseRunStampanti( bool pause ) {

			ISpoolStampeSrv srv = LumenApplication.Instance.getServizioAvviato<ISpoolStampeSrv>();
			if( pause )
				srv.pauseTutteLeStampanti();
			else
				srv.resumeTutteLeStampanti();
		}

#endregion Metodi


#region MemBus

		public void OnCompleted()
		{
			throw new NotImplementedException();
		}

		public void OnError(Exception error)
		{
			throw new NotImplementedException();
		}
		
		public void OnNext( RefreshMsg msg ) {

			Application.Current.Dispatcher.BeginInvoke( reidrataProviniAction );

			OnPropertyChanged( "isPossibileModificareCarrello" );
		}

		public void OnNext( FotoModificateMsg fmMsg ) {

			DateTime prima = DateTime.Now;
		
			// In un mondo bello ,
			// quando vado a rileggere i provini, dovrei farlo nel thread della UI per poter vedere i cambiamenti
			// Purtroppo però il binding non avviene sull'attributo "imgProvino" che cambia, ma il binding è sulla intera Fotografia
			// che però non cambia. 
			// Quindi non seve. Sarò costretto a fare il refresh della CW
//			Application.Current.Dispatcher.BeginInvoke(
//				new Action( () => {
					rinfrescaFotoModificate( fmMsg.fotos );
//							}
//						) );


			TimeSpan elapsed = DateTime.Now.Subtract( prima );
			_giornale.Debug( "rinfresco foto modificate gallery tempo durata ms = " + elapsed.TotalMilliseconds );
		}

		private void rinfrescaFotoModificate( List<Fotografia> fotos ) {
			
			bool almenoUna = false;


			foreach( Fotografia modificata in fotos ) {

				// List<Fotografia> lista = (List < Fotografia > )fotografieCW.SourceCollection;


				int pos = fotografieCW.IndexOf( modificata );
				if( pos >= 0 ) {
					almenoUna = true;

					// Estraggo l'oggetto
					Fotografia f = (Fotografia)fotografieCW.GetItemAt( pos );

					// TODO
					// Per evitare di chiamare il metodo refresh che è molto lento, tolgo e rimetto l'elemento dalla collezione
					// lista.RemoveAt( pos );
					// fotografieCW.RemoveAt( pos );

					AiutanteFoto.disposeImmagini( f, IdrataTarget.Provino );

					// Se la foto è stata modificata, allora mi copio le correzioni.
					f.correzioniXml = modificata.correzioniXml;
					// Se ho a  disposizione l'immagine del provino, me la copio, altrimenti me la rileggo da disco.
					if( modificata.imgProvino != null )
						f.imgProvino = (Digiphoto.Lumen.Imaging.IImmagine)modificata.imgProvino;
					else
						AiutanteFoto.idrataImmaginiFoto( f, IdrataTarget.Provino, true );


					// Controllo se per caso sto lavorando in alta qualita allora devo aggiornare anche la foto grande
					if( GrigliaImageConverter.richiedeAltaQualita( numRighePag, numColonnePag ) ) {

						AiutanteFoto.disposeImmagini( f, IdrataTarget.Risultante );

						// Se ho a  disposizione l'immagine del provino, me la copio, altrimenti me la rileggo da disco.
						if( modificata.imgRisultante != null )
							f.imgProvino = (Digiphoto.Lumen.Imaging.IImmagine)modificata.imgRisultante;
						else
							AiutanteFoto.idrataImmagineDaStampare( f );
					}


				}
			}

			if( almenoUna ) {
				// Sono costretto a fare il refresh perché il ProperyChange su imgProvino non sortisce effetto alcuno.
				// Infatti il binding è sulla intera Fotografia che non cambia. (questo è dovuto al fatto che dobbiamo scegliere il provino o la risultante tramite un converter.
				fotografieCW.Refresh();
				// OnPropertyChanged( "fotografieCW" );
			}

		}


		public void OnNext(ClonaFotoMsg value)
		{
			if( value.fase == FaseClone.FineClone ) {

				eseguireRicerca( RicercaFlags.Niente );

#if POLLO
				Digiphoto.Lumen.UI.App.Current.Dispatcher.BeginInvoke(
					new Action( () => {

						// Siccome lavoro nel thread della ui, probabilmente non ho la UoW
						if( UnitOfWorkScope.hasCurrent )
							eseguireRicerca( RicercaFlags.Niente );
						else {
							using( new UnitOfWorkScope() ) {
								eseguireRicerca( RicercaFlags.Niente );
							}
						}
					}
				) );
#endif
			}
		}

		public void OnNext( FotoEliminateMsg msg ) {

			if( this.fotografieCW != null ) {

				// Conto se ci sono delle foto a video che sono state eliminate
				// In tal caso rilancio la ricerca (per farle sparire)
				Fotografia[] fotos = new Fotografia[this.fotografieCW.Count];
				int ii = 0;
				foreach( var foto in fotografieCW.SourceCollection )
					fotos[ii++] = (Fotografia)foto;

				int quanteFotoSonoVisualizzate = fotos.Intersect( msg.listaFotoEliminate ).Count();
				if( quanteFotoSonoVisualizzate > 0 )
					eseguireRicerca( RicercaFlags.Niente );
			}
		}

		public void OnNext(NuovaFotoMsg value)
        {
            Fotografia fotoOrig = ricavaFotoByNumber(value.foto.numero);
            // ricreo la collection-view e notifico che è cambiato il risultato. Le immagini verranno caricate poi

            IList<Fotografia> fotografie = fotografieCW.SourceCollection as IList<Fotografia>;
            List<Fotografia> fotos = fotografie.ToList();
            int index = fotos.IndexOf(fotoOrig) + 1;
            fotos.Insert(index, value.foto);
			ricreaCollectionViewFoto( fotos );

			// TODO ma non basterebbe aggiungere alla CW ???? provare
        }

		public void OnNext( CambioStatoMsg csm ) {

			if( csm.sender is SlideShowViewModel )
				OnPropertyChanged( "isSlideShowRunning" );
		}

		public void OnNext( GestoreCarrelloMsg value ) {

			if( value.fase == GestoreCarrelloMsg.Fase.CreatoNuovoCarrello ||
				value.fase == GestoreCarrelloMsg.Fase.LoadCarrelloSalvato ) {

				Application.Current.Dispatcher.BeginInvoke( reidrataProviniAction );

				OnPropertyChanged( "modoVendita" );
				OnPropertyChanged( "isPossibileModificareCarrello" );
			}

			if( value.fase == GestoreCarrelloMsg.Fase.VisualizzareInGallery ) {

				azzeraParamRicerca();

				IEnumerable<Guid> tantiIds = fotoExplorerSrv.caricaFotoDalCarrello();
				paramCercaFoto.idsFotografie = tantiIds.ToArray();

				paramCercaFoto.evitareJoinEvento = true;

				completaParametriRicerca();  // Mi serve true per eseguire anche la count dei risultati

				eseguireRicerca( RicercaFlags.NuovaRicerca | RicercaFlags.MantenereListaIds );
			}
		}


		public void OnNext( StampatoMsg msg ) {

			if( msg.esito == Esito.Ok && msg.lavoroDiStampa is LavoroDiStampaFoto ) {

				Fotografia fotoStampata = ((LavoroDiStampaFoto)msg.lavoroDiStampa).fotografia;
				int pos = fotografieCW.IndexOf( fotoStampata );
				if( pos >= 0 ) { // Ok la foto è visibile
					Fotografia fotoGallery = (Fotografia)fotografieCW.GetItemAt( pos );
					fotoGallery.contaStampata += ((LavoroDiStampaFoto)msg.lavoroDiStampa).param.numCopie;
				}
			}
		}

#endregion MemBus



	}


}
