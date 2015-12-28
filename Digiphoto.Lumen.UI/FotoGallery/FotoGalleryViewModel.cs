using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Diapo;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Digiphoto.Lumen.Servizi.Masterizzare;
using Digiphoto.Lumen.Servizi.Vendere;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Core;
using PeteBrown.ScreenCapture;
using System.Windows;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.UI.ScreenCapture;
using Digiphoto.Lumen.UI.Pubblico;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.UI.Main;
using Digiphoto.Lumen.UI.Dialogs;
using Digiphoto.Lumen.Servizi.EliminaFotoVecchie;
using Digiphoto.Lumen.Model.Util;
using System.Collections;
using Digiphoto.Lumen.UI.Util;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Digiphoto.Lumen.Servizi.Ritoccare.Clona;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.UI.SelettoreAzioniRapide;

namespace Digiphoto.Lumen.UI {



	public class FotoGalleryViewModel : ViewModelBase, IObserver<StampatoMsg>, IObserver<ClonaFotoMsg>, IObserver<SvuotaFiltriMsg>, IAzzioniRapide
	{
		private BackgroundWorker _bkgIdrata;

		// Quando viene eseguito un comando che potenzialmente modifica la visuale della finestra pubblica, lo segnalo con un evento
		public delegate void SnpashotCambiataEventHandler( object sender, EventArgs args );
		public event SnpashotCambiataEventHandler snpashotCambiataEventHandler;

