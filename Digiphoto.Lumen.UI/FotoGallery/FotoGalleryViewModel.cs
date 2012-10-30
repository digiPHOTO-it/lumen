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
using Digiphoto.Lumen.Servizi.Ritoccare.Clona;

namespace Digiphoto.Lumen.UI {



	public class FotoGalleryViewModel : ViewModelBase, IObserver<GestoreCarrelloMsg>, IObserver<StampatoMsg>, IObserver<ClonaFotoMsg>
	{

		private Boolean operazioniCarrelloBloccanti = false;

		private BackgroundWorker _bkgIdrata;

		public FotoGalleryViewModel() {

			IObservable<GestoreCarrelloMsg> observableCarrello = LumenApplication.Instance.bus.Observe<GestoreCarrelloMsg>();
			observableCarrello.Subscribe(this);

			IObservable<StampatoMsg> observableStampato = LumenApplication.Instance.bus.Observe<StampatoMsg>();
			observableStampato.Subscribe(this);

			IObservable<ClonaFotoMsg> observableClonaFoto = LumenApplication.Instance.bus.Observe<ClonaFotoMsg>();
			observableClonaFoto.Subscribe(this);

			metadati = new MetadatiFoto();

			// Istanzio i ViewModel dei componenti di cui io sono composto
			selettoreEventoFiltro = new SelettoreEventoViewModel();
			selettoreEventoMetadato = new SelettoreEventoViewModel();
			selettoreFotografoViewModel = new SelettoreFotografoViewModel();
			selettoreAzioniRapideViewModel = new SelettoreAzioniRapideViewModel();

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
					selettoreAzioniRapideViewModel.fotografieCW = value;
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

		public bool isAlmenoUnaSelezionata {
			get {
				return fotografieCW != null && fotografieCW.SelectedItems != null && fotografieCW.SelectedItems.Count > 0;
			}
		}

		public bool possoCaricareSlideShow {
			get {
				return fotografieCW != null && fotografieCW.SelectedItems.Count > 0;
			}
		}

		public bool possoControllareSlideShow
		{
			get
			{
				return slideShowViewModel != null && slideShowViewModel.slideShow != null;
			}
		}

		public bool possoAggiungereAlMasterizzatore {
			get {

				if (IsInDesignMode)
					return true;

				bool posso = true;

				if (posso && !isAlmenoUnaSelezionata)
					posso = false;

				// Verifico che non abbia fatto nel carrello operazioni di 
				// stampa con errore o abbia caricato un carrello salvato
				if (posso && operazioniCarrelloBloccanti)
					posso = false;

				return posso;
			}
		}

		public bool possoStampare {
			get {
				if (IsInDesignMode)
				return true;

				bool posso = true;

				if (posso && !isAlmenoUnaSelezionata)
					posso = false;

				// Verifico che non abbia fatto nel carrello operazioni di 
				// stampa con errore o abbia caricato un carrello salvato
				if (posso && operazioniCarrelloBloccanti)
					posso = false;

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

		public SelettoreEventoViewModel selettoreEventoFiltro {
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

		public BitmapSource statusSlideShowImage {

			get {
				// Decido qual'è la giusta icona da caricare per mostrare lo stato dello slide show (Running, Pause, Empty)

				// Non so perchè ma se metto il percorso senza il pack, non funziona. boh eppure sono nello stesso assembly.
				string uriTemplate = @"pack://application:,,,/Digiphoto.Lumen.UI;component/Resources/##-16x16.png";
				Uri uri = null;

				if( slideShowViewModel != null ) {
					if( slideShowViewModel.isRunning )
						uri = new Uri( uriTemplate.Replace( "##", "ssRunning" ) );

					if( slideShowViewModel.isPaused )
						uri = new Uri( uriTemplate.Replace( "##", "ssPause" ) );

					if( slideShowViewModel.isEmpty )
						uri = new Uri( uriTemplate.Replace( "##", "ssEmpty" ) );

					return new BitmapImage( uri );
				} else
					return null;
			}
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
				return myApp.slideShowViewModel;
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
				return ( slideShowViewModel != null ) ? slideShowViewModel.slideShow.righe : (short)1;
			}
			set {
				if( slideShowViewModel != null )
					if( slideShowViewModel.slideShowRighe != value ) {
						slideShowViewModel.slideShowRighe = value;
						OnPropertyChanged( "numRigheSlideShow" );
					}
			}
		}

		public short numColonneSlideShow {
			get {
				return (slideShowViewModel != null && slideShowViewModel.slideShow != null) ? slideShowViewModel.slideShow.colonne : (short)2;
			}
			set {
				if( slideShowViewModel != null )
					if( slideShowViewModel.slideShowColonne != value ) {
						slideShowViewModel.slideShowColonne = value;
						OnPropertyChanged( "numColonneSlideShow" );
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
																		   );
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
					_filtrareSelezionateCommand = new RelayCommand( param => filtrareSelezionate( Convert.ToBoolean(param) ) );
				}
				return _filtrareSelezionateCommand;
			}
		}

		private RelayCommand _eseguireRicercaCommand;
		public ICommand eseguireRicercaCommand {
			get {
				if( _eseguireRicercaCommand == null ) {
					_eseguireRicercaCommand = new RelayCommand( numPag => eseguireRicerca() );
				}
				return _eseguireRicercaCommand;
			}
		}

		private RelayCommand _caricareSlideShowCommand;
		public ICommand caricareSlideShowCommand {
			get {
				if( _caricareSlideShowCommand == null ) {
					_caricareSlideShowCommand = 
						new RelayCommand( autoManual => caricareSlideShow( (string) autoManual ),
										  autoManual => possoCaricareSlideShow );
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

		#endregion


		#region Metodi

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
		}

		/// <summary>
		/// Aggiungo le immagini selezionate al masterizzatore
		/// </summary>
		/// <returns></returns>
		public void aggiungereAlMasterizzatore() {

			IEnumerable<Fotografia> listaSelez = creaListaFotoSelezionate();
			venditoreSrv.aggiungiMasterizzate( listaSelez );
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

		/// <summary>
		/// Devo mandare in stampa le foto selezionate
		/// Nel parametro mi arriva l'oggetto StampanteAbbinata che mi da tutte le indicazioni
		/// per la stampa: il formato carta e la stampante
		/// </summary>
		private void stampare( object objStampanteAbbinata ) {
			
			StampanteAbbinata stampanteAbbinata = (StampanteAbbinata)objStampanteAbbinata;

			IList<Fotografia> listaSelez = creaListaFotoSelezionate();

			// Se ho selezionato più di una foto, e lavoro in stampa diretta, allora chiedo conferma
			bool procediPure = true;
			int quante = listaSelez.Count;
			if (quante >= 1 && Configurazione.UserConfigLumen.modoVendita == ModoVendita.StampaDiretta)
			{
				dialogProvider.ShowConfirmation( "Confermi la stampa di " + quante + " foto ?", "Richiesta conferma",
				  (confermato) => {
					  procediPure = confermato;
				  } );
			}

			if( procediPure ) {
				if(Configurazione.UserConfigLumen.modoVendita == ModoVendita.StampaDiretta){
					using( IVenditoreSrv venditoreStampaDiretta = LumenApplication.Instance.creaServizio<IVenditoreSrv>() ) 
					{
						venditoreStampaDiretta.creaNuovoCarrello();
						venditoreStampaDiretta.carrello.intestazione = VenditoreSrvImpl.INTESTAZIONE_STAMPA_RAPIDA;
						venditoreStampaDiretta.aggiungiStampe(listaSelez, creaParamStampaFoto(stampanteAbbinata));
						if (venditoreStampaDiretta.vendereCarrello())
						{
							dialogProvider.ShowMessage("Carrello venduto Correttamente", "Avviso");
						}
						else
						{
							dialogProvider.ShowError("Errore inserimento carrello nella cassa","Errore", null);
						}
					}
				}else{
					venditoreSrv.aggiungiStampe( listaSelez, creaParamStampaFoto( stampanteAbbinata ) );
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
				
				venditoreSrv.aggiungiStampe(listaSelez, creaParamStampaProvini(d.paramStampaProvini));
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

		//public string paramGiornataIniz {
		//    get;
		//    set;
		//}

		/// <summary>
		/// Chiamo il servizio che esegue la query sul database
		/// </summary>
		private void eseguireRicerca() {

			// Se avevo un worker già attivo, allora provo a cancellarlo.
			if( _bkgIdrata.WorkerSupportsCancellation == true && _bkgIdrata.IsBusy )
				_bkgIdrata.CancelAsync();
				

			completaParametriRicerca();

			// Eseguo la ricerca nel database
			fotoExplorerSrv.cercaFoto( paramCercaFoto );


			// Ora ci penso io ad idratare le immagini, perchè devo fare questa operazione nello stesso thread della UI
			if( ! _bkgIdrata.IsBusy )
				_bkgIdrata.RunWorkerAsync();


			// ricreo la collection-view e notifico che è cambiato il risultato. Le immagini verranno caricate poi
			fotografieCW = new MultiSelectCollectionView<Fotografia>( fotoExplorerSrv.fotografie );
			OnPropertyChanged( "fotografieCW" );

			// spengo tutte le selezioni eventualmente rimaste da prima
			deselezionareTutto();

			// Se non ho trovato nulla, allora avviso l'utente
			if( fotografieCW.Count <= 0 )
				dialogProvider.ShowMessage( "Nessuna fotografia trovata con questi filtri di ricerca", "AVVISO" );

			OnPropertyChanged( "totFotoPaginaAttuale" );
			OnPropertyChanged( "stoPaginando" );
		}

		private void bkgIdrata_DoWork( object sender, DoWorkEventArgs e ) {
			
			BackgroundWorker worker = sender as BackgroundWorker;

			int tot = fotoExplorerSrv.fotografie.Count;

			for( int ii = 0; (ii < tot); ii++ ) {
				if( (worker.CancellationPending == true) ) {
					e.Cancel = true;
					break;
				} else {

					// Perform a time consuming operation and report progress.
					AiutanteFoto.idrataImmaginiFoto( fotoExplorerSrv.fotografie[ii], IdrataTarget.Provino );
					// worker.ReportProgress( 123 );
				}
			}

			LumenApplication.Instance.bus.Publish( new RicercaModificataMessaggio( this ) );
		}

		/// <summary>
		/// Sistemo i parametri e gestisco la paginazione
		/// </summary>
		/// <param name="numPagina">il numero di pagina</param>
		private void completaParametriRicerca() {

			paramCercaFoto.idratareImmagini = false;

			// Aggiungo eventuale parametro il fotografo
			if( selettoreFotografoViewModel.fotografoSelezionato != null )
				paramCercaFoto.fotografi = new Fotografo [] { selettoreFotografoViewModel.fotografoSelezionato };
			else
				paramCercaFoto.fotografi = null;

			// Aggiungo eventuale parametro l'evento
			if( selettoreEventoFiltro.eventoSelezionato != null )
				paramCercaFoto.eventi = new Evento [] { selettoreEventoFiltro.eventoSelezionato };
			else
				paramCercaFoto.eventi = null;

			// Gestisco la paginazione
			if( paginaAttualeRicerca > 0 && paramCercaFoto.paginazione != null ) {
				paramCercaFoto.paginazione.skip = ((paginaAttualeRicerca - 1) * Configurazione.UserConfigLumen.paginazioneRisultatiGallery);
			}

		}

		private void caricareSlideShow( string modo ) {

			((App)Application.Current).forseApriWindowPubblica();

			if( modo.Equals( "Manual", StringComparison.CurrentCultureIgnoreCase ) )
				slideShowViewModel.creaShow( creaListaFotoSelezionate() );
			else if( modo.Equals( "Auto", StringComparison.CurrentCultureIgnoreCase ) ) {
				completaParametriRicerca();
				ParamCercaFoto copiaParam = paramCercaFoto.ShallowCopy();
				slideShowViewModel.creaShow( copiaParam );
			} else {
				throw new ArgumentOutOfRangeException( "modo slide show" );
			}

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

			OnPropertyChanged( "statusSlideShowImage" );
		}

		void azzeraParamRicerca() {
			paramCercaFoto = new ParamCercaFoto();
			
			OnPropertyChanged( "paramCercaFoto" );
			OnPropertyChanged( "stringaNumeriFotogrammi" );

			OnPropertyChanged( "isMattinoChecked" );
			OnPropertyChanged( "isPomeriggioChecked" );
			OnPropertyChanged( "isSeraChecked" );


			selettoreEventoFiltro.eventoSelezionato = null;
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
		
		public void OnNext(GestoreCarrelloMsg msg)
		{
			if (msg.fase == Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.ErroreMasterizzazione ||
				msg.fase == Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.LoadCarrelloSalvato)
			{
				operazioniCarrelloBloccanti = true;
			}

			if (msg.fase == Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.CreatoNuovoCarrello)
			{
				operazioniCarrelloBloccanti = false;
			}
		}

		public void OnNext(StampatoMsg value)
		{
			if (value.lavoroDiStampa.esitostampa == EsitoStampa.Errore)
			{
				dialogProvider.ShowError("Stampa non Eseguita Correttamente", "Errore", null);
			}
		}

		public void OnNext(ClonaFotoMsg value)
		{
			this.eseguireRicerca();
		}

		#endregion

	}
}
