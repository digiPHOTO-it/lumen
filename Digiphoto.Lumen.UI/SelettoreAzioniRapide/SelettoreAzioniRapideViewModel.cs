using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Media;
using Digiphoto.Lumen.Util;
using System.Windows;
using Digiphoto.Lumen.UI.Pubblico;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.UI.Main;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Digiphoto.Lumen.UI.PanAndZoom;

namespace Digiphoto.Lumen.UI
{
	public class SelettoreAzioniRapideViewModel : ViewModelBase, IObserver<GestoreCarrelloMsg>
	{
		private Boolean operazioniCarrelloBloccanti = false;

		public event Digiphoto.Lumen.UI.FotoRitocco.FotoRitoccoViewModel.EditorModeChangedEventHandler editorModeChangedEvent;

		public SelettoreAzioniRapideViewModel()
		{
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
			get;
			set;
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
				return myApp.slideShowViewModel;
			}
		}

/*
		private Transform _trasformazioneCorrente;
		public Transform trasformazioneCorrente
		{
			get
			{
				return _trasformazioneCorrente;
			}
			set
			{
				if (_trasformazioneCorrente != value)
				{
					_trasformazioneCorrente = value;
					OnPropertyChanged("trasformazioneCorrente");

					forseInizioModifiche();
				}
			}
		}
*/

		private ObservableCollection<Fotografia> _fotografieDaModificare;
		public ObservableCollection<Fotografia> fotografieDaModificare
		{
			get
			{
				return _fotografieDaModificare;
			}
			set
			{
				_fotografieDaModificare = value;
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
					return fotografieCW.SelectedItems.ToList();
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

		public bool possoSalvareCorrezioni
		{
			get
			{
				return modificheInCorso == true;
			}
		}

		/// <summary>
		/// Le foto selezionate sono in fase di modifica
		/// </summary>
		private bool _modificheInCorso;
		public bool modificheInCorso
		{
			get
			{
				return _modificheInCorso;
			}
			private set
			{
				if (_modificheInCorso != value)
				{
					_modificheInCorso = value;
					OnPropertyChanged("modificheInCorso");
				}
			}
		}

		public bool possoRifiutareCorrezioni
		{
			get
			{
				return possoSalvareCorrezioni;
			}
		}

		public bool possoApplicareCorrezione
		{
			get
			{
				return isAlmenoUnaSelezionata;
			}
		}

		ModalitaEdit _modalitaEdit;
		public ModalitaEdit modalitaEdit
		{
			get
			{
				return _modalitaEdit;
			}
			set
			{
				if (_modalitaEdit != value)
				{
					_modalitaEdit = value;
					OnPropertyChanged("modalitaEdit");
					OnPropertyChanged("isGestioneMaschereAttiva");
					OnPropertyChanged("isGestioneMaschereDisattiva");
					onEditorModeChanged(new EditorModeEventArgs(modalitaEdit));
				}
			}
		}

		public bool possoModificareConEditorEsterno
		{
			get
			{
				return isAlmenoUnaSelezionata &&
					   modalitaEdit == ModalitaEdit.FotoRitoccoPuntuale &&
					   (!modificheInCorso);
			}
		}

		#endregion Controlli


		#region Medoti

		/// <summary>
		/// Carico tutti i formati carta che sono abbinati alle stampanti installate
		/// Per ognuno di questi elementi, dovrò visualizzare un pulsante per la stampa
		/// </summary>
		private void caricaStampantiAbbinate()
		{
			string ss = Configurazione.UserConfigLumen.stampantiAbbinate;
			this.stampantiAbbinate = StampantiAbbinateUtil.deserializza(ss);
		}

		private void filtrareSelezionate(bool attivareFiltro)
		{

			// Alcune collezioni non sono filtrabili, per esempio la IEnumerable
			if (fotografieCW.CanFilter == false)
				return;

			if (attivareFiltro)
			{
				// Creo un oggetto Predicate al volo.
				fotografieCW.Filter = obj =>
				{
					return fotografieCW.SelectedItems.Contains(obj);
				};
			}
			else
			{
				fotografieCW.Filter = null;
			}
		}

		/// <summary>
		/// Aggiungo le immagini selezionate al masterizzatore
		/// </summary>
		/// <returns></returns>
		public void aggiungereAlMasterizzatore()
		{
			IEnumerable<Fotografia> listaSelez = fotoSelezionate;
			venditoreSrv.aggiungiMasterizzate(listaSelez);
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
						venditoreStampaDiretta.creaNuovoCarrello();
						venditoreStampaDiretta.carrello.intestazione = VenditoreSrvImpl.INTESTAZIONE_STAMPA_RAPIDA;
						venditoreStampaDiretta.aggiungiStampe(listaSelez, creaParamStampaFoto(stampanteAbbinata));
						if (venditoreStampaDiretta.vendereCarrello())
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
					venditoreSrv.aggiungiStampe(listaSelez, creaParamStampaFoto(stampanteAbbinata));
				}
				// Spengo tutto
				if (!singolaFotoWorks)
					deselezionareTutto();
			}
		}

