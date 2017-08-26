using System;
using System.Collections.Generic;
using System.Linq;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Model;
using System.Windows.Input;
using Digiphoto.Lumen.UI.FotoRitocco;
using Digiphoto.Lumen.Servizi.Vendere;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Servizi.EliminaFotoVecchie;
using Digiphoto.Lumen.Util;
using System.Windows;
using Digiphoto.Lumen.UI.Pubblico;
using Digiphoto.Lumen.UI.PanAndZoom;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.Core.Collections;
using Digiphoto.Lumen.UI.Dialogs;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;

namespace Digiphoto.Lumen.UI {

	public enum TargetMode {
		Singola,
		Selezionate,
		Tutte
	}


	public class SelettoreAzioniRapideViewModel : ViewModelBase, IObserver<GestoreCarrelloMsg> {

		private Boolean operazioniCarrelloBloccanti = false;

		public event Digiphoto.Lumen.UI.FotoRitocco.FotoRitoccoViewModel.EditorModeChangedEventHandler editorModeChangedEvent;

		public ISelettore<Fotografia> fotografieSelector;
		
		public SelettoreFotografoViewModel selettoreFotografoViewModelFaccia {
			get;
			private set;
		}


		public SelettoreAzioniRapideViewModel( ISelettore<Fotografia> fotografieSelector ) {
			this.fotografieSelector = fotografieSelector;
			IObservable<GestoreCarrelloMsg> observableCarrello = LumenApplication.Instance.bus.Observe<GestoreCarrelloMsg>();
			observableCarrello.Subscribe( this );

			selettoreFotografoViewModelFaccia = new SelettoreFotografoViewModel();


			if( IsInDesignMode ) {
			} else {
				caricaStampantiAbbinate();
			}
		}

		#region Proprieta

		public IList<StampanteAbbinata> stampantiAbbinate {
			get;
			private set;
		}

		private bool _isTuttoBloccato;
		public bool isTuttoBloccato {
			get {
				return _isTuttoBloccato;
			}
			set {
				if( _isTuttoBloccato != value ) {
					_isTuttoBloccato = value;
					OnPropertyChanged( "isTuttoBloccato" );
				}
			}
		}

		// Questo view model lo recupero dalla application.
		private SlideShowViewModel slideShowViewModel {
			get {
				if( IsInDesignMode )
					return null;

				App myApp = (App)Application.Current;
				return myApp.gestoreFinestrePubbliche.slideShowViewModel;
			}
		}

		public bool gestitaSelezioneMultipla {
			get; set;
		}
		
		private bool _visualizzaEliminaFoto = true;
		public bool visualizzaEliminaFoto {
			get {
				return _visualizzaEliminaFoto;
			}
			set {
				if( _visualizzaEliminaFoto != value ) {
					_visualizzaEliminaFoto = value;
					OnPropertyChanged( "visualizzaEliminaFoto" );
				}
			}
		}

		/// <summary>
		/// Questo metodo ritorna tutte le foto selezionate, anche in pagine diverse.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<Fotografia> getFotoSelezionate() {
			return fotografieSelector.getElementiSelezionati();
		}

		private IEnumerable<Fotografia> getFotoTutte() {
			return fotografieSelector.getElementiTutti();
		}


		private bool _sceltaFotografoPopupIsOpen;
		public bool sceltaFotografoPopupIsOpen
		{
			get
			{
				return _sceltaFotografoPopupIsOpen;
			}
			set
			{
				if( _sceltaFotografoPopupIsOpen != value ) {
					_sceltaFotografoPopupIsOpen = value;
					OnPropertyChanged( "sceltaFotografoPopupIsOpen" );
				}
			}
		}

		#endregion Proprieta

		#region Servizi

