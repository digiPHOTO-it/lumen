using System;
using System.Collections.Generic;
using System.Linq;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.Model;
using System.Windows.Input;
using Digiphoto.Lumen.UI.FotoRitocco;
using Digiphoto.Lumen.Servizi.Vendere;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.Dialogs;
using Digiphoto.Lumen.Servizi.EliminaFotoVecchie;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Util;
using System.Windows;
using Digiphoto.Lumen.UI.Pubblico;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.UI.PanAndZoom;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.UI.SelettoreAzioniRapide;

namespace Digiphoto.Lumen.UI
{
	public class SelettoreAzioniRapideViewModel : ViewModelBase, IObserver<GestoreCarrelloMsg>
	{
		private Boolean operazioniCarrelloBloccanti = false;

		public event Digiphoto.Lumen.UI.FotoRitocco.FotoRitoccoViewModel.EditorModeChangedEventHandler editorModeChangedEvent;

		public IAzzioniRapide azioniRapideViewModel;

		public SelettoreAzioniRapideViewModel(IAzzioniRapide azioniRapideViewModel)
		{
			this.azioniRapideViewModel = azioniRapideViewModel;
			IObservable<GestoreCarrelloMsg> observableCarrello = LumenApplication.Instance.bus.Observe<GestoreCarrelloMsg>();
			observableCarrello.Subscribe(this);

			if (IsInDesignMode)
			{
			}
			else
			{
				caricaStampantiAbbinate();
			}
		}

		#region Proprieta

		public IList<StampanteAbbinata> stampantiAbbinate
		{
			get;
			private set;
		}

		public MultiSelectCollectionView<Fotografia> fotografieCW
		{
			get{
				return azioniRapideViewModel.fotografieCW;
			}
		}