		private void stampaRapida( StampanteAbbinata stampanteAbbinata, bool autoZoomNoBordiBianchi )
		{
			using (IVenditoreSrv venditoreSpampaRapida = LumenApplication.Instance.creaServizio<IVenditoreSrv>())
			{

				venditoreSpampaRapida.creaNuovoCarrello();
				venditoreSpampaRapida.carrello.intestazione = VenditoreSrvImpl.INTESTAZIONE_STAMPA_RAPIDA;
				venditoreSpampaRapida.aggiungiStampe( fotoSelezionate, creaParamStampaFoto( stampanteAbbinata, autoZoomNoBordiBianchi) );

				if (venditoreSpampaRapida.vendereCarrello())
				{
					// quando tutto va bene non diciamo niente. Segnaliamo solo gli errori.
					// dialogProvider.ShowMessage("Carrello venduto Correttamente", "Avviso");
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
			SelettoreMetadatiDialog s = new SelettoreMetadatiDialog(fotografieCW);

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
				foreach( Fotografia fDacanc in listaClone )
					fotografieCW.Remove( fDacanc );
				if(!singolaFotoWorks)
					deselezionareTutto();
			} else
				dialogProvider.ShowError( "Impossibile eliminare le foto indicate", "ERRORE", null );
		}

		private void ruotare(int pGradi)
		{
			addCorrezione(new Ruota() { gradi = pGradi });

			salvareCorrezioni();
		}

		private void tornareOriginale()
		{
			// per ogni foto elimino le correzioni e ricreo il provino partendo dall'originale.
			foreach (Fotografia f in fotoSelezionate)
				fotoRitoccoSrv.tornaOriginale(f);
		}

		void modificareConEditorEsterno()
		{
			try
			{
				isTuttoBloccato = true;

				// Accodo le stampe da modificare
				fotoRitoccoSrv.modificaConProgrammaEsterno(fotoSelezionate.ToArray());
			}
			finally
			{
				isTuttoBloccato = false;
			}
		}


		/// <summary>
		/// La prima volta che inizio a toccare una foto,
		/// devo salvarmi le correzioni attuali di tutte quelle che stanno per essere modificate.
		/// Mi serve per gestire eventuale rollback
		/// </summary>
		private void forseInizioModifiche()
		{
			if (!modificheInCorso)
			{
				// devo fare qualcosa al primo cambio di stato ?
			}
			modificheInCorso = true;
		}

		/// <summary>
		/// Aggiungo la correzione a tutte le foto selezionate
		/// </summary>
		private void addCorrezione(Correzione correzione)
		{

			forseInizioModifiche();

			foreach (Fotografia f in fotoSelezionate)
				fotoRitoccoSrv.addCorrezione(f, correzione);
		}

		private void rifiutareCorrezioni()
		{
			foreach (Fotografia f in fotoSelezionate)
				rifiutareCorrezioni(f, false);
			modificheInCorso = false;
		}

		/// <summary>
		/// Voglio rinunciare a modificare una foto e la tolgo anche dall'elenco di quelle in modifica.
		/// </summary>
		/// <param name="daTogliere"></param>
		internal void rifiutareCorrezioni(Fotografia daTogliere, bool toglila)
		{
			fotoRitoccoSrv.undoCorrezioniTransienti(daTogliere);
			if (toglila)
			{
				// La spengo
				fotografieCW.Deselect(daTogliere);
			}
		}

		void addFotoDaModificare(Fotografia f)
		{
			if (this.fotografieDaModificare.Contains(f) == false)
			{
				//				fotografieDaModificareCW.AddNewItem( f );
				this.fotografieDaModificare.Insert(0, f);

				AiutanteFoto.idrataImmaginiFoto(f, IdrataTarget.Provino);
			}
		}

		private void salvareCorrezioni()
		{
/*
			// Purtoppo anche la trasformazione di rotazione, è gestita a parte.
			if (trasformazioneCorrente is RotateTransform)
			{
				Ruota rc = new Ruota();
				rc.gradi = (float)((RotateTransform)trasformazioneCorrente).Angle;
				addCorrezione(rc);
			}
*/
			foreach (Fotografia f in fotoSelezionate)
				fotoRitoccoSrv.salvaCorrezioniTransienti(f);

			// Ora che ho persistito, concludo "dicamo cosi" la transazione, faccio una specie di commit.
			modificheInCorso = false;
		}

		private void caricareSlideShow(string modo)
		{
			((App)Application.Current).forseApriWindowPubblica();

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
			if(param.ToString().Equals("SINGOLA"))
			{
				singolaFotoWorks = true;
			}else if(param.ToString().Equals("MULTI")){
				singolaFotoWorks = false;
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
																		   );
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
					                                    param => possoStampare, false);
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
					                                        param => this.possoStampare, false);
				}
				return _stampaRapidaCommand;
			}
		}
		private RelayCommand _stampaRapidaBordiBianchiCommand;
		public ICommand stampaRapidaBordiBianchiCommand {
			get {
				if( _stampaRapidaBordiBianchiCommand == null ) {
					_stampaRapidaBordiBianchiCommand = new RelayCommand( param => stampaRapida( (StampanteAbbinata)param, false ),
															  param => this.possoStampare, false );
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
										  autoManual => possoCaricareSlideShow);
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
																  param => possoMandareInModifica);
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
															 p => isAlmenoUnaSelezionata, false);
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
														true);
				}
				return _ruotareCommand;
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
														true);
				}
				return _tornareOriginaleCommand;
			}
		}

		private RelayCommand _modificareConEditorEsternoCommand;
		public ICommand modificareConEditorEsternoCommand
		{
			get
			{
				if (_modificareConEditorEsternoCommand == null)
				{
					_modificareConEditorEsternoCommand = new RelayCommand(p => modificareConEditorEsterno(),
																		   p => possoModificareConEditorEsterno);
				}
				return _modificareConEditorEsternoCommand;
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
																true);
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
					_viewFotoFullScreenCommand = new RelayCommand(param => viewFotoFullScreen(), p => isAlmenoUnaSelezionata, false);
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
					_clonaFotografieCommand = new RelayCommand(param => clonaFotografie(), p => isAlmenoUnaSelezionata, null);
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
					_setSingolaFotoWorkCommand = new RelayCommand(param => setSingolaFotoWork(param), param => true, null);
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

		public void OnNext(StampatoMsg value)
		{
			if (value.lavoroDiStampa.esitostampa == EsitoStampa.Errore)
			{
				dialogProvider.ShowError("Stampa non Eseguita Correttamente", "Errore", null);
			}
		}

		#region Gestori Eventi

		protected void onEditorModeChanged(EditorModeEventArgs e)
		{
			if (editorModeChangedEvent != null)
				editorModeChangedEvent(this, e);
		}

		public void OnNext(Messaggio msg)
		{
			if (msg is FotoDaModificareMsg)
				gestisciFotoDaModificareMsg(msg as FotoDaModificareMsg);

			if (msg is NuovaFotoMsg)
				gestisciNuovaFotoMsg(msg as NuovaFotoMsg);

			if (msg is EliminateFotoMsg)
				gestisciFotoEliminate(msg as EliminateFotoMsg);

		}

		// Sono state eliminate delle foto. Se per caso le avevo in modifica, le devo togliere
		private void gestisciFotoEliminate(EliminateFotoMsg eliminateFotoMsg)
		{
			foreach (Fotografia ff in eliminateFotoMsg.listaFotoEliminate)
			{
				rifiutareCorrezioni(ff, true);
				fotografieDaModificare.Remove(ff);
			}
		}

		private void gestisciNuovaFotoMsg(NuovaFotoMsg nuovaFotoMsg)
		{
			if (AiutanteFoto.isMaschera(nuovaFotoMsg.foto))
			{
				// E' stata memorizzata una nuova fotografia che in realtà è una cornice
				addFotoDaModificare(nuovaFotoMsg.foto);

				// Visto che l'immagine del provino viene caricata in un altro thread, qui non sono in grado di visualizzarla. La devo rileggere per forza.
				// Questo mi consente di visualizzare il provino come primo elemento 
				AiutanteFoto.idrataImmaginiFoto(nuovaFotoMsg.foto, IdrataTarget.Provino, true);
			}
		}

		private void gestisciFotoDaModificareMsg(FotoDaModificareMsg fotoDaModificareMsg)
		{
			// Ecco che sono arrivate delle nuove foto da modificare
			// Devo aggiungerle alla lista delle foto in attesa di modifica.
			foreach (Fotografia f in fotoDaModificareMsg.fotosDaModificare)
				addFotoDaModificare(f);

			// Se richiesta la modifica immediata...
			if (fotoDaModificareMsg.immediata)
			{
				// ... e sono in modalità di fotoritocco
				if (this.modalitaEdit == ModalitaEdit.FotoRitoccoPuntuale)
				{
					// ... e non ho nessuna altra modifica in corso ...
					if (modificheInCorso == false)
					{
						fotografieCW.SelectedItems.Clear();
						foreach (Fotografia f in fotoDaModificareMsg.fotosDaModificare)
						{
							//Controllo se ho ragiunto il limite massimo di foto modificabili
							if (fotoDaModificareMsg.fotosDaModificare.Count == Configurazione.UserConfigLumen.maxNumFotoMod)
							{
								_giornale.Debug("Raggiunto limite massimo di foto modificabili di " + Configurazione.UserConfigLumen.maxNumFotoMod+" fotografie");
								break;
							}
							fotografieCW.SelectedItems.Add(f);
						}
						fotografieCW.Refresh();
					}
				}

				// Pubblico un messaggio di richiesta cambio pagina. Voglio andare sulla pagina del fotoritocco
				CambioPaginaMsg cambioPaginaMsg = new CambioPaginaMsg(this);
				cambioPaginaMsg.nuovaPag = "FotoRitoccoPag";
				LumenApplication.Instance.bus.Publish(cambioPaginaMsg);
			}
		}

		#endregion Gestori Eventi

		#endregion
	}
}