		IFotoExplorerSrv fotoExplorerSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
			}
		}

		private IVenditoreSrv venditoreSrv {
			get {
				return (IVenditoreSrv)LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();
			}
		}

		public IFotoRitoccoSrv fotoRitoccoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IFotoRitoccoSrv>();
			}
		}

		public IGestoreImmagineSrv gestoreImmaginiSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
			}
		}

		#endregion Servizi

		#region Controlli


		public bool possoCaricareSlideShow {
			get {
				return isAlmenoUnElementoSelezionato;
			}
		}

		public bool possoAggiungereAlMasterizzatore {
			get {
				if( IsInDesignMode )
					return true;

				bool posso = true;

				if( posso && !isAlmenoUnElementoSelezionato )
					posso = false;

				// Verifico che non abbia fatto nel carrello operazioni di 
				// stampa con errore o abbia caricato un carrello salvato
				if( posso && operazioniCarrelloBloccanti )
					posso = false;

				return posso;
			}
		}

		public bool possoStampare {
			get {
				if( IsInDesignMode )
					return true;

				bool posso = true;

				if( posso && ! isAlmenoUnElementoSelezionato )
					posso = false;

				// Verifico che non abbia fatto nel carrello operazioni di 
				// stampa con errore o abbia caricato un carrello salvato
				if( posso && operazioniCarrelloBloccanti )
					posso = false;

				return posso;
			}
		}

		public bool possoApplicareCorrezione {
			get {
				
				if( targetMode == TargetMode.Singola && singolaFotoTarget != null )
					return true;

				if( targetMode == TargetMode.Selezionate && countSelezionate > 0 )
					return true;

				if( targetMode == TargetMode.Tutte && countTotali > 0 )
					return true;
				
				return false;
			}
		}

		#endregion Controlli


		#region Metodi

		/// <summary>
		/// Carico tutti i formati carta che sono abbinati alle stampanti installate
		/// Per ognuno di questi elementi, dovrò visualizzare un pulsante per la stampa
		/// </summary>
		private void caricaStampantiAbbinate() {
			string ss = Configurazione.UserConfigLumen.stampantiAbbinate;
			this.stampantiAbbinate = StampantiAbbinateUtil.deserializza( ss );
		}

		/// <summary>
		/// Aggiungo le immagini selezionate al masterizzatore
		/// </summary>
		/// <returns></returns>
		public void aggiungereAlMasterizzatore() {
			if( venditoreSrv.possoAggiungereMasterizzate ) {
				venditoreSrv.aggiungereMasterizzate( getListaFotoTarget() );
				deselezionareTutto();
			}

		}

		private void deselezionareTutto() {
			fotografieSelector.deselezionareTutto();
		}

		private int countSelezionate {
			get {
				return fotografieSelector.countElementiSelezionati;
			}
		}

		private int countTotali {
			get {
				return fotografieSelector.countElementiTotali;
			}
		}

		private bool chiedereConfermaPerProseguire( string msg ) {
			return chiedereConfermaPerProseguire( msg, "Richiesta conferma" );
		}

		private bool chiedereConfermaPerProseguire( string msg, string tit ) {

			bool procediPure = false;

			dialogProvider.ShowConfirmation( msg, tit,
				confermato => {
					procediPure = confermato;
				} );

			return procediPure;
		}

		/// <summary>
		/// Devo mandare in stampa le foto selezionate
		/// Nel parametro mi arriva l'oggetto StampanteAbbinata che mi da tutte le indicazioni
		/// per la stampa: il formato carta e la stampante
		/// </summary>
		private void stampare( StampanteAbbinata stampanteAbbinata )
		{
			
			// Se ho selezionato più di una foto, e lavoro in stampa diretta, allora chiedo conferma
			bool procediPure = true;
			
			if (countSelezionate >= 1 && Configurazione.UserConfigLumen.modoVendita == ModoVendita.StampaDiretta)
			{
				procediPure = chiedereConfermaPerProseguire( "Confermi la stampa di " + countSelezionate + " foto ?" );
			}

			if (procediPure)
			{
				if (Configurazione.UserConfigLumen.modoVendita == ModoVendita.StampaDiretta)
				{
					using (IVenditoreSrv venditoreStampaDiretta = LumenApplication.Instance.creaServizio<IVenditoreSrv>())
					{
						venditoreStampaDiretta.creareNuovoCarrello();
						venditoreStampaDiretta.carrello.intestazione = VenditoreSrvImpl.INTESTAZIONE_STAMPA_RAPIDA;
						venditoreStampaDiretta.aggiungereStampe( getListaFotoTarget(), creaParamStampaFoto(stampanteAbbinata));
						string msgErrore = venditoreStampaDiretta.vendereCarrello();
						bool esitoOk = (msgErrore == null);
						if( esitoOk )
						{
							dialogProvider.ShowMessage("Carrello venduto Correttamente", "Avviso");
						}
						else
						{
							dialogProvider.ShowError("Errore inserimento carrello nella cassa", "Errore", null);
						}
					}
				}
				else
				{
					venditoreSrv.aggiungereStampe( getListaFotoTarget(), creaParamStampaFoto(stampanteAbbinata));
				}

				deselezionaFoto();
			}
		}

		private void stampaRapida( StampanteAbbinata stampanteAbbinata, bool autoZoomNoBordiBianchi )
		{

			// Un parametro della configurazione mi dice il totale foto oltre il quale chiedere conferma
			if(    targetMode == TargetMode.Selezionate 
			    && Configurazione.UserConfigLumen.sogliaNumFotoConfermaInStampaRapida > 0 
				&& countSelezionate >= Configurazione.UserConfigLumen.sogliaNumFotoConfermaInStampaRapida ) {
				bool procediPure = false;

				procediPure = chiedereConfermaPerProseguire( "Sei sicuro di voler stampare\nle " + countSelezionate + " fotografie selezionate?", "Stampa rapida foto senza carrello" );

				if( !procediPure )
					return;
			}

			using (IVenditoreSrv venditoreSpampaRapida = LumenApplication.Instance.creaServizio<IVenditoreSrv>())
			{

				venditoreSpampaRapida.creareNuovoCarrello();
				venditoreSpampaRapida.carrello.intestazione = VenditoreSrvImpl.INTESTAZIONE_STAMPA_RAPIDA;
				var listaFoto = getListaFotoTarget();
				var param = creaParamStampaFoto( stampanteAbbinata, autoZoomNoBordiBianchi );

				venditoreSpampaRapida.aggiungereStampe( listaFoto, param );
				
				string msgErrore = venditoreSpampaRapida.vendereCarrello();
				bool esitoOk = (msgErrore == null);
                if( esitoOk )
				{
					// Spengo le foto che ormai sono andate
					deselezionaFoto();

				}
				else
				{
					dialogProvider.ShowError("Stampa diretta non riuscita.", "Errore", null);
				}
			}
		}

		/// <summary>
		/// Creo i parametri di stampa, mixando un pò di informazioni prese
		/// dalla configurazione, dallo stato dell'applicazione, e dalla scelta dell'utente.
		/// </summary>
		/// <param name="stampanteAbbinata"></param>
		/// <returns></returns>
		private ParamStampaFoto creaParamStampaFoto( StampanteAbbinata stampanteAbbinata ) {
			return creaParamStampaFoto( stampanteAbbinata, true );
		}

		private ParamStampaFoto creaParamStampaFoto(StampanteAbbinata stampanteAbbinata, bool autoZoomNoBordiBianchi )
		{

			ParamStampaFoto p = venditoreSrv.creaParamStampaFoto();

			p.nomeStampante = stampanteAbbinata.StampanteInstallata.NomeStampante;
			p.formatoCarta = stampanteAbbinata.FormatoCarta;
			p.autoZoomNoBordiBianchi = autoZoomNoBordiBianchi;
			// TODO per ora il nome della Porta a cui è collegata la stampante non lo uso. Non so cosa farci.

			return p;
		}

		void modificaMetadati() {

/*
			var lista = getListaFotoTarget().ToList();
			MultiSelectCollectionView<Fotografia> fotoSel = new MultiSelectCollectionView<Fotografia>( lista );
			foreach( Fotografia fS in lista ) {
				fotoSel.SelectedItems.Add( fS );
			}
*/
			SelettoreMetadatiDialog s = new SelettoreMetadatiDialog( getListaFotoTarget() );

			bool? esito = s.ShowDialog();

			if( esito == true ) {
				// boh
			}

			s.Close();

			// Spengo le foto su cui ho agito
			deselezionaFoto();
		}

		/// <summary>
		/// Eliminazione definitiva delle foto selezionate.
		/// </summary>
		/// <param name="param">Se param è una stringa e contiene il valore "SEL" allora elimino le foto selezionate.</param>
		void eliminareFoto() {

			IEnumerable<Fotografia> itemsToDelete = getListaFotoTarget();

			string msg = "Sei sicuro di voler eliminare definitivamente\nle " + itemsToDelete.Count() + " fotografie selezionate?\nL'operazione non è recuperabile !";
			bool procediPure = chiedereConfermaPerProseguire( msg, "Eliminazione definitiva foto" );

			if( !procediPure )
				return;

			if( itemsToDelete.Count() > 5 )
				procediPure = chiedereConfermaPerProseguire( msg, "Eliminazione definitiva foto, 2^ conferma" );

			if( !procediPure )
				return;

			// chiamo il servizio che mi elimina fisicamente i files immagini, e le Fotografie dal db.
			int quanti = 0;
			using( IEliminaFotoVecchieSrv srv = LumenApplication.Instance.creaServizio<IEliminaFotoVecchieSrv>() ) {
				quanti = srv.elimina( itemsToDelete );
			}
		}

		private void ruotare(int pGradi)
		{
			fotoRitoccoSrv.ruotare( getListaFotoTarget(), pGradi );
		}

		private IEnumerable<Fotografia> getListaFotoTarget() {

			IEnumerable<Fotografia> lista = null;

			// Demando al servizio più opportuno
			if( targetMode == TargetMode.Singola )
				lista = new Fotografia[] { singolaFotoTarget };
			else if( targetMode == TargetMode.Selezionate )
				lista = getFotoSelezionate();
			else if( targetMode == TargetMode.Tutte )
				lista = getFotoTutte();

			return lista;

		}


		/// <summary>
		/// Aggiunge il logo di default alla foto
		/// </summary>
		/// <param name="posiz"></param>
		private void aggiungereLogo( String posiz ) {

			if( targetMode == TargetMode.Singola )
				fotoRitoccoSrv.addLogoDefault( singolaFotoTarget, posiz, true );
			else {
				IEnumerator<Fotografia> itera = null;
				if( targetMode == TargetMode.Selezionate )
					itera = fotografieSelector.getEnumeratorElementiSelezionati();
				if( targetMode == TargetMode.Tutte )
					itera = fotografieSelector.getEnumeratorElementiTutti();

				while( itera.MoveNext() )
					fotoRitoccoSrv.addLogoDefault( itera.Current, posiz, true );
			}	
        }

		private void tornareOriginale()
		{
			if( targetMode == TargetMode.Singola )
				fotoRitoccoSrv.tornaOriginale( singolaFotoTarget );
			else {

				if( !chiedereConfermaPerProseguire( "Confermi torna originale" ) )
					return;


				// Uso metodo specifico che lavora su tutta la collezione
				IEnumerable<Fotografia> lista = null;

				if( targetMode == TargetMode.Selezionate )
					lista = fotografieSelector.getElementiSelezionati();
				if( targetMode == TargetMode.Tutte )
					lista = fotografieSelector.getElementiTutti();

				fotoRitoccoSrv.tornaOriginale( lista );
			}

		}

		private void caricareSlideShow(string modo)
		{
			// Se lo ss è ancora mai stato usato, apro la finestra
			((App)Application.Current).gestoreFinestrePubbliche.aprireFinestraSlideShow();

			IEnumerable<Fotografia> lista = getListaFotoTarget();

			if( modo.Equals( "Manual", StringComparison.CurrentCultureIgnoreCase ) ) {
				slideShowViewModel.add( lista.ToList() );

				if( ! slideShowViewModel.isRunning )
					slideShowViewModel.start();

			} else
				_giornale.Error( "modo slide show non riconoscito: " + modo );

			
		}


		private void viewFotoFullScreen()
		{
			string nomeFile = AiutanteFoto.idrataImmagineDaStampare( singolaFotoTarget );

			PanAndZoomViewModel panZommViewModel = new PanAndZoomViewModel(nomeFile);
			PanAndZoomWindow w = new Digiphoto.Lumen.UI.PanAndZoom.PanAndZoomWindow();
			w.DataContext = panZommViewModel;
			w.ShowDialog();

		}

		private void clonaFotografie() {

			var vettore = getListaFotoTarget().ToArray<Fotografia>();
			
			if( vettore.Count() > 3 )
				if( ! chiedereConfermaPerProseguire( "Sei sicuro di voler clonare " + vettore.Count() + " foto ?", "Conferma Clone Multiplo" ) )
					return;
					
			fotoRitoccoSrv.clonaFotografie( vettore );
		}

		/// <summary>
		/// Questa proprietà mi dice su quale range di elementi voglio lavorare
		/// </summary>
