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

namespace Digiphoto.Lumen.UI {



	public class FotoGalleryViewModel : ViewModelBase, IObserver<GestoreCarrelloMsg>, IObserver<StampatoMsg>
	{

		private Boolean operazioniCarrelloBloccanti = false;

		private BackgroundWorker _bkgIdrata;

		public FotoGalleryViewModel() {

			IObservable<GestoreCarrelloMsg> observableCarrello = LumenApplication.Instance.bus.Observe<GestoreCarrelloMsg>();
			observableCarrello.Subscribe(this);

			IObservable<StampatoMsg> observableStampato = LumenApplication.Instance.bus.Observe<StampatoMsg>();
			observableStampato.Subscribe(this);

			metadati = new MetadatiFoto();

			// Istanzio i ViewModel dei componenti di cui io sono composto
			selettoreEventoFiltro = new SelettoreEventoViewModel();
			selettoreEventoMetadato = new SelettoreEventoViewModel();
			selettoreFotografoViewModel = new SelettoreFotografoViewModel();

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
			this.stampantiAbbinate = StampantiAbbinateUtil.deserializza( ss );
		}


		#region Proprietà

		/// <summary>
		///  Uso questa particolare collectionView perché voglio tenere traccia nel ViewModel degli N-elementi selezionati.
		///  Sottolineo N perché non c'è supporto nativo per questo. Vedere README.txt nel package di questa classe.
		/// </summary>
		public MultiSelectCollectionView<Fotografia> fotografieCW {
			get;
			set;
		}

		public ParamCercaFoto paramCercaFoto {
			get;
			set;
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
				return slideShowViewModel != null;
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
				return (slideShowViewModel != null) ? slideShowViewModel.slideShow.colonne : (short)2;
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
				return paramCercaFoto.numeriFotogrammi == null || paramCercaFoto.numeriFotogrammi.Length == 0 ? null : String.Join( ",", paramCercaFoto.numeriFotogrammi );
			}
			set {
				if( String.IsNullOrEmpty(value) )
					paramCercaFoto.numeriFotogrammi = null;
				else

					try {
						paramCercaFoto.numeriFotogrammi = value.Split( ',' ).Select( nn => Convert.ToInt32( nn ) ).ToArray();
					} catch( Exception ) {
						dialogProvider.ShowError( "I numeri dei fotogrammi devono essere separati da virgola", "Formato errato", null );
						OnPropertyChanged( "stringaNumeriFotogrammi" );
					}
			}
		}

		#endregion Proprietà

		public MetadatiFoto metadati {
			get;
			private set;
		}

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

		private RelayCommand _stampaRapidaCommand;
		public ICommand stampaRapidaCommand
		{
			get
			{
				if (_stampaRapidaCommand == null)
				{
					_stampaRapidaCommand = new RelayCommand(param => stampaRapida( param ),
						param => true, false);
				}
				return _stampaRapidaCommand;
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
					_eseguireRicercaCommand = new RelayCommand( param => eseguireRicerca() );
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

		private RelayCommand _applicareMetadatiCommand;
		public ICommand applicareMetadatiCommand {
			get {
				if( _applicareMetadatiCommand == null ) {
					_applicareMetadatiCommand = new RelayCommand( p => applicareMetadati(),
					                                              p => possoApplicareMetadati, false );
				}
				return _applicareMetadatiCommand;
			}
		}
		
		private RelayCommand _eliminareMetadatiCommand;
		public ICommand eliminareMetadatiCommand {
			get {
				if( _eliminareMetadatiCommand == null ) {
					_eliminareMetadatiCommand = new RelayCommand( p => eliminareMetadati(),
					                                              p => possoEliminareMetadati, false );
				}
				return _eliminareMetadatiCommand;
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
					venditoreSrv.creaNuovoCarrelloStampaDiretta();
					venditoreSrv.effettuaStampaDiretta( listaSelez, creaParamStampaFoto( stampanteAbbinata ) );
					if (venditoreSrv.vendereCarrelloStampaDiretta())
					{
						dialogProvider.ShowMessage("Carrello venduto Correttamente", "Avviso");
					}
					else
					{
						dialogProvider.ShowError("Errore inserimento carrello nella cassa","Errore", null);
					}
				}else{
					venditoreSrv.aggiungiStampe( listaSelez, creaParamStampaFoto( stampanteAbbinata ) );
				}
				// Spengo tutto
				deselezionareTutto();
			}
		}

		private void stampaRapida(object objStampanteAbbinata)
		{
			StampanteAbbinata stampanteAbbinata = (StampanteAbbinata)objStampanteAbbinata;

			IList<Fotografia> listaSelez = creaListaFotoSelezionate();

			venditoreSrv.creaNuovoCarrelloStampaDiretta();
			venditoreSrv.effettuaStampaDiretta(listaSelez, creaParamStampaFoto(stampanteAbbinata));
			if (venditoreSrv.vendereCarrelloStampaDiretta())
			{
				// quando tutto va bene non diciamo niente. Segnaliamo solo gli errori.
				// dialogProvider.ShowMessage("Carrello venduto Correttamente", "Avviso");
			}
			else
			{
				dialogProvider.ShowError("Stampa diretta non riuscita.", "Errore", null);
			}
			// Spengo tutto
			deselezionareTutto();
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

		void applicareMetadati() {

			// Ricavo l'Evento dall'apposito componente di selezione.
			// Tutti gli altri attributi sono bindati direttamente sulla struttura MetadatiFoto.
			metadati.evento = selettoreEventoMetadato.eventoSelezionato;

			fotoExplorerSrv.modificaMetadatiFotografie( creaListaFotoSelezionate(), metadati );

			// Svuoto ora i metadati per prossime elaborazioni
			metadati = new MetadatiFoto();
			selettoreEventoMetadato.eventoSelezionato = null;
			OnPropertyChanged( "metadati" );
		}

		void eliminareMetadati() {

			bool procediPure = false;
			dialogProvider.ShowConfirmation( "Sei sicuro di voler eliminare i metadati\ndelle " + fotografieCW.SelectedItems.Count + " fotografie selezionate?", "Eliminazione metadati",
								  ( confermato ) => {
									  procediPure = confermato;
								  } );

			if( !procediPure )
				return;

			fotoExplorerSrv.modificaMetadatiFotografie( creaListaFotoSelezionate(), new MetadatiFoto() );

			// Svuoto ora i metadati
			metadati = new MetadatiFoto();
			selettoreEventoMetadato.eventoSelezionato = null;
			dialogProvider.ShowMessage( "Eliminati i metadati delle " + fotografieCW.SelectedItems.Count + " fotografie selezionate!", "Operazione eseguita" );
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

		#endregion

	}
}
