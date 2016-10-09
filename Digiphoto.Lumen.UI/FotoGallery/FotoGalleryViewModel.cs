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

namespace Digiphoto.Lumen.UI {



	public class FotoGalleryViewModel : ViewModelBase, IContenitoreGriglia, ISelettore<Fotografia>,
	                                    IObserver<FotoModificateMsg>, IObserver<NuovaFotoMsg>, IObserver<ClonaFotoMsg>, IObserver<GestoreCarrelloMsg>, IObserver<RefreshMsg>, IObserver<CambioStatoMsg>

	{

		#region Campi

		private BackgroundWorker _bkgIdrata;

		#endregion

		public FotoGalleryViewModel() {

			// Devo ascoltare tutti i messaggi applicativi che girano
			IObservable<FotoModificateMsg> observableFM = LumenApplication.Instance.bus.Observe<FotoModificateMsg>();
			observableFM.Subscribe( this );

			IObservable<ClonaFotoMsg> observableClonaFoto = LumenApplication.Instance.bus.Observe<ClonaFotoMsg>();
			observableClonaFoto.Subscribe( this );

			IObservable<NuovaFotoMsg> observableNuovaFoto = LumenApplication.Instance.bus.Observe<NuovaFotoMsg>();
			observableNuovaFoto.Subscribe( this );

			IObservable<GestoreCarrelloMsg> observableGesCarrello = LumenApplication.Instance.bus.Observe<GestoreCarrelloMsg>();
			observableGesCarrello.Subscribe( this );

			IObservable<RefreshMsg> observableRefresh = LumenApplication.Instance.bus.Observe<RefreshMsg>();
			observableRefresh.Subscribe( this );

			IObservable<CambioStatoMsg> observableCambioStato = LumenApplication.Instance.bus.Observe<CambioStatoMsg>();
			observableCambioStato.Subscribe( this );


			// Istanzio i ViewModel dei componenti di cui io sono composto
			selettoreEventoViewModel = new SelettoreEventoViewModel();
			selettoreScaricoCardViewModel = new SelettoreScaricoCardViewModel();
			selettoreScaricoCardViewModel.scarichiCardsCW.SelectionChanged += scarichiCardsCW_SelectionChanged;

			selettoreEventoMetadato = new SelettoreEventoViewModel();
			selettoreFotografoViewModel = new SelettoreFotografoViewModel();

			selettoreAzioniRapideViewModel = new SelettoreAzioniRapideViewModel( this );
			selettoreAzioniRapideViewModel.gestitaSelezioneMultipla = true;

			selettoreMetadatiViewModel = new SelettoreMetadatiViewModel( this );


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
			cambiarePaginazione( 2 );
        }



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

					_numRighePag = value;
					OnPropertyChanged( "numRighePag" );
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

					_numColonnePag = value;
					OnPropertyChanged( "numColonnePag" );
				}
			}
		}


		/// <summary>
		/// Prima era in configurazione, ora invece lo decide l'utente al volo.
		/// </summary>
        public int paginazioneRisultatiGallery {
			get {
				return numRighePag * numColonnePag;
			}
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


		public SelettoreAzioniRapideViewModel selettoreAzioniRapideViewModel
		{
			get;
			set;
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

		public float ratioAreaStampabile {
			get {
				ISpoolStampeSrv srv = LumenApplication.Instance.getServizioAvviato<ISpoolStampeSrv>();
				return srv == null ? 0f : srv.ratioAreaStampabile;
			}
		}

		int _idrataProgress;
		public int idrataProgress {
			get {
				return _idrataProgress;
			}
			private set {
				if( _idrataProgress != value ) {
					_idrataProgress = value;
					OnPropertyChanged( "idrataProgress" );
				}
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
		/// Se imprimo l'area di rispetto sul jpg del provino,
		/// allora il bottone per visualizzare i controlli grafici sopra la foto, lo tengo spento.
		/// </summary>
		public bool possoVisualizzareAreaDiRispetto {
			get {
				return ! Configurazione.UserConfigLumen.imprimereAreaDiRispetto;
			}
		}

		#endregion Proprietà



		#region Comandi

		private RelayCommand _cambiarePaginazioneCommand;
		public ICommand cambiarePaginazioneCommand {
			get {
				if( _cambiarePaginazioneCommand == null ) {
					_cambiarePaginazioneCommand = new RelayCommand( stelline => cambiarePaginazione( Convert.ToInt16( stelline ) ),
																    stelline => true );
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

		private RelayCommand _caricareSlideShowCommand;
		public ICommand caricareSlideShowCommand {
			get {
				if( _caricareSlideShowCommand == null ) {
					_caricareSlideShowCommand =
						new RelayCommand( autoManual => caricareSlideShow( (string)autoManual ),
										  autoManual => possoCaricareSlideShow( (string)autoManual ),
                                          null,
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

		private RelayCommand _riportaOriginaleFotoSelezionateCommand;

		public event SelezioneCambiataEventHandler selezioneCambiata;

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

		#endregion


		#region Metodi

		public void setModalitaSingolaFoto( Fotografia foto ) {

			// Ho cliccato con il destro sulla singola foto.
			// Memorizzo la foto per eventuali operazioni da lanciare
			if( selettoreAzioniRapideViewModel.setSingolaFotoWorkCommand.CanExecute( foto ) )
				selettoreAzioniRapideViewModel.setSingolaFotoWorkCommand.Execute( foto );
			else
				dialogProvider.ShowError( "comando non utilizzabile", "setSingolaFotoWorkCommand", null );
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

		private void cambiarePaginazione( short stelline ) {

			int idx = stelline - 1;
			numRighePag = Configurazione.UserConfigLumen.prefGalleryViste[idx].numRighe;
			numColonnePag = Configurazione.UserConfigLumen.prefGalleryViste[idx].numColonne;
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

			eseguireRicerca( true );

			// Ricarico la collezione delle selezionate che durante la ricerca viene spenta
			idsFotografieSelez = new ObservableCollectionEx<Guid>( paramCercaFoto.idsFotografie );

			selezionareElementiPaginaCorrente();
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

			eseguireRicerca( true, false );

			// Ricarico la lista dei selezionati che nel frattempo si è svuotata
			idsFotografieSelez = new ObservableCollectionEx<Guid>( saveIds );

			selezionareElementiPaginaCorrente();

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
				AiutanteFoto.idrataImmaginiFoto( f, IdrataTarget.Provino );
				lista.Add( f );
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

		public void deselezionareTutto() {
			accendiSpegniTutto( false );
		}
		 
		public void deselezionare( Fotografia foto ) {

			if( idsFotografieSelez != null && idsFotografieSelez.Contains( foto.id ) )
				idsFotografieSelez.Remove( foto.id );

			// Poi lavoro sulla collezione visuale della pagina
			if( fotografieCW != null && fotografieCW.SelectedItems.Contains( foto ) )
				fotografieCW.SelectedItems.Remove( foto );

		}

		private void selezionareTutto() {
			accendiSpegniTutto( true );
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
			raiseSelezioneCambiataEvent();
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
						venditoreSrv.modoVendita = ModoVendita.StampaDiretta;
						venditoreStampaDiretta.creareNuovoCarrello();
						venditoreStampaDiretta.carrello.intestazione = VenditoreSrvImpl.INTESTAZIONE_STAMPA_RAPIDA;
						venditoreStampaDiretta.aggiungereStampe(listaSelez, creaParamStampaFoto(stampanteAbbinata));
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
			eseguireRicerca( true );
		}

		private void eseguireRicerca( bool nuovaRicerca ) {

			bool chiediConfermaSeFiltriVuoti = (nuovaRicerca == true && modalitaFiltroSelez == ModalitaFiltroSelez.Tutte);
			eseguireRicerca( nuovaRicerca, chiediConfermaSeFiltriVuoti );
		}

		/// <summary>
		/// Eseguo una ricerca sul database. Quindi aggiorno la UI
		/// </summary>
		/// <param name="nuovaRicerca">Se true significa che ho premuto il tasto RICERCA e quindi devo ripartire da capo.
		/// Se invece è FALSE significa che sto paginando su di una ricerca già effettuata in precedenza
		/// </param>
		private void eseguireRicerca( bool nuovaRicerca, bool chiediConfermaSeFiltriVuoti ) {


			idrataProgress = 0;


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

				azzeraFotoSelez();

				// Queste sono le foto selezionate
				if( modalitaFiltroSelez == ModalitaFiltroSelez.Tutte )
					paramCercaFoto.idsFotografie = null;

				contaTotFotoRicerca();
			}


			// Eseguo la ricerca nel database
			fotoExplorerSrv.cercaFoto( paramCercaFoto );

			azioniPostRicerca( nuovaRicerca );

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
		private void azioniPostRicerca( bool nuovaRicerca ) {

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

			// spengo tutte le selezioni eventualmente rimaste da prima
			if( nuovaRicerca )
				deselezionareTutto();
			else {
				selezionareElementiPaginaCorrente();
			}
				

			// Se non ho trovato nulla, allora avviso l'utente
			if( fotografieCW.Count <= 0 )
				dialogProvider.ShowMessage( "Nessuna fotografia trovata con questi filtri di ricerca", "AVVISO" );

			OnPropertyChanged( "isAlmenoUnaFoto" );
			OnPropertyChanged( "fotoAttualeRicerca" );
			OnPropertyChanged( "percentualePosizRicerca" );

			raisePropertyChangeSelezionate();

			// Ora ci penso io ad idratare le immagini, perchè devo fare questa operazione nello stesso thread della UI
			if( !_bkgIdrata.IsBusy )
				_bkgIdrata.RunWorkerAsync();
			// Lasciare come ultima cosa l'idratazione delle foto.

		}

		private void selezionareElementiPaginaCorrente() {

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
		}

		private bool verificaChiediConfermaRicercaSenzaParametri()
		{
			bool procediPure = true;
			
			if (!paramCercaFoto.isEmpty())
				return procediPure;

			procediPure = false;
			StringBuilder msg = new StringBuilder("Attenzione: stai eseguendo una ricerca senza parametri.\nConfermi?");
			dialogProvider.ShowConfirmation(msg.ToString(), "Richiesta conferma",
				(confermato) =>
				{
					procediPure = confermato;
				});

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
			foreach( var fa in e.AddedItems ) {
				addSelezionata( fa as Fotografia );
			}
						
			// Eventuali rimozioni
			foreach( var fr in e.RemovedItems )
				removeSelezionata( fr as Fotografia );

			raiseSelezioneCambiataEvent();

			raisePropertyChangeSelezionate();
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


		public void calcolaFotoCorrenteSelezionataScorrimento( int direzione ) {

throw new NotImplementedException( "TODO da rivedere");

			int posiz = -1;
			if( fotoCorrenteSelezionataScorrimento != null )
				posiz = fotografieCW.SelectedItems.IndexOf( fotoCorrenteSelezionataScorrimento );

			if( posiz < 0 )
				posiz = fotografieCW.SelectedItems.IndexOf( fotoCorrente );
			
			
			// Ho già un valore memorizzato. Vediamo se ho un elenco su cui iterare.
			if( posiz >= 0 ) {
				posiz += direzione;
				if( posiz < 0 )
					posiz = fotografieCW.SelectedItems.Count - 1;  // fondo
				else if( posiz >= fotografieCW.SelectedItems.Count )
					posiz = 0;
			}

			if( posiz >= 0 ) {
				fotoCorrenteSelezionataScorrimento = fotografieCW.SelectedItems.ElementAt( posiz );
			} else
				fotoCorrenteSelezionataScorrimento = null;  // non ho rimediato nulla.

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
			int percPrec = 0;
			worker.ReportProgress( percPrec );


			for( int ii = 0; (ii < tot); ii++ ) {
				if( (worker.CancellationPending == true) ) {
					e.Cancel = true;
					break;
				} else {

					Fotografia foto = (Fotografia) fotografieCW.GetItemAt( ii );

					try {
						// Perform a time consuming operation and report progress.
						AiutanteFoto.idrataImmaginiFoto( foto, IdrataTarget.Provino );
					} catch( Exception ) {

						// Provo a crearlo. Magari non c'è perché è stato cancellato per sbaglio, oppure c'è ma si è corrotto.
						try {

							App.Current.Dispatcher.BeginInvoke(
								new Action( () => {

									try {
										AiutanteFoto.creaProvinoFoto( foto );
									} catch( Exception ) {

										_giornale.Debug( "Problemi nel provinare la foto: " + foto );

										// Se qualcosa va male, pazienza, a questo punto non posso fare altro che tirare avanti.
										// TODO : forse dovrei togliere la foto in esame dalla collezione della gallery ....
									}
								}
							) );

						} catch( Exception ) {
							// Se qualcosa va male, pazienza, a questo punto non posso fare altro che tirare avanti.
						}
					}

					// Aggiorno la percentuale di progressi di idratazione. Esiste una ProgressBar che si abilita all'uopo.
					int perc = (ii + 1) * 100 / tot;
					if( percPrec != perc ) {
						worker.ReportProgress( perc );
						percPrec = perc;
					}
				}
			}

			// Se sono arrivato in fondo, comunico il progresso massimo giusto per sicurezza.
			if( !e.Cancel )
				worker.ReportProgress( 100 );
			
		}

		void bkgIdrata_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e ) {

			_giornale.Debug( "Terminato di idratare le foto in background: abortito = " + e.Cancelled );

			if( ! e.Cancelled ) {
				RicercaModificataMessaggio msg = new RicercaModificataMessaggio( this );
				msg.abortito = e.Cancelled;
				LumenApplication.Instance.bus.Publish( msg );
			}

		}

		void bkgIdrata_ProgressChanged( object sender, ProgressChangedEventArgs e ) {
			idrataProgress = e.ProgressPercentage;
		}

		private void completaParametriRicerca()
		{
			completaParametriRicercaWithOrder( false );
		}

		/// <summary>
		/// Sistemo i parametri e gestisco la paginazione
		/// </summary>
		/// <param name="ordinamento asc/desc">il numero di pagina</param>
		private void completaParametriRicercaWithOrder( bool usaOrdinamentoAsc )
		{

			paramCercaFoto.idratareImmagini = false;

			if(usaOrdinamentoAsc) {
				if( Configurazione.UserConfigLumen.invertiRicerca )
					paramCercaFoto.ordinamento = Ordinamento.Asc;
				else
					paramCercaFoto.ordinamento = Ordinamento.Desc;
			}

			// Aggiungo eventuale parametro il fotografo
			if( selettoreFotografoViewModel.fotografoSelezionato != null )
				paramCercaFoto.fotografi = new Fotografo [] { selettoreFotografoViewModel.fotografoSelezionato };
			else
				paramCercaFoto.fotografi = null;

			// Aggiungo eventuale parametro l'evento
			if( selettoreEventoViewModel.eventoSelezionato != null )
				paramCercaFoto.eventi = new Evento [] { selettoreEventoViewModel.eventoSelezionato };
			else
				paramCercaFoto.eventi = null;

			// Aggiungo eventuale parametro lo scarico card.
			if( selettoreScaricoCardViewModel.scarichiCardsCW != null && selettoreScaricoCardViewModel.scarichiCardsCW.SelectedItems.Count > 0 )
				paramCercaFoto.scarichiCard = selettoreScaricoCardViewModel.scarichiCardsCW.SelectedItems.ToArray();
			else
				paramCercaFoto.scarichiCard = null;

			// Ampiezza finestra di paginazione
			paramCercaFoto.paginazione.take = paginazioneRisultatiGallery;
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

			deselezionareTutto();
		
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
				paramCercaFoto.paginazione.skip += (deltaPag * paginazioneRisultatiGallery);
			
			// Non posso scendere sotto il minimo
			if( paramCercaFoto.paginazione.skip < 0 )
				paramCercaFoto.paginazione.skip = 0;



			// Non posso salire più del massimo
			if( paramCercaFoto.paginazione.skip > 0 ) {
				
				if( paramCercaFoto.paginazione.skip > maxSkip )
					paramCercaFoto.paginazione.skip = maxSkip;
			}

			eseguireRicerca( false );

			OnPropertyChanged( "percentualePosizRicerca" );
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

			forseReidrataProvini();

			OnPropertyChanged( "isPossibileModificareCarrello" );
		}

		public void OnNext( FotoModificateMsg fmMsg ) {

			bool almenoUna = false;

			foreach( Fotografia modificata in fmMsg.fotos ) {

				int pos = fotografieCW.IndexOf( modificata );
				if( pos >= 0 ) {
					almenoUna = true;
					Fotografia f = (Fotografia)fotografieCW.GetItemAt( pos );
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

			if( almenoUna )
				fotografieCW.Refresh();
		}

		public void OnNext(ClonaFotoMsg value)
		{
			if (value.fase == FaseClone.FineClone)
			{
				eseguireRicerca( false );
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
					eseguireRicerca( false );
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

				forseReidrataProvini();

				OnPropertyChanged( "modoVendita" );
				OnPropertyChanged( "isPossibileModificareCarrello" );
			}

			if( value.fase == GestoreCarrelloMsg.Fase.VisualizzareInGallery ) {

				azzeraParamRicerca();

				IEnumerable<Guid> tantiIds = fotoExplorerSrv.caricaFotoDalCarrello();
				paramCercaFoto.idsFotografie = tantiIds.ToArray();

				paramCercaFoto.evitareJoinEvento = true;

				completaParametriRicerca();  // Mi serve true per eseguire anche la count dei risultati

				eseguireRicerca( true );

			}
		}

		#endregion MemBus

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
			public ParamCercaFoto paramCercaFoto { get; set;  }

		}

	}

	// La gallery può funzionare in queste modalità
	public enum ModalitaFiltroSelez {
		Tutte,
		SoloSelezionate
	}

}