#if false
		private TargetMode _targetMode;
        public TargetMode targetMode {
			set {
				if( _targetMode != value ) {
					_targetMode = value;
					OnPropertyChanged( "targetMode" );
				}
			}

			get {
				return _targetMode;
			}
		}
#else
			/// <summary>
			/// <para>identifica la modalità di target: una - selezionate - tutte</para>
			/// <para>Vedi anche <seealso cref="singolaFotoTarget"/></para>
			/// </summary>
		public TargetMode targetMode {
			get; 
			set;
		}
#endif



		/// <summary>
		/// <para>
		/// Questa è la foto che è stata selezionata con il tasto destro. 
		/// Serve per inviare i comandi alla singola foto.
		/// </para>
		/// <para>Vedi anche <seealso cref="targetMode"/></para>
		/// </summary>
		public Fotografia singolaFotoTarget {
			get;
			private set;
		}
		
		

		/// <summary>
		/// Imposto il target su cui sto lavorando
		/// </summary>
		/// <param name="param"></param>
		public void setTarget( string param )
		{
			// Setto solo la modalità
			targetMode = (TargetMode) Enum.Parse( typeof( TargetMode ), param );
		}

		public void setTarget( Fotografia foto ) {
			// Setto sia la foto che la modalità. Qui arrivo quando catturo l'evento di mouse down sulla foto
			targetMode = TargetMode.Singola;
			singolaFotoTarget = foto;
		}

		public void deselezionaFoto()
        {
			if( targetMode == TargetMode.Singola )
				fotografieSelector.deselezionareSingola( singolaFotoTarget );
			else 
				deselezionareTutto();
        }
		
		public bool isAlmenoUnElementoSelezionato {
			get {
				return countSelezionate > 0;
			}
		}

		void sceltaFotografoPopup() {
			sceltaFotografoPopupIsOpen = true;
		}
	

		#endregion Metodi


		#region Comandi

		private RelayCommand _aggiungereAlMasterizzatoreCommand;
		public ICommand aggiungereAlMasterizzatoreCommand
		{
			get
			{
				if (_aggiungereAlMasterizzatoreCommand == null)
				{
					_aggiungereAlMasterizzatoreCommand = new RelayCommand(param => aggiungereAlMasterizzatore()
																		   , param => possoAggiungereAlMasterizzatore
																		   , false
                                                                           , param => deselezionaFoto() );
				}
				return _aggiungereAlMasterizzatoreCommand;
			}
		}

		private RelayCommand _stampareCommand;
		public ICommand stampareCommand
		{
			get
			{
				if (_stampareCommand == null)
				{
					_stampareCommand = new RelayCommand(param => stampare( param as StampanteAbbinata ),
					                                    param => possoStampare, 
                                                        false,
                                                        param => deselezionaFoto());
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
					_stampaRapidaCommand = new RelayCommand(param => stampaRapida( (StampanteAbbinata)param, true ),
					                                        param => this.possoStampare, 
                                                            false,
                                                            param => deselezionaFoto());
				}
				return _stampaRapidaCommand;
			}
		}
		private RelayCommand _stampaRapidaBordiBianchiCommand;
		public ICommand stampaRapidaBordiBianchiCommand {
			get {
				if( _stampaRapidaBordiBianchiCommand == null ) {
					_stampaRapidaBordiBianchiCommand = new RelayCommand( param => stampaRapida( (StampanteAbbinata)param, false ),
															  param => this.possoStampare,
                                                              false, 
                                                              param => deselezionaFoto());
				}
				return _stampaRapidaBordiBianchiCommand;
			}
		}


		private RelayCommand _caricareSlideShowCommand;
		public ICommand caricareSlideShowCommand
		{
			get
			{
				if (_caricareSlideShowCommand == null)
				{
					_caricareSlideShowCommand =
						new RelayCommand(autoManual => caricareSlideShow((string)autoManual),
										  autoManual => possoCaricareSlideShow, 
                                          null,
                                          param => deselezionaFoto()
                                          );
				}
				return _caricareSlideShowCommand;
			}
		}

		private RelayCommand _eliminareFotoCommand;
		public ICommand eliminareFotoCommand
		{
			get
			{
				if (_eliminareFotoCommand == null)
				{
					_eliminareFotoCommand = new RelayCommand(param => eliminareFoto(),
															 p => isAlmenoUnElementoSelezionato, 
                                                             false, 
                                                             param => deselezionaFoto());
				}
				return _eliminareFotoCommand;
			}
		}

		private RelayCommand _ruotareCommand;
		public ICommand ruotareCommand
		{
			get
			{
				if (_ruotareCommand == null)
				{
					_ruotareCommand = new RelayCommand(sGradi => this.ruotare(Convert.ToInt16(sGradi)),
														sGradi => this.possoApplicareCorrezione,
														true,
                                                        param => deselezionaFoto());
				}
				return _ruotareCommand;
			}
		}
		

		private RelayCommand _aggiungereLogoCommand;
		public ICommand aggiungereLogoCommand
		{
			get
			{
				if (_aggiungereLogoCommand == null)
				{
					_aggiungereLogoCommand = new RelayCommand( posiz => this.aggiungereLogo( posiz.ToString() ),
						                                       posiz => this.possoApplicareCorrezione,
														       true,
                                                               param => deselezionaFoto());
				}
				return _aggiungereLogoCommand;
			}
		}

		private RelayCommand _tornareOriginaleCommand;
		public ICommand tornareOriginaleCommand
		{
			get
			{
				if (_tornareOriginaleCommand == null)
				{
					_tornareOriginaleCommand = new RelayCommand(param => this.tornareOriginale(),
														gradi => this.possoApplicareCorrezione,
														true,
                                                        param => deselezionaFoto());
				}
				return _tornareOriginaleCommand;
			}
		}

		private RelayCommand _modificaMetadatiCommand;
		public ICommand modificaMetadatiCommand {
			get {
				if( _modificaMetadatiCommand == null ) {
					_modificaMetadatiCommand = new RelayCommand( p => modificaMetadati(),
					                                             p => possoApplicareCorrezione,
					                                             true,
					                                             param => deselezionaFoto() );
				}
				return _modificaMetadatiCommand;
			}
		}

		private RelayCommand _viewFotoFullScreenCommand;
		public ICommand viewFotoFullScreenCommand
		{
			get
			{
				if (_viewFotoFullScreenCommand == null)
				{
                    _viewFotoFullScreenCommand = new RelayCommand(param => viewFotoFullScreen(), 
                                                                  p => isAlmenoUnElementoSelezionato, 
                                                                  false,
                                                                  param => deselezionaFoto());
				}
				return _viewFotoFullScreenCommand;
			}
		}

		private RelayCommand _clonaFotografieCommand;
		public ICommand clonaFotografieCommand {
			get {
				if( _clonaFotografieCommand == null ) {
					_clonaFotografieCommand = new RelayCommand( param => clonaFotografie(),
															   p => isAlmenoUnElementoSelezionato,
															   false,
															   param => deselezionaFoto() );
				}
				return _clonaFotografieCommand;
			}
		}

		private RelayCommand _sceltaFotografoPopupCommand;
		public ICommand sceltaFotografoPopupCommand
		{
			get
			{
				if( _sceltaFotografoPopupCommand == null ) {
					_sceltaFotografoPopupCommand = new RelayCommand( param => sceltaFotografoPopup(),
					                                                 p => isAlmenoUnElementoSelezionato,
																	 false );
				}
				return _sceltaFotografoPopupCommand;
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

		#region Gestori Eventi

		protected void onEditorModeChanged(EditorModeEventArgs e)
		{
			if (editorModeChangedEvent != null)
				editorModeChangedEvent(this, e);
		}

		#endregion Gestori Eventi

		#endregion






		// ------------------

		private bool _associaFacciaFotografoPopupIsOpen;
		public bool associaFacciaFotografoPopupIsOpen
		{
			get { return _associaFacciaFotografoPopupIsOpen; }
			set
			{
				_associaFacciaFotografoPopupIsOpen = value;
				OnPropertyChanged( "associaFacciaFotografoPopupIsOpen" );
			}
		}

		private RelayCommand _openAssociaFacciaFotografoPopupCommand;
		public ICommand openAssociaFacciaFotografoPopupCommand
		{
			get
			{
				if( _openAssociaFacciaFotografoPopupCommand == null ) {
					_openAssociaFacciaFotografoPopupCommand = new RelayCommand( param => openAssociaFacciaFotografoPopup(), param => true, false );
				}
				return _openAssociaFacciaFotografoPopupCommand;
			}
		}


		private void openAssociaFacciaFotografoPopup() {
			associaFacciaFotografoPopupIsOpen = true;
		}

		void associareFacciaFotografo() {

			// La foto la prendo da quella selezionata
			if( targetMode != TargetMode.Singola )
				throw new InvalidOperationException( "Operazione solo su singola foto" );

			if( selettoreFotografoViewModelFaccia.countElementiSelezionati != 1 )
				throw new InvalidOperationException( "Non è stato selezionato un Fotografo" );

			Fotografia fotografia = getListaFotoTarget().Single();

			Fotografo fotografo = selettoreFotografoViewModelFaccia.fotografoSelezionato;

			AiutanteFoto.setImmagineFotografo( fotografia, fotografo );

			// Spengo la selezione per la prossima volta
			selettoreFotografoViewModelFaccia.fotografiCW.deselezionaTutto();

			string msg = string.Format( "OK : impostata immagine per il fotografo {0}\nCon la foto numero {1}", fotografo.cognomeNome, fotografia.numero );
			dialogProvider.ShowMessage( msg, "Operazione riuscita" );
		}


		private RelayCommand _associareFacciaFotografoCommand;
		public ICommand associareFacciaFotografoCommand
		{
			get
			{
				if( _associareFacciaFotografoCommand == null ) {
					_associareFacciaFotografoCommand = new RelayCommand( param => associareFacciaFotografo(), param => true, false );
				}
				return _associareFacciaFotografoCommand;
			}
		}
		

	}
}
