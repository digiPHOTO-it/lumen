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

namespace Digiphoto.Lumen.UI {

	public class FotoGalleryViewModel : ViewModelBase {

		public FotoGalleryViewModel() {

			paramCercaFoto = new ParamCercaFoto();

			// Istanzio i ViewModel dei componenti di cui io sono composto
			selettoreEventoViewModel = new SelettoreEventoViewModel();

			selettoreFotografoViewModel = new SelettoreFotografoViewModel();

			

			if( IsInDesignMode ) {

			} else {

				//
				caricaStampantiAbbinate();
			}
		}


		/// <summary>
		/// Carico tutti i formati carta che sono abbinati alle stampanti installate
		/// Per ognuno di questi elementi, dovrò visualizzare un pulsante per la stampa
		/// </summary>
		private void caricaStampantiAbbinate() {

			using( IStampantiAbbinateSrv srv = LumenApplication.Instance.creaServizio<IStampantiAbbinateSrv>() ) {
				this.stampantiAbbinate = srv.stampantiAbbinate;
			}
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
				return fotografieCW.SelectedItems.Count > 0;
			}
		}

		public bool possoCaricareSlideShow {
			get {
				return fotografieCW != null && fotografieCW.SelectedItems.Count > 0;
			}
		}


		public bool possoAggiungereAlMasterizzatore {
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

		public SelettoreEventoViewModel selettoreEventoViewModel {
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

				if( slideShowViewModel.isRunning )
					uri = new Uri( uriTemplate.Replace( "##", "ssRunning" ) );

				if( slideShowViewModel.isPaused )
					uri = new Uri( uriTemplate.Replace( "##", "ssPause" ) );

				if( slideShowViewModel.isEmpty )
					uri = new Uri( uriTemplate.Replace( "##", "ssEmpty" ) );

				return new BitmapImage( uri );
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
				App myApp = (App)Application.Current;
				return myApp.slideShowViewModel;
			}
		}

		#endregion   // fasi del giorno

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

		#endregion   // Proprietà

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
				                                                         //  ,param => possoAggiungereAlMasterizzatore 
																		   );
				}
				return _aggiungereAlMasterizzatoreCommand;
			}
		}

		private RelayCommand _stampareCommand;
		public ICommand stampareCommand {
			get {
				if( _stampareCommand == null ) {
					_stampareCommand = new RelayCommand( param => stampare( param )
						//  ,param => possoAggiungereAlMasterizzatore 
																		   );
				}
				return _stampareCommand;
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
					_controllareSlideShowCommand = new RelayCommand( azione => controllareSlideShow( (string)azione ) );
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
			if( quante > 1 && Configurazione.modoVendita == ModoVendita.StampaDiretta ) {
				dialogProvider.ShowConfirmation( "Confermi la stampa di " + quante + " foto ?", "Richiesta conferma",
				  (confermato) => {
					  procediPure = confermato;
				  } );
			}

			if( procediPure ) {
				// Aggiungo al carrello oppure stampo direttamente
				venditoreSrv.aggiungiStampe( listaSelez, creaParamStampaFoto( stampanteAbbinata ) );
				
				// Spengo tutto
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

			ParamStampaFoto p = venditoreSrv.creaParamStampaFoto();

			p.nomeStampante = stampanteAbbinata.StampanteInstallata.NomeStampante;
			p.formatoCarta = stampanteAbbinata.FormatoCarta;
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


			completaParametriRicerca();

	//		paramCercaFoto.giornataIniz = Convert.ToDateTime( paramGiornataIniz );


			// Faccio una ricerca a vuoto
			fotoExplorerSrv.cercaFoto( paramCercaFoto );
			/*
						var query = from f in fotoExplorerSrv.fotografie
									select new DiapositivaViewModel( f );
						diapositiveViewModel = query.ToList<DiapositivaViewModel>();
			 */

			//			ObservableCollection<Fotografia> appo = new ObservableCollection<Fotografia>( fotoExplorerSrv.fotografie );

			fotografieCW = new MultiSelectCollectionView<Fotografia>( fotoExplorerSrv.fotografie );
// non dovrebbe essere necessario perchè è una collezione osservabile.
			OnPropertyChanged( "fotografieCW" );

			deselezionareTutto();
		}

		private void completaParametriRicerca() {

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

		}

		private void caricareSlideShow( string modo ) {

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
		}

		#endregion

	}
}