		public FotoGalleryViewModel() {

			IObservable<StampatoMsg> observableStampato = LumenApplication.Instance.bus.Observe<StampatoMsg>();
			observableStampato.Subscribe(this);

			IObservable<ClonaFotoMsg> observableClonaFoto = LumenApplication.Instance.bus.Observe<ClonaFotoMsg>();
			observableClonaFoto.Subscribe(this);

			metadati = new MetadatiFoto();

			// Istanzio i ViewModel dei componenti di cui io sono composto
			selettoreEventoViewModel = new SelettoreEventoViewModel();
			selettoreScaricoCardViewModel = new SelettoreScaricoCardViewModel();
			selettoreEventoMetadato = new SelettoreEventoViewModel();
			selettoreFotografoViewModel = new SelettoreFotografoViewModel();
			selettoreAzioniRapideViewModel = new SelettoreAzioniRapideViewModel(this);

			azzeraParamRicerca();

			dimensioneIconaFoto = 120;  // Valore di default per la grandezza dei fotogrammi da visualizzare.

			if( IsInDesignMode ) {

			} else {

				//
				caricaStampantiAbbinate();

				_bkgIdrata = new BackgroundWorker();
				_bkgIdrata.WorkerReportsProgress = false;  // per ora non mi complico la vita
				_bkgIdrata.WorkerSupportsCancellation = true; // per ora non mi complico la vita
				_bkgIdrata.DoWork += new DoWorkEventHandler( bkgIdrata_DoWork );
				_bkgIdrata.RunWorkerCompleted += new RunWorkerCompletedEventHandler( bkgIdrata_RunWorkerCompleted );
				_bkgIdrata.ProgressChanged += new ProgressChangedEventHandler( bkgIdrata_ProgressChanged );
				_bkgIdrata.WorkerReportsProgress = true;
			}
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


		#region Proprietà

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

		public ParamCercaFoto paramCercaFoto {
			get;
			private set;
		}

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


		public bool isAlmenoUnaSelezionata {
			get {
				bool result = fotografieCW != null && fotografieCW.SelectedItems != null && fotografieCW.SelectedItems.Count > 0;
				if (!result && flagPosizionaSuSelezionate)
					flagPosizionaSuSelezionate = false;

				if (!result && flagFiltraSelezionate)
					flagFiltraSelezionate = false;

				return result;
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
				posso = fotografieCW != null && fotografieCW.SelectedItems.Count > 0;
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

		public bool possoAggiungereAlMasterizzatore {
			get {

				bool posso = true;

				if( !IsInDesignMode ) {

					if( posso && !isAlmenoUnaSelezionata )
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

					if( posso && !isAlmenoUnaSelezionata )
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
				return isAlmenoUnaSelezionata;
			}
		}

		public bool possoMandareInModifica
		{
			get {
				return isAlmenoUnaSelezionata;
			}
		}
		
		public bool possoEliminareMetadati {
			get {
				return isAlmenoUnaSelezionata;
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
				return fotografieCW != null && fotografieCW.SelectedItems.Count > 0;   // Se ho almeno una foto selezionata
			}
		}

		double _dimensioneIconaFoto;
		public double dimensioneIconaFoto {
			get {
				return _dimensioneIconaFoto;
			}
			set {
				if( _dimensioneIconaFoto != value ) {
					_dimensioneIconaFoto = value;
					OnPropertyChanged( "dimensioneIconaFoto" );
				}
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

		public MetadatiFoto metadati {
			get;
			private set;
		}

		public SelettoreAzioniRapideViewModel selettoreAzioniRapideViewModel
		{
			get;
			set;
		}

		
		public bool isGestitaPaginazione {
			get {
				return Configurazione.UserConfigLumen.paginazioneRisultatiGallery > 0;
			}
      }
			
		public int totFotoPaginaAttuale {
			get {
				return paginaAttualeRicerca * Configurazione.UserConfigLumen.paginazioneRisultatiGallery;
			}
		}

		short _paginaAttualeRicerca;
		public short paginaAttualeRicerca {
			get {
				return _paginaAttualeRicerca;
			}
			private set {
				if( _paginaAttualeRicerca != value ) {
					_paginaAttualeRicerca = value;
					OnPropertyChanged( "paginaAttualeRicerca" );
					OnPropertyChanged( "totFotoPaginaAttuale" );
					OnPropertyChanged( "stoPaginando" );
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

		private bool _flagPosizionaSuSelezionate;
		public bool flagPosizionaSuSelezionate {
			get {
				return _flagPosizionaSuSelezionate;
			}
			set {
				if( _flagPosizionaSuSelezionate != value ) {
					_flagPosizionaSuSelezionate = value;
					OnPropertyChanged( "flagPosizionaSuSelezionate" );

					posizionareSuSelezionate( value );
					if( value )
						flagFiltraSelezionate = false;
				}
			}
		}

		private bool _flagFiltraSelezionate;
		public bool flagFiltraSelezionate {
			get {
				return _flagFiltraSelezionate;
			}
			set {
				if( _flagFiltraSelezionate != value ) {
					_flagFiltraSelezionate = value;
					OnPropertyChanged( "flagFiltraSelezionate" );

					filtrareSelezionate( value );
					if( value )
						flagPosizionaSuSelezionate = false; // Spengo l'altro (sono esclusivi)
				}
			}
		}

		private ModoVendita _modoVendita = Configurazione.UserConfigLumen.modoVendita;
		public ModoVendita modoVendita
		{
			get
			{
				return _modoVendita;
			}
			set
			{
				if (_modoVendita != value)
				{
					_modoVendita = value;

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
					_filtrareSelezionateCommand = new RelayCommand( param => filtrareSelezionate( Convert.ToBoolean(param) ),
																	param => possoFiltrareSelezionate( Convert.ToBoolean( param ) ) );
				}
				return _filtrareSelezionateCommand;
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

		private bool possoFiltrareNumFotogramma( object numero ) {

			if( isAlmenoUnaFoto == false )
				return false;

			return true;
		}

		private bool possoFiltrareSelezionate( bool soloSelez ) {

			if( isAlmenoUnaFoto == false )
				return false;

			if( soloSelez == false ) // attulamente il pulsante NON è premuto. Quindi sto vedento tutte le foto. Devo dire se posso premere per filtrare solo le selezionate
				if( isAlmenoUnaSelezionata == false )
					return false;

			// if( soloSelez == true ) // Attualmente il pulsante E' premuto. Quindi sto vedendo solo le selezionate. Devo dire se posso premere per vedere tutto.

			return true;
		}

		private RelayCommand _eseguireRicercaCommand;
		public ICommand eseguireRicercaCommand {
			get {
				if( _eseguireRicercaCommand == null ) {
					_eseguireRicercaCommand = new RelayCommand( numPag => eseguireRicerca( true ), p => possoEseguireRicercaCommand, false );
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
					_controllareSlideShowCommand = new RelayCommand( azione => controllareSlideShow( (string)azione ),
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


/*
		private RelayCommand _screenShotPubblicaCommand;
		public ICommand screenShotPubblicaCommand {
			get {
				if( _screenShotPubblicaCommand == null ) {
					_screenShotPubblicaCommand = new RelayCommand( param => screenShotPubblica( param ) );
				}
				return _screenShotPubblicaCommand;
			}
		}


		private void screenShotPubblica( object param ) {
			FrameworkElement fwkElem = (FrameworkElement)param;
			BitmapSource screenShot = SnapshotUtil.CreateBitmap( fwkElem, true );
			windowPubblicaViewModel.screenShot = screenShot;
		}
*/
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
																  param => possoMandareInModifica	);
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

		private void filtrareSelezionate( bool attivareFiltro ) {

			// Alcune collezioni non sono filtrabili, per esempio la IEnumerable
			if( fotografieCW.CanFilter == false )
				return;

			if( attivareFiltro ) {

				// Creo un oggetto Predicate al volo.
				fotografieCW.Filter = obj => {
					return fotografieCW.SelectedItems.Contains( obj );
				};

			} else {
				fotografieCW.Filter = null;
			}

			raiseSnpashotCambiataEvent( EventArgs.Empty );
		}

		private void posizionareSuSelezionate( bool attivare ) {
			// per ora niente
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

		private IList<Fotografia> creaListaFotoSelezionate() {
			return fotografieCW.SelectedItems.ToList();
		}

		private void deselezionareTutto() {
			accendiSpegniTutto( false );
		}
		
		private void selezionareTutto() {
			accendiSpegniTutto( true );
		}

		/// <summary>
		/// Accendo o Spengo tutte le selezioni
		/// </summary>
		private void accendiSpegniTutto( bool selez ) {
			if( selez )
				fotografieCW.SelectAll();
			else
				fotografieCW.DeselectAll();
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
						venditoreStampaDiretta.creaNuovoCarrello();
						venditoreStampaDiretta.carrello.intestazione = VenditoreSrvImpl.INTESTAZIONE_STAMPA_RAPIDA;
						venditoreStampaDiretta.aggiungereStampe(listaSelez, creaParamStampaFoto(stampanteAbbinata));
						if (venditoreStampaDiretta.vendereCarrello())
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
			d.totaleFotoSelezionate = fotografieCW.SelectedItems.Count;
			d.totoleFotoGallery = fotografieCW.Count;
			bool? esito = d.ShowDialog();

			if (esito == true)
			{
				if (!d.stampaSoloSelezionate)
				{
					deselezionareTutto();
					selezionareTutto();
				}

				IList<Fotografia> listaSelez = creaListaFotoSelezionate();

				// Riordino i Provini per data acquisizione foto + numero foto (Prima quelli più vecchi)
				IEnumerable<Fotografia> sortedEnum = listaSelez.OrderBy(f => f.dataOraAcquisizione).OrderBy(f => f.numero);
				listaSelez = sortedEnum.ToList();
	
// non capisco a cosa servisse clonare i parametri			
//				venditoreSrv.aggiungereStampe(listaSelez, creaParamStampaProvini(d.paramStampaProvini));
				venditoreSrv.aggiungereStampe( listaSelez, d.paramStampaProvini );

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
		
// non so perché l'avevo fatto. Sembra un clone !! boh ??		
#if false
		private ParamStampaProvini creaParamStampaProvini( ParamStampaProvini param ) {

			ParamStampaProvini p = venditoreSrv.creaParamStampaProvini();

			p.nomeStampante = param.nomeStampante;
			p.formatoCarta = param.formatoCarta;
			p.numeroColonne = param.numeroColonne;
			p.numeroRighe = param.numeroRighe;
			p.intestazione = param.intestazione;
			p.macchiaProvini = param.macchiaProvini;
			// TODO per ora il nome della Porta a cui è collegata la stampante non lo uso. Non so cosa farci.

			return p;
		}
#endif

		//public string paramGiornataIniz {
		//    get;
		//    set;
		//}

		/// <summary>
		/// Chiamo il servizio che esegue la query sul database
		/// </summary>
		private void eseguireRicerca() {
			eseguireRicerca( false );
		}

		private void eseguireRicerca( bool chiediConfermaSeFiltriVuoti ) {

			// Se avevo un worker già attivo, allora provo a cancellarlo.
			if( _bkgIdrata.WorkerSupportsCancellation == true && _bkgIdrata.IsBusy ) {
				_giornale.Debug( "idratatore impegnato. Lo stoppo" );
				_bkgIdrata.CancelAsync();
				return;
			}

			idrataProgress = 0;

			// while( _bkgIdrata.IsBusy )
			//	System.Threading.Thread.Sleep( 2000 );

			completaParametriRicercaWithOrder(true);

			//Dopo aver completato tutti i parametri di ricerca...
			//verifico se ho impostato almeno un parametro
			if( chiediConfermaSeFiltriVuoti )
				if (verificaChiediConfermaRicercaSenzaParametri() == false)
					return;

			// Eseguo la ricerca nel database
			fotoExplorerSrv.cercaFoto( paramCercaFoto );

			// ricreo la collection-view e notifico che è cambiato il risultato. Le immagini verranno caricate poi
			fotografieCW = new MultiSelectCollectionView<Fotografia>( fotoExplorerSrv.fotografie );
			fotografieCW.SelectionChanged += fotografie_selezioneCambiata;


			// spengo tutte le selezioni eventualmente rimaste da prima
			deselezionareTutto();

			// Se non ho trovato nulla, allora avviso l'utente
			if( fotografieCW.Count <= 0 )
				dialogProvider.ShowMessage( "Nessuna fotografia trovata con questi filtri di ricerca", "AVVISO" );

			OnPropertyChanged( "totFotoPaginaAttuale" );
			OnPropertyChanged( "stoPaginando" );
			OnPropertyChanged( "isAlmenoUnaFoto" );

			// Ora ci penso io ad idratare le immagini, perchè devo fare questa operazione nello stesso thread della UI
			if( !_bkgIdrata.IsBusy )
				_bkgIdrata.RunWorkerAsync();
			// Lasciare come ultima cosa l'idratazione delle foto.
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
			OnPropertyChanged( "isAlmenoUnaSelezionata" );
		}

		public void calcolaFotoCorrenteSelezionataScorrimento( int direzione ) {

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

		private void bkgIdrata_DoWork( object sender, DoWorkEventArgs e ) {

			_giornale.Debug( "Inizio a idratare le foto in background" );

			BackgroundWorker worker = sender as BackgroundWorker;

			int tot = fotoExplorerSrv.fotografie.Count;
			int percPrec = 0;
			worker.ReportProgress( percPrec );


			for( int ii = 0; (ii < tot); ii++ ) {
				if( (worker.CancellationPending == true) ) {
					e.Cancel = true;
					break;
				} else {

					try {
						// Perform a time consuming operation and report progress.
						AiutanteFoto.idrataImmaginiFoto( fotoExplorerSrv.fotografie[ii], IdrataTarget.Provino );						
					} catch( Exception ) {

						// Provo a crearlo. Magari non c'è perché è stato cancellato per sbaglio, oppure c'è ma si è corrotto.
						try {

							// Provo a dare il fuoco al mio usercontrol ma nel thread della GUI
							Fotografia daRiProvinare = fotoExplorerSrv.fotografie[ii];
//							bool okCiSonoRiuscito = false;
							App.Current.Dispatcher.BeginInvoke(
								new Action( () => {

									try {
										AiutanteFoto.creaProvinoFoto( daRiProvinare );
//										okCiSonoRiuscito = true;
									} catch( Exception ) {
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
			if( ! e.Cancel )
				worker.ReportProgress( 100 );

		}

		void bkgIdrata_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e ) {

			_giornale.Debug( "Terminato di idratare le foto in background: abortito = " + e.Cancelled );

			RicercaModificataMessaggio msg = new RicercaModificataMessaggio( this );
			msg.abortito = e.Cancelled;
			LumenApplication.Instance.bus.Publish( msg );

			raiseSnpashotCambiataEvent( EventArgs.Empty );
		}

		void bkgIdrata_ProgressChanged( object sender, ProgressChangedEventArgs e ) {
			idrataProgress = e.ProgressPercentage;
		}

		private void completaParametriRicerca()
		{
			completaParametriRicercaWithOrder(false);
		}

		/// <summary>
		/// Sistemo i parametri e gestisco la paginazione
		/// </summary>
		/// <param name="numPagina">il numero di pagina</param>
		private void completaParametriRicercaWithOrder(bool usaOrdinamentoAsc)
		{

			paramCercaFoto.idratareImmagini = false;

			if(usaOrdinamentoAsc)
				paramCercaFoto.ordinamentoAsc = Configurazione.UserConfigLumen.invertiRicerca;

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
			if( selettoreScaricoCardViewModel.scaricoCardSelezionato != null )
				paramCercaFoto.scaricoCard = selettoreScaricoCardViewModel.scaricoCardSelezionato;
			else
				paramCercaFoto.scaricoCard = null;

			// Gestisco la paginazione
			if( paginaAttualeRicerca > 0 && paramCercaFoto.paginazione != null ) {
				paramCercaFoto.paginazione.skip = ((paginaAttualeRicerca - 1) * Configurazione.UserConfigLumen.paginazioneRisultatiGallery);
			}

		}

		private void caricareSlideShow( string modo ) {

			((App)Application.Current).gestoreFinestrePubbliche.forseApriSlideShowWindow();

			if( modo == "AddSelez" )
				slideShowViewModel.add( creaListaFotoSelezionate() );
			else if( modo == "ZeroPiuSelez" ) {
				// Azzera e fa partire le selezionate
				slideShowViewModel.creaShow( creaListaFotoSelezionate() );
			} else if( modo == "Tutte" ) {
				completaParametriRicerca();
				ParamCercaFoto copiaParam = paramCercaFoto.ShallowCopy();
				slideShowViewModel.creaShow( copiaParam );
			} else {
				throw new ArgumentOutOfRangeException( "modo slide show" );
			}

			// L'azione di play deve essere automatica (ipse dixit)
			slideShowViewModel.start();

			OnPropertyChanged( "possoControllareSlideShow" );
		}

		private void controllareSlideShow( string operaz ) {

			switch( operaz.ToUpper() ) {

				case "START":
					slideShowViewModel.start();
					break;
				
				case "STOP":
					slideShowViewModel.stop();
					break;

				case "RESET":
					slideShowViewModel.reset();
					break;
			}
		}


		private void playPauseSlideShow() {
			if( slideShowViewModel.isRunning )
				slideShowViewModel.stop();
			else if( slideShowViewModel.isPaused )
				slideShowViewModel.start();
		}


		void azzeraParamRicerca() {
			paramCercaFoto = new ParamCercaFoto();
			
			OnPropertyChanged( "paramCercaFoto" );
			OnPropertyChanged( "stringaNumeriFotogrammi" );

			OnPropertyChanged( "isMattinoChecked" );
			OnPropertyChanged( "isPomeriggioChecked" );
			OnPropertyChanged( "isSeraChecked" );


			selettoreEventoViewModel.eventoSelezionato = null;
			selettoreScaricoCardViewModel.scaricoCardSelezionato = null;
			selettoreFotografoViewModel.fotografoSelezionato = null;


			// Se gestita la paginazione, istanzio apposita classe
			if( Configurazione.UserConfigLumen.paginazioneRisultatiGallery > 0 ) {
				paramCercaFoto.paginazione = new Paginazione {
					take = Configurazione.UserConfigLumen.paginazioneRisultatiGallery
				};
			}

			paginaAttualeRicerca = 1;
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

			if( deltaPag == -999 )
				paginaAttualeRicerca = 1;
			else
				paginaAttualeRicerca += deltaPag;
			
			// Non posso sformare il minimo
			if( paginaAttualeRicerca < 1 )
				paginaAttualeRicerca = 1;

			eseguireRicerca();
		}

		bool possoSpostarePaginazione( short delta ) {

			if( IsInDesignMode )
				return true;

			// Se gestisco la paginazione ed ho un risultato caricato.
			bool posso = stoPaginando;

			if( posso && delta > 0 ) {
				// Voglio spostarmi avanti. Controllo di NON essere sull'ultimo risultato.
				posso = (fotoExplorerSrv.fotografie.Count >= Configurazione.UserConfigLumen.paginazioneRisultatiGallery);
			}

			if( posso && delta < 0 ) {

				// Voglio spostarmi indietro. Controllo di avere sufficienti pagine precedenti.
				if( delta == -999 )  // Torno alla prima pagina
					posso = (paginaAttualeRicerca > 1);
				else
					posso = (paginaAttualeRicerca + delta ) > 0;
			}

			return posso;
		}

		public bool stoPaginando {
			get {
				return isGestitaPaginazione
					&& fotoExplorerSrv != null && fotoExplorerSrv.fotografie != null
					&& (paginaAttualeRicerca > 1 || (fotoExplorerSrv.fotografie.Count >= Configurazione.UserConfigLumen.paginazioneRisultatiGallery) );
			}
		}


		private bool riportaOriginaleFotoSelezionate()
		{
			bool procediPure = true;

			if (!fotografieCW.SelectedItems.Any())
				return false;

			if (fotografieCW.SelectedItems.Count > 10)
			{
				procediPure = false;
				StringBuilder msg = new StringBuilder("Attenzione: stai per far tornare a Originale più di 10 Fotografie!!!");
				dialogProvider.ShowConfirmation(msg.ToString(), "Richiesta conferma",
					(confermato) =>
					{
						procediPure = confermato;
					});
			}

			if (procediPure)
			{

				foreach (Fotografia f in fotografieCW.SelectedItems)
				{
					fotoRitoccoSrv.tornaOriginale(f);
				}

				dialogProvider.ShowMessage("Operazione Terminata", "Info");
			}

			return true;
		}

		#endregion Metodi

		#region Gestori Eventi

		/// <summary>
		/// Avviso gli interessati che la schermata pubblica (snapshot) è cambiata.
		/// Chi crede può prenderne atto.
		/// </summary>
		/// <param name="e"></param>
		protected void raiseSnpashotCambiataEvent( EventArgs e ) {
			if( snpashotCambiataEventHandler != null )
				snpashotCambiataEventHandler( this, e );
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
		
		public void OnNext(StampatoMsg value)
		{
			// TODO forse non serve più ascoltare questo messaggio.
			//      Ora il messaggio lo ascolta il MainWindow per dare avviso all'utente
		}

		public void OnNext(ClonaFotoMsg value)
		{
			if (value.fase == FaseClone.FineClone)
			{
				this.eseguireRicerca();
			}
		}

        public void OnNext(SvuotaFiltriMsg value)
        {
            if (value.sender is SelettoreScaricoCardViewModel)
            {
                paramCercaFoto = new ParamCercaFoto();

                OnPropertyChanged("paramCercaFoto");
                OnPropertyChanged("stringaNumeriFotogrammi");

                OnPropertyChanged("isMattinoChecked");
                OnPropertyChanged("isPomeriggioChecked");
                OnPropertyChanged("isSeraChecked");


                selettoreEventoViewModel.eventoSelezionato = null;
                selettoreFotografoViewModel.fotografoSelezionato = null;
            }
            else if(value.sender is SelettoreEventoViewModel)
            {
                selettoreScaricoCardViewModel.scaricoCardSelezionato = null;
            }
            else if(value.sender is SelettoreFotografoViewModel)
            {
                selettoreScaricoCardViewModel.scaricoCardSelezionato = null;
            }
        }

        #endregion
    }
}
