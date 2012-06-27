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

namespace Digiphoto.Lumen.UI.Pubblico {

	public class SlideShowViewModel : ClosableWiewModel, IObserver<ScaricoFotoMsg> {

		private DispatcherTimer _orologio;

		public SlideShowViewModel() {
			// La dimensione delle foto deve essere calcolata in automatico in base alle dimensione del canvas che le contiene
			dimensioneIconaFoto = double.NaN;
		}

		#region Proprietà

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
				return slideShow.colonne * slideShow.righe;
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


		public short slideShowRighe {
			get {
				return slideShow != null ? slideShow.righe : (short)1;
			}
			set {
				if( slideShow.righe != value ) {
					slideShow.righe = value;
					OnPropertyChanged( "slideShowRighe" );
				}
			}
		}

		public short slideShowColonne {
			get {
				return slideShow != null ? slideShow.colonne : (short)2;
			}
			set {
				if( slideShow.colonne != value ) {
					slideShow.colonne = value;
					OnPropertyChanged( "slideShowColonne" );
				}
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
			_orologio.Start();
		}

		public void stop() {
			_orologio.Stop();
		}

		/// <summary>
		///  Fermo, svuoto ed azzero lo show
		/// </summary>
		public void reset() {
			stop();
			this.slideShow = null;
			this.slidesVisibili.Clear();
			this.numSlideCorrente = 0;
		}

		public void creaShow( ParamCercaFoto paramCercaFoto ) {

			// Mi f accio dare le foto dal servizio
			fotoExplorerSrv.cercaFoto( paramCercaFoto );

			this.slideShow = new SlideShowAutomatico( paramCercaFoto );
			this.slideShow.slides = fotoExplorerSrv.fotografie;
			creaShow();
		}

		private void creaShow() {
			// Qualche parametro di default
			slideShow.colonne = 2;	// TODO deve sceglierle l'utente
			slideShow.righe = 1;	// TODO deve sceglierle l'utente
			slideShow.millisecondiIntervallo = Configurazione.UserConfigLumen.millisIntervalloSlideShow;

			// Avvio il timer che serve a far girare le foto
			creaNuovoTimer();

			// Se lo show è automatico, devo ascoltare se arrivano nuove foto.
			if( slideShow is SlideShowAutomatico ) {
				IObservable<ScaricoFotoMsg> observable = LumenApplication.Instance.bus.Observe<ScaricoFotoMsg>();
				observable.Subscribe( this );
			}				
		}

		/// <summary>
		/// creo uno slide show Manuale, con le immagini indicate
		/// </summary>
		public void creaShow( IList<Fotografia> newSlides )  {
			this.slideShow = new SlideShow() { 
				slides = newSlides 
			};
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

			// carico la collezione delle slide visibili andando avanti di una pagina

			if( slidesVisibili == null )
				slidesVisibili = new ObservableCollection<Fotografia>();
			else
				slidesVisibili.Clear();

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

			// Le foto ritornate dal servizio di ricerca, non sono idratate. 
			// Quindi le idrato un pò alla volta quando passano da qui
			// Al primo giro sarà più lento perché le deve idratare per davvero. 
			// Dal secondo giro, invece non ci sarà più bisogno
			foreach( Fotografia f in slidesVisibili )
				AiutanteFoto.idrataImmaginiFoto( f, IdrataTarget.Provino );


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
	}
}
