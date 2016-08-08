using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Model;
using System.IO;
using Digiphoto.Lumen.Applicazione;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Imaging.Wic;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.Pubblico.GestioneGeometria;
using System.Configuration;
using System.Windows;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Servizi;

namespace Digiphoto.Lumen.UI.Pubblico {

	public enum SlideShowStatus {
		Empty = 0,
		Running = 1,
		Stopped = 2
	};


	public class SlideShowViewModel : ClosableWiewModel, IContenitoreGriglia, IObserver<ScaricoFotoMsg>, IObserver<FotoModificateMsg> {

		private DispatcherTimer _orologio;


		public SlideShowViewModel() {

			pSSG = GestSlideShowViewModel.pSSG;

			_elencoSpots = caricaElencoSpot();

			slideShowRighe = Configurazione.LastUsedConfigLumen.slideShowNumRighe;
			slideShowColonne = Configurazione.LastUsedConfigLumen.slideShowNumColonne;
		}

		private List<string> _elencoSpots;
		private int _contaSchermate = 0;
		private int _indexSpotAttuale = 0;

		#region Proprietà

		public ParamSlideShowGeom pSSG {
			get;
			set;
		}

		private int numSlideCorrente {
			get;
			set;
		}

		public ObservableCollection<Fotografia> slidesVisibili {
			get;
			set;
		}

		private int totSlidesPerPagina {
			get {
				return slideShowColonne * slideShowRighe;
			}
		}

		/// <summary>
		/// Slide Show corrente
		/// </summary>
		public SlideShow slideShow {
			get;
			private set;
		}

		public bool isRunning {
			get {
				return (_orologio != null && _orologio.IsEnabled && isEmpty == false);
			}
		}

		public bool isPaused {
			get {
				return (isEmpty == false && isRunning == false);
			}
		}

		public bool isEmpty {
			get {
				return (slideShow == null || slideShow.slides.Count <= 0);
			}
		}

		public bool isLoaded {
			get {
				return !isEmpty;
			}
		}

		public bool possoApplicareWaterMark {
			get {
				// Anche se non c'entra molto mi aggancio alla configurazione di macchia provini
				// Eventualmente creare un flag di configurazione separato
				return Configurazione.UserConfigLumen.macchiaProvini;
			}
		}

		public string watermarkText {
			get {
				// Eventualmente leggere dalla configurazione.
				return "digiPHOTO.it";
			}
		}