        private String _visibility;
        public String visibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    OnPropertyChanged("visibility");
                }
            }
        }

		private bool _isTuttoBloccato;
		public bool isTuttoBloccato
		{
			get
			{
				return _isTuttoBloccato;
			}
			set
			{
				if (_isTuttoBloccato != value)
				{
					_isTuttoBloccato = value;
					OnPropertyChanged("isTuttoBloccato");
				}
			}
		}

		// Questo view model lo recupero dalla application.
		private SlideShowViewModel slideShowViewModel
		{
			get
			{
				if (IsInDesignMode)
					return null;

				App myApp = (App)Application.Current;
				return myApp.gestoreFinestrePubbliche.slideShowViewModel;
			}
		}

		public IList<Fotografia> fotoSelezionate
		{
			get
			{
				if (singolaFotoWorks)
				{
					List<Fotografia> foto = new List<Fotografia>();
					foto.Add(ultimaFotoSelezionata);
					return foto.ToList();
				}
				else
				{
					if (fotografieCW == null)
						return null;
					else
						return fotografieCW.SelectedItems.ToList();
				}
			}
		}

		private Fotografia _ultimaFotoSelezionata;
		public Fotografia ultimaFotoSelezionata
		{
			get
			{
				return _ultimaFotoSelezionata;
			}
			set
			{
				if(_ultimaFotoSelezionata != value){
					_ultimaFotoSelezionata = value;
					OnPropertyChanged("ultimaFotoSelezionata");
				}
			}
		}

        private bool _visualizzaEliminaFoto = true;
        public bool visualizzaEliminaFoto
        {
            get
            {
                return _visualizzaEliminaFoto;
            }
            set
            {
                if (_visualizzaEliminaFoto != value)
                {
                    _visualizzaEliminaFoto = value;
                    OnPropertyChanged("visualizzaEliminaFoto");
                }
            }
        }

        #endregion Proprieta

        #region Servizi

        IFotoExplorerSrv fotoExplorerSrv
		{
			get
			{
				return LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
			}
		}

		private IVenditoreSrv venditoreSrv
		{
			get
			{
				return (IVenditoreSrv)LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();
			}
		}

		public IFotoRitoccoSrv fotoRitoccoSrv
		{
			get
			{
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

		public bool isAlmenoUnaSelezionata
		{
			get
			{
				return fotoSelezionate != null && fotoSelezionate.Count > 0;
			}
		}

		public bool possoCaricareSlideShow
		{
			get
			{
				return isAlmenoUnaSelezionata;
			}
		}

		public bool possoAggiungereAlMasterizzatore
		{
			get
			{
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

		public bool possoStampare
		{
			get
			{
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

		public bool possoMandareInModifica
		{
			get
			{
				return isAlmenoUnaSelezionata;
			}
		}

		public bool possoApplicareCorrezione
		{
			get
			{
				return isAlmenoUnaSelezionata;
			}
		}

		#endregion Controlli


		#region Metodi

		/// <summary>
		/// Carico tutti i formati carta che sono abbinati alle stampanti installate
		/// Per ognuno di questi elementi, dovrò visualizzare un pulsante per la stampa
		/// </summary>
		private void caricaStampantiAbbinate()
		{
			string ss = Configurazione.UserConfigLumen.stampantiAbbinate;
			this.stampantiAbbinate = StampantiAbbinateUtil.deserializza(ss);
		}

		/// <summary>
		/// Aggiungo le immagini selezionate al masterizzatore
		/// </summary>
		/// <returns></returns>
		public void aggiungereAlMasterizzatore()
		{
			IEnumerable<Fotografia> listaSelez = fotoSelezionate;
			venditoreSrv.aggiungereMasterizzate(listaSelez);
			deselezionareTutto();
		}

		private void deselezionareTutto()
		{
			accendiSpegniTutto(false);
		}

		private void selezionareTutto()
		{
			accendiSpegniTutto(true);
		}

		/// <summary>
		/// Accendo o Spengo tutte le selezioni
		/// </summary>
		private void accendiSpegniTutto(bool selez)
		{
			if (fotografieCW == null)
				return;
			
			if (selez)
				fotografieCW.SelectAll();
			else
				fotografieCW.DeselectAll();
			
		}

		/// <summary>
		/// Devo mandare in stampa le foto selezionate
		/// Nel parametro mi arriva l'oggetto StampanteAbbinata che mi da tutte le indicazioni
		/// per la stampa: il formato carta e la stampante
		/// </summary>
		private void stampare( StampanteAbbinata stampanteAbbinata )
		{
			IList<Fotografia> listaSelez = fotoSelezionate;

			// Se ho selezionato più di una foto, e lavoro in stampa diretta, allora chiedo conferma
			bool procediPure = true;
			int quante = listaSelez.Count;
			if (quante >= 1 && Configurazione.UserConfigLumen.modoVendita == ModoVendita.StampaDiretta)
			{
				dialogProvider.ShowConfirmation("Confermi la stampa di " + quante + " foto ?", "Richiesta conferma",
				  (confermato) =>
				  {
					  procediPure = confermato;
				  });
			}

			if (procediPure)
			{
				if (Configurazione.UserConfigLumen.modoVendita == ModoVendita.StampaDiretta)
				{
					using (IVenditoreSrv venditoreStampaDiretta = LumenApplication.Instance.creaServizio<IVenditoreSrv>())
					{
						venditoreStampaDiretta.creareNuovoCarrello();
						venditoreStampaDiretta.carrello.intestazione = VenditoreSrvImpl.INTESTAZIONE_STAMPA_RAPIDA;
						venditoreStampaDiretta.aggiungereStampe(listaSelez, creaParamStampaFoto(stampanteAbbinata));
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
					venditoreSrv.aggiungereStampe(listaSelez, creaParamStampaFoto(stampanteAbbinata));
				}
				// Spengo tutto
				if (!singolaFotoWorks)
					deselezionareTutto();
			}
		}

		private void stampaRapida( StampanteAbbinata stampanteAbbinata, bool autoZoomNoBordiBianchi )
		{

			// Un parametro della configurazione mi dice il totale foto oltre il quale chiedere conferma
			if( Configurazione.UserConfigLumen.sogliaNumFotoConfermaInStampaRapida > 0 && fotoSelezionate.Count >= Configurazione.UserConfigLumen.sogliaNumFotoConfermaInStampaRapida ) {
				bool procediPure = false;
				dialogProvider.ShowConfirmation( "Sei sicuro di voler stampare\nle " + fotoSelezionate.Count + " fotografie selezionate?", "Stampa rapida foto senza carrello",
									  ( confermato ) => {
										  procediPure = confermato;
									  } );
				if( !procediPure )
					return;
			}

			using (IVenditoreSrv venditoreSpampaRapida = LumenApplication.Instance.creaServizio<IVenditoreSrv>())
			{

				venditoreSpampaRapida.creareNuovoCarrello();
				venditoreSpampaRapida.carrello.intestazione = VenditoreSrvImpl.INTESTAZIONE_STAMPA_RAPIDA;
				venditoreSpampaRapida.aggiungereStampe( fotoSelezionate, creaParamStampaFoto( stampanteAbbinata, autoZoomNoBordiBianchi) );

				string msgErrore = venditoreSpampaRapida.vendereCarrello();
				bool esitoOk = (msgErrore == null);
                if( esitoOk )
				{
					// quando tutto va bene non diciamo niente. Segnaliamo solo gli errori.
					// dialogProvider.ShowMessage("Carrello venduto Correttamente", "Avviso");
                    foreach(Fotografia foto in fotoSelezionate)
                    {
                        foto.contaStampata++;
                    }
				}
				else
				{
					dialogProvider.ShowError("Stampa diretta non riuscita.", "Errore", null);
				}
				// Spengo tutto
				if (!singolaFotoWorks)
					deselezionareTutto();
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
		
		void modificaMetadati()
		{

			MultiSelectCollectionView<Fotografia> fotoSel = new MultiSelectCollectionView<Fotografia>(fotoSelezionate.ToList<Fotografia>());
			foreach (Fotografia fS in fotoSelezionate){
				fotoSel.SelectedItems.Add(fS);
			}

			SelettoreMetadatiDialog s = new SelettoreMetadatiDialog(fotoSel);

			bool? esito = s.ShowDialog();

			if (esito == true)
			{

			}

			s.Close();

			// Spengo tutto
			if (!singolaFotoWorks)
				deselezionareTutto();
		}

		/// <summary>
		/// Eliminazione definitiva delle foto selezionate.
		/// </summary>
		/// <param name="param">Se param è una stringa e contiene il valore "SEL" allora elimino le foto selezionate.</param>
		void eliminareFoto() {

			IEnumerable<Fotografia> itemsToDelete = null;

			if (fotoSelezionate.Count <= 0)
					return;

			itemsToDelete = fotoSelezionate;

			bool procediPure = false;
			dialogProvider.ShowConfirmation("Sei sicuro di voler eliminare definitivamente\nle " + fotoSelezionate.Count + " fotografie selezionate?", "Eliminazione definitiva foto",
								  ( confermato ) => {
									  procediPure = confermato;
								  } );

			if( !procediPure )
				return;

			// chiamo il servizio che mi elimina fisicamente i files immagini, e le Fotografie dal db.
			int quanti = 0;
			using( IEliminaFotoVecchieSrv srv = LumenApplication.Instance.creaServizio<IEliminaFotoVecchieSrv>() ) {
				quanti = srv.elimina( itemsToDelete );
			}

			// Poi visto che erano a video, elimino le fotografie dalla Gallery.	
			// Devo però duplicare la collezione perché non posso iterare e rimuovere contemporaneamente dalla stessa.
			if( quanti > 0 ) {
				Fotografia [] listaClone = itemsToDelete.ToArray();
				if (fotografieCW != null)
				{
					foreach (Fotografia fDacanc in listaClone)
						fotografieCW.Remove(fDacanc);
					if (!singolaFotoWorks)
						deselezionareTutto();
				}
			} else
				dialogProvider.ShowError( "Impossibile eliminare le foto indicate", "ERRORE", null );
		}

		private void ruotare(int pGradi)
		{
			// Demando al servizio più opportuno
			fotoRitoccoSrv.ruotare( fotoSelezionate.AsEnumerable(), pGradi );
		}

		/// <summary>
		/// Aggiunge il logo di default alla foto
		/// </summary>
		/// <param name="posiz"></param>
		private void aggiungereLogo( String posiz ) {

#if DEBUG
// TODO eliminare: solo per prova !! ! ! !
Digiphoto.Lumen.Servizi.EntityRepository.IEntityRepositorySrv<AzioneAuto> repo = LumenApplication.Instance.getServizioAvviato<Digiphoto.Lumen.Servizi.EntityRepository.IEntityRepositorySrv<AzioneAuto>>();
string id = "42662270-CB0D-40A1-A179-EB753F955DE2";
id = "aac63718-b23d-4d33-8371-a10dee7c67a0";
id = "6cbd4017-ce5c-4231-9735-ab0f62f517f1";
AzioneAuto azioneAuto = repo.getById( new Guid( id ) );
fotoRitoccoSrv.applicareAzioneAutomatica( fotoSelezionate.AsEnumerable(), azioneAuto );
#else
			foreach( Fotografia f in fotoSelezionate ) {
				fotoRitoccoSrv.addLogoDefault( f, posiz, true );
			}
#endif
			
		}

		private void tornareOriginale()
		{
			// per ogni foto elimino le correzioni e ricreo il provino partendo dall'originale.
			foreach( Fotografia f in fotoSelezionate )
				fotoRitoccoSrv.tornaOriginale( f );
		}

		private void caricareSlideShow(string modo)
		{
			((App)Application.Current).gestoreFinestrePubbliche.forseApriFinestraSlideShow();

			if (modo.Equals("Manual", StringComparison.CurrentCultureIgnoreCase))
				slideShowViewModel.creaShow(fotoSelezionate);
			else if (modo.Equals("Auto", StringComparison.CurrentCultureIgnoreCase))
			{
			}
			else
			{
				throw new ArgumentOutOfRangeException("modo slide show");
			}
		}

		/// <summary>
		/// Aggiungo alla lista delle foto da modificare, tutte le foto che sono illuminate
		/// </summary>
		void mandareInModifica()
		{
			// Pubblico un messaggio indicando che ci sono delle foto da modificare.
			FotoDaModificareMsg msg = new FotoDaModificareMsg(this);
			msg.fotosDaModificare.InsertRange(0, fotoSelezionate);

			// Per semplificare le operazioni lavoro ancora in modalità immediata altrimenti ci sono troppi tasti da premere.
			msg.immediata = true;

			LumenApplication.Instance.bus.Publish(msg);

		}

		private void viewFotoFullScreen()
		{

			if (fotoSelezionate.Count <= 0)
				return;
			
			string nomeFile = AiutanteFoto.idrataImmagineDaStampare(fotoSelezionate.First());

			PanAndZoomViewModel panZommViewModel = new PanAndZoomViewModel(nomeFile);
			PanAndZoomWindow w = new Digiphoto.Lumen.UI.PanAndZoom.PanAndZoomWindow();
			w.DataContext = panZommViewModel;
			w.ShowDialog();

		}

		private void clonaFotografie()
		{
			if (fotoSelezionate.Count <= 0)
			{
				return;
			}
			else if (fotoSelezionate.Count > 1)
			{
				bool procediPure = false;
				dialogProvider.ShowConfirmation("Sei sicuro di voler clonare " + fotoSelezionate.Count + " foto ?", "Conferma Clone Multiplo",
								  ( confermato ) => {
									  procediPure = confermato;
								  } );

			if( !procediPure )
				return;
			}
			
			fotoRitoccoSrv.clonaFotografie(fotoSelezionate.ToArray<Fotografia>());
		}

		private bool singolaFotoWorks = false;
		private void setSingolaFotoWork(Object param)
		{
			if(param.ToString().Equals("SINGOLA")){
				singolaFotoWorks = true;
			}else if(param.ToString().Equals("MULTI")){
				singolaFotoWorks = false;
			}
		}

        public void deselezionaFoto()
        {
            if (!singolaFotoWorks)
                deselezionareTutto();
            else
            {
                if(fotografieCW != null && fotografieCW.SelectedItems != null)
                {
                    fotografieCW.Deselect(ultimaFotoSelezionata);
                }
            }
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

		private RelayCommand _mandareInModificaCommand;
		public ICommand mandareInModificaCommand
		{
			get
			{
				if (_mandareInModificaCommand == null)
				{
					_mandareInModificaCommand = new RelayCommand(param => mandareInModifica(),
																  param => possoMandareInModifica,
                                                                  null,
                                                                  param => deselezionaFoto());
				}
				return _mandareInModificaCommand;
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
															 p => isAlmenoUnaSelezionata, 
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
		public ICommand modificaMetadatiCommand
		{
			get
			{
				if (_modificaMetadatiCommand == null)
				{
					_modificaMetadatiCommand = new RelayCommand(p => modificaMetadati(),
																p => isAlmenoUnaSelezionata,
																true,
                                                                param => deselezionaFoto());
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
                                                                  p => isAlmenoUnaSelezionata, 
                                                                  false,
                                                                  param => deselezionaFoto());
				}
				return _viewFotoFullScreenCommand;
			}
		}

		private RelayCommand _clonaFotografieCommand;
		public ICommand clonaFotografieCommand
		{
			get
			{
				if (_clonaFotografieCommand == null)
				{
					_clonaFotografieCommand = new RelayCommand(param => clonaFotografie(), 
                                                               p => isAlmenoUnaSelezionata, 
                                                               null,
                                                               param => deselezionaFoto());
				}
				return _clonaFotografieCommand;
			}
		}

		private RelayCommand _setSingolaFotoWorkCommand;
		public ICommand setSingolaFotoWorkCommand
		{
			get
			{
				if (_setSingolaFotoWorkCommand == null)
				{
					_setSingolaFotoWorkCommand = new RelayCommand(param => setSingolaFotoWork(param), 
                                                                  param => true);
				}
				return _setSingolaFotoWorkCommand;
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
	}
}