		IFotoExplorerSrv fotoExplorerSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
			}
		}

		/// <summary>
		/// Questo flag mi dice se intanto che sta girando lo show, se si sono acquisite 
		/// delle nuove foto che potenzialmente devo andare a visualizzare.
		/// </summary>
		private bool sonoEntrateNuoveFotoNelFrattempo {
			get;
			set;
		}



		private short _slideShowRighe;
		public short slideShowRighe {
			get {
				return _slideShowRighe;
			}
			set {
				if( _slideShowRighe != value ) {
					_slideShowRighe = value;
					OnPropertyChanged( "slideShowRighe" );
				}
			}
		}

		private short _slideShowColonne;
		public short slideShowColonne {
			get {
				return _slideShowColonne;
			}
			set {
				if( _slideShowColonne != value ) {
					_slideShowColonne = value;
					OnPropertyChanged( "slideShowColonne" );
				}
			}
		}

		private bool _pubblicitaInCorso = false;
		public bool pubblicitaInCorso {
			get {
				return _pubblicitaInCorso;
			}
			set {
				if( _pubblicitaInCorso != value ) {
					_pubblicitaInCorso = value;
					OnPropertyChanged( "pubblicitaInCorso" );
					OnPropertyChanged( "slideShowInCorso" );
					OnPropertyChanged( "numFotoCorrente" );
				}
			}
		}

		public bool slideShowInCorso {
			get {
				return !pubblicitaInCorso;
			}
		}


		public string numFotoCorrente {
			get {
				return (slideShowInCorso && slidesVisibili.Count > 0) ? slidesVisibili [0].etichetta : null;
			}
		}

		#endregion   // Proprietà



		#region Metodi

		protected override void OnDispose() {

			try {
				if( _orologio != null ) {
					_orologio.Stop();
					_orologio.Tick -= orologio_Tick;
				}				
			} finally {
				base.OnDispose();
			}
		}

		protected override void OnRequestClose() {
			stop();  // Fermo lo slide show
			base.OnRequestClose();
		} 

		
		public void start() {

			// Preparo messaggio di cambio stato
			CambioStatoMsg msg = null;
			if( !isRunning ) {
				msg = new CambioStatoMsg( this ) {
					nuovoStato = (int)SlideShowStatus.Running
				};
			}

			_orologio.Start();

			// Rilancio messaggio di cambio stato
			if( msg != null )
				LumenApplication.Instance.bus.Publish( msg );


			// Se fosse aperta l'altra finestra pubblica, allora la chiudo
			((App)Application.Current).gestoreFinestrePubbliche.chiudiSnapshotPubblicoWindow();

			raiseCambioStatoProperties();
		}

		private void raiseCambioStatoProperties() {
			OnPropertyChanged( "isRunning" );
			OnPropertyChanged( "isPaused" );
			OnPropertyChanged( "isEmpty" );
			OnPropertyChanged( "isLoaded" );
			OnPropertyChanged( "pubblicitaInCorso" );
			OnPropertyChanged( "slideShowInCorso" );
			OnPropertyChanged( "numFotoCorrente" );
		}

		public void stop() {

			// Preparo messaggio di cambio stato
			CambioStatoMsg msg = null;
			if( isRunning ) {
				msg = new CambioStatoMsg( this ) {
					nuovoStato = (int)SlideShowStatus.Stopped
				};
			}

			_orologio.Stop();

			// Rilancio messaggio di cambio stato
			if( msg != null )
				LumenApplication.Instance.bus.Publish( msg );

			raiseCambioStatoProperties();
		}

		/// <summary>
		///  Fermo, svuoto ed azzero lo show
		/// </summary>
		public void reset() {

			// Preparo messaggio di cambio stato
			CambioStatoMsg msg = null;
			if( !isEmpty ) {
				msg = new CambioStatoMsg( this ) {
					nuovoStato = (int)SlideShowStatus.Empty
				};
			}

			stop();
			this.slideShow = null;
			rilasciaEdAzzeraVisibili();
			this.numSlideCorrente = 0;

			// Rilancio messaggio di cambio stato
			if( msg != null )
				LumenApplication.Instance.bus.Publish( msg );

			raiseCambioStatoProperties();
		}

		/// <summary>
		/// Se ho delle foto visibili, rilascio la memoria delle foto più pesanti
		/// poi svuoto la lista
		/// </summary>
		void rilasciaEdAzzeraVisibili() {
			
			if( slidesVisibili != null ) {

				// Devo rilasciare la memoria delle immagini pesanti precedenti prima di pulire
				foreach( var slideVisibile in slidesVisibili ) {
					AiutanteFoto.disposeImmagini( slideVisibile, IdrataTarget.Risultante );
					AiutanteFoto.disposeImmagini( slideVisibile, IdrataTarget.Originale );
				}

				slidesVisibili.Clear();
			}
		}



		public void creaShow( ParamCercaFoto paramCercaFoto ) {

			this.slideShow = new SlideShowAutomatico( paramCercaFoto );

			// Quindi devo eseguire la ricerca nuovamente (appunto perché nella gallery ho un sottoinsieme paginato)
			IRicercatoreSrv ricercaSrv = LumenApplication.Instance.getServizioAvviato<IRicercatoreSrv>();
			var fotografie = ricercaSrv.cerca( paramCercaFoto );
			this.slideShow.sostituisciFoto( fotografie );

			creaShow();
		}

		private void creaShow() {
			
			slideShow.millisecondiIntervallo = Configurazione.UserConfigLumen.millisIntervalloSlideShow;

			// Avvio il timer che serve a far girare le foto
			creaNuovoTimer();

			// Se lo show è automatico, devo ascoltare se arrivano nuove foto.
			if( slideShow is SlideShowAutomatico ) {
				IObservable<ScaricoFotoMsg> observable = LumenApplication.Instance.bus.Observe<ScaricoFotoMsg>();
				observable.Subscribe( this );
			}

			// Devo ascoltare sempre se qualche foto viene modificata
			IObservable<FotoModificateMsg> observableFM = LumenApplication.Instance.bus.Observe<FotoModificateMsg>();
			observableFM.Subscribe( this );

			raiseCambioStatoProperties();
		}

		/// <summary>
		/// creo uno slide show Manuale, con le immagini indicate
		/// </summary>
		public void creaShow( IList<Fotografia> newSlides )  {

			slideShow = new SlideShow( newSlides );
			creaShow();
		}

		/// <summary>
		/// Aggiunge le foto ad uno show esistente senza interromperlo
		/// </summary>
		/// <param name="newSlides"></param>
		public void add( IList<Fotografia> newSlides ) {

			bool isVuoto = (slideShow == null);
			if( isVuoto )
				this.slideShow = new SlideShow();
			slideShow.slides.AddRange( newSlides );

			if( isVuoto )
				creaShow();
		}

		private void creaNuovoTimer() {

			// Se avevo un timer precedete, lo distruggo. Poi ne creo un altro.
			if( _orologio != null )
				_orologio.Stop();

			_orologio = new DispatcherTimer();
			_orologio.Interval = new TimeSpan( 0,0,0,0, slideShow.millisecondiIntervallo );
			_orologio.Tick += new EventHandler( orologio_Tick );
		}

		#endregion


		
			
		private void orologio_Tick (object sender, EventArgs e) {
			

			if( eventualePubblicita() == true )
				return;
			
			// carico la collezione delle slide visibili andando avanti di una pagina
			if( slidesVisibili == null )
				slidesVisibili = new ObservableCollection<Fotografia>();
			else {
				// Prima di azzerare la lista, libero la memoria delle precedenti
				rilasciaEdAzzeraVisibili();
			}

			int conta = 0;

			do {

				// Se sono arrivato alla fine dello show, ricomincio da capo
				gestisciFineShow();

				if( numSlideCorrente < slideShow.slides.Count )
					this.slidesVisibili.Add( slideShow.slides [numSlideCorrente++] );
				else
					break;   // si vede che la lista è vuota lunga zero.

				++conta;

				// esco se ho finito la pagina, oppure se ho finito le foto
			} while( conta < totSlidesPerPagina && conta < slideShow.slides.Count );

			OnPropertyChanged( "slidesVisibili" );
			OnPropertyChanged( "numFotoCorrente" );

			// Le foto ritornate dal servizio di ricerca, non sono idratate. 
			// Quindi le idrato un pò alla volta quando passano da qui
			// Al primo giro sarà più lento perché le deve idratare per davvero. 
			// Dal secondo giro, invece non ci sarà più bisogno
			foreach( Fotografia f in slidesVisibili ) {

				try {
					AiutanteFoto.idrataImmaginiFoto( f, IdrataTarget.Provino );
				} catch( Exception ) {
					// Se la foto è rovinata, oppure inaccessibile, devo proseguire
				}
			}

			// Dopo che ho visualizzato le foto, se mi accorgo che il numero totale di foto da visualizzare 
			// è inferiore al numero massimo di foto che stanno nello show,
			// allora è inutile che lascio il timer acceso, tanto non ho altro da mostrare.
			if( slideShow.slides.Count <= totSlidesPerPagina )
				stop();
		}

		private void gestisciFineShow() {
			// Se arrivo al massimo, torno all'inizio
			if( numSlideCorrente >= slideShow.slides.Count ) {

				if( sonoEntrateNuoveFotoNelFrattempo && slideShow is SlideShowAutomatico ) {
					// Devo rieseguire la query
					ParamCercaFoto param = ((SlideShowAutomatico)slideShow).paramCercaFoto;
					creaShow( param );
					// Riazzero per prossimo test
					sonoEntrateNuoveFotoNelFrattempo = false;
				}

				numSlideCorrente = 0;
			}

		}

		List<string> caricaElencoSpot() {
			
			List<string> elenco = null;

			try {

				if( Configurazione.UserConfigLumen.intervalliPubblicita > 0 )
					elenco = Directory.EnumerateFiles( Configurazione.UserConfigLumen.cartellaPubblicita ).ToList();

			} catch (Exception ee )	{
				_giornale.Warn( "Impossibile caricare spot pubblicitari", ee );
			}

			return elenco;
		}




		/// <summary>
		/// Controllo se devo gestire la pubblicita.
		/// Se si, allora devo controllare che ad ogni intervallo prefissato, devo far partire uno spot.
		/// </summary>
		/// <returns>true se ho fatto lo spot</returns>
		private bool eventualePubblicita() {

			bool visualizzato = false;
			nomeFileSpotAttuale = null;

			if( !devoGestirePubblicita )
				return false;

			if( ++_contaSchermate > Configurazione.UserConfigLumen.intervalliPubblicita ) {

				pubblicitaInCorso = true;

				if( _indexSpotAttuale >= _elencoSpots.Count )
					_indexSpotAttuale = 0;

				nomeFileSpotAttuale = _elencoSpots.ElementAt( _indexSpotAttuale++ );

				_contaSchermate = 0;
				visualizzato = true;
			} else
				pubblicitaInCorso = false;

			return visualizzato;
		}

		string _nomeFileSpotAttuale;
		public string nomeFileSpotAttuale {
			get {
				return _nomeFileSpotAttuale;
			}
			set {
				if( _nomeFileSpotAttuale != value ) {
					_nomeFileSpotAttuale = value;
					OnPropertyChanged( "nomeFileSpotAttuale" );
				}
			}
		}
		public bool devoGestirePubblicita {
			get {
				return (Configurazione.UserConfigLumen.intervalliPubblicita > 0 && _elencoSpots != null && _elencoSpots.Count > 0);
			}
		}

		#region Interfaccia IContenitoreGriglia

		public short numRighe {
			get {
				return slideShowRighe;
			}
		}

		public short numColonne {
			get {
				return slideShowColonne;
			}
		}

		#endregion Interfaccia IContenitoreGriglia

		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( ScaricoFotoMsg value ) {

			// Qualcuno ha appena scaricato delle nuove foto nell'archivio.
			sonoEntrateNuoveFotoNelFrattempo = true;

			// Se avevo lo show fermo perchè foto non sufficienti, allora lo riaccendo
			if( slideShow is SlideShowAutomatico ) 
				if( slideShow != null && slideShow.slides != null && slideShow.slides.Count <= totSlidesPerPagina )
					if( isRunning == false )
						start();
		}


		public void OnNext( FotoModificateMsg fmMsg ) {

			if( slideShow == null )
				return;

			foreach( Fotografia modificata in fmMsg.fotos ) {

				int pos =  slideShow.slides.IndexOf( modificata );
				if( pos > 0 ) {
					AiutanteFoto.disposeImmagini( slideShow.slides[pos], IdrataTarget.Provino );

					// Se la foto è stata modificata, allora mi copio le correzioni.
					slideShow.slides[pos].correzioniXml = modificata.correzioniXml;
					// Se ho a  disposizione l'immagine del provino, me la copio, altrimenti me la rileggo da disco.
					if( modificata.imgProvino != null )
						slideShow.slides[pos].imgProvino = (Digiphoto.Lumen.Imaging.IImmagine)modificata.imgProvino;
					else
						AiutanteFoto.idrataImmaginiFoto( slideShow.slides[pos], IdrataTarget.Provino, true );
				}
			}
		}


	}
}
