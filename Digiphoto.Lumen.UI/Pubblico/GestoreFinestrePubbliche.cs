using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.Pubblico.GestioneGeometria;
using Digiphoto.Lumen.UI.ScreenCapture;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Imaging.Wic;
using log4net;
using System.Windows.Forms;

namespace Digiphoto.Lumen.UI.Pubblico {


	/// <summary>
	/// Questa classe si occupa di mantenere lo stato delle finestre pubbliche, e di sollevare eventi
	/// quando queste vengono aperte o chiuse
	/// </summary>
	public class GestoreFinestrePubbliche {

		protected static readonly ILog _giornale = LogManager.GetLogger( typeof( GestoreFinestrePubbliche ) );

		/// <summary>
		/// Finestra modale in cui gira lo slide show
		/// </summary>
		private SlideShowWindow _slideShowWindow;

		/// <summary>
		/// Finestra modale dove si vedono replicate le foto della gallery per la vendita al pubblico
		/// </summary>
		private PubblicoWindow _pubblicoWindow;

		private SnapshotPubblicoWindow _snapshotPubblicoWindow;
		private MainWindow _mainWindow;
		
		
		public GestoreFinestrePubbliche() {

			// Assegno geometria iniziale
			this.geomSS = Configurazione.UserConfigLumen.geometriaFinestraSlideShow;
		}


		public SlideShowViewModel slideShowViewModel {
			get {
				return _slideShowWindow == null ? null : (SlideShowViewModel)_slideShowWindow.DataContext;
			}
		}

		public FotoGalleryViewModel fotoGalleryViewModel {
			get {
				return _mainWindow == null ? null : (FotoGalleryViewModel)_mainWindow.fotoGallery.DataContext;
			}
		}

		public SnapshotPubblicoViewModel snapshotPubblicoViewModel {
			get {
				return _snapshotPubblicoWindow == null ? null : (SnapshotPubblicoViewModel)_snapshotPubblicoWindow.DataContext;
			}
		}

		// Posiziono la finestra rileggendo i valori dalla configurazione memorizzata
		internal void ripristinaFinestraSlideShow() {

			chiudereFinestraSlideShow();

			this.geomSS = (GeometriaFinestra) Configurazione.UserConfigLumen.geometriaFinestraSlideShow.Clone();

			aprireFinestraSlideShow();
		}

		public bool isSlideShowVisible { 
			get {
				return _slideShowWindow != null && _slideShowWindow.WindowState != WindowState.Minimized;
			}
		}

		public bool isPubblicoVisible {
			get {
				return _pubblicoWindow != null && _pubblicoWindow.WindowState != WindowState.Minimized;
			}
		}

		/// <summary>
		///  Se lo SS è visibile, mi indica in quale schermo è posizionato
		/// </summary>
		/// <returns>-1 se non è visibile, altrimenti il numero dello schermo</returns>
		public WpfScreen getScreenSlideShow() {
			if( isSlideShowVisible ) {
				return WpfScreen.GetScreenFrom( _slideShowWindow );
			} else
				return null;
		}

		/// <summary>
		/// Questa è la posizione della finestra SS.
		/// Inizialmente viene presa dalla configurazione, ma poi può essere modificata
		/// dallo spostamento della finestra, oppure dalle operazioni dell'utente nel menu preferenze.
		/// </summary>
		public GeometriaFinestra geomSS { 
			get; 
			private set; 
		}

		public void aprireFinestraMain() {
			_mainWindow = new MainWindow();
			_mainWindow.Show();			
		}

		/// <summary>
		/// Stiamo per uscire. chiudo tutto
		/// </summary>
		public void chiudereTutteLeFinestre() {

			chiudereFinestraSlideShow();
			chiudereFinestraPubblico();
			chiudereFinestraSnapshotPubblico();
			chiuidereFinestraMain();

			System.Windows.Application.Current.Shutdown();
		}

		public bool possoMassimizzareSlideShow { 
			get {
				return isSlideShowVisible && _slideShowWindow.WindowState != WindowState.Maximized;
			}
		}

		public void massimizzareFinestraSlideShow() {
			massimizzareFinestra( _slideShowWindow );
		}

		/// <summary>
		/// Mi serve per capire se la chiusura di una finestra è dovuta ad una azione che
		/// sto facendo da qui,
		/// oppure uno ha proprio chiuso la finestra con la "X"
		/// </summary>
		private bool azioneInCorso {
			get;
			set;
		}

		/// <summary>
		/// Posiziono la finestra dello slide show sul monitor primario,
		/// con una dimensione che sicuramente è visibile (cioè con una geometria di default)
		/// </summary>
		internal void normalizzareFinestraSlideShowSulMonitor1() {

			// Per prima cosa apro la finestra se questa è chiusa
			aprireFinestraSlideShow();
			
			// Creo una geometria dalle dimensioni del monitor.
			GeometriaFinestra geo = Configurazione.creaGeometriaSlideShowDefault();
			posizionaFinestra( _slideShowWindow, geo );
		}


		/// <summary>
		/// Massimizzo la finestra indicata sul monitor secondario
		/// </summary>
		internal void massimizzareFinestraSlideShowSulMonitor2() {

			// Adesso calcolo la posizione del monitor 2
			WpfScreen scrn = WpfScreen.AllScreens().Where( s => s.IsPrimary == false ).FirstOrDefault();
			if( scrn != null ) {

				// Per prima cosa apro la finestra se questa è chiusa
				aprireFinestraSlideShow();

				// Creo una geometria dalle dimensioni del monitor.
				GeometriaFinestra geo = creaGeometriaDaScreen( scrn );
				posizionaFinestra( _slideShowWindow, geo );

				massimizzareFinestraSlideShow();
			} else {
				_giornale.Warn( "E' stato chiesto di usare il monitor secondario ma questo non è presente" );
			}

		}

		internal static void massimizzareFinestra( Window window ) {
			window.WindowState = WindowState.Maximized;
		}

		public void chiudereFinestraSlideShow() {

			if( _slideShowWindow != null ) {

				ClosableWiewModel cvm = (ClosableWiewModel)slideShowViewModel;
				Object param = null;
				if( cvm.CloseCommand.CanExecute( param ) )
					cvm.CloseCommand.Execute( param );
				_slideShowWindow = null;
			}

		}

		private void chiuidereFinestraMain() {
			if( _mainWindow != null ) {

				ClosableWiewModel cvm = (ClosableWiewModel)_mainWindow.DataContext;
				Object param = null;
				if( cvm.CloseCommand.CanExecute( param ) )
					cvm.CloseCommand.Execute( param );
				_mainWindow = null;
			}
		}

		public void azionePosizionamentoIniziale() {

			// Apro la finestra della gallery pubblica, e mi memorizzo il flag che mi dice se era aperta.
			aprireFinestraPubblico();
			stavaGirandoPubblico = true;

		}

		/// <summary>
		/// Avvio il carosello
		/// </summary>
		public void azioneAvvioSlideShow() {

			try {
				azioneInCorso = true;

				// Se la finestra pubblica è aperta, la chiudo perché tanto parte lo show
				// siccome le foto sotto non si vedrebbero, chiudo la finestra così risparmio un pò di risorse di sistema (cpu,ram,hd)
				if( stavaGirandoPubblico )
					chiudereFinestraPubblico();

				// avvio carosello
				slideShowViewModel.start();

				// porto in primo piano la finestra perché non si sa mai
				_slideShowWindow.Topmost = true;

				stavaGirandoLoSlideShow = true;

			} catch( Exception ) {
				throw;
			} finally {
				azioneInCorso = false;
            }
		}
		
		/// <summary>
		/// Fermo il carosello, e mi rimetto in modalità gallery con il form Pubblico
		/// </summary>
		public void azioneFermaSlideShow() {

			try {
				azioneInCorso = true;

				slideShowViewModel.stop();
				_slideShowWindow.Topmost = false;
				stavaGirandoLoSlideShow = false;

				if( stavaGirandoPubblico ) {
					aprireFinestraPubblico();
					_pubblicoWindow.Topmost = true;
				}

			} catch( Exception ) {

				throw;
			} finally {
				azioneInCorso = false;
			}




		}

		public void chiudereFinestraSnapshotPubblico() {

			if( _snapshotPubblicoWindow != null ) {
				ClosableWiewModel cvm = (ClosableWiewModel)snapshotPubblicoViewModel;
				Object param = null;
				if( cvm.CloseCommand.CanExecute( param ) )
					cvm.CloseCommand.Execute( param );
				_snapshotPubblicoWindow = null;

				// Se quando mi sono aperta, stava girando lo ss, allora lo avevo messo in pausa,
				// quindi adesso lo riattivo.
				if( stavaGirandoLoSlideShow ) {
					if( slideShowViewModel != null && slideShowViewModel.isPaused )
						slideShowViewModel.start();
				}
				stavaGirandoLoSlideShow = false;
			}
		}

		public void chiudereFinestraPubblico() {

			if( _pubblicoWindow != null ) {

				// Il viewmodel della finestra pubblica è quello della gallery quindi non posso chiuderlo.
				// Chiudo solo la finestra
				_pubblicoWindow.Close();
				_pubblicoWindow = null;
			}

		}

		/// <summary>
		/// Se la finestra del pubblico non è aperta (o non è istanziata)
		/// la creo sul momento
		/// </summary>
		public bool aprireFinestraPubblico() {

			// Se è già aperta, non faccio niente
			if( _pubblicoWindow != null )
				return false;

			IInputElement elementoKeyboardInfuocato = Keyboard.FocusedElement;

			// Creo
			_pubblicoWindow = new PubblicoWindow();

			// Posiziono
			posizionaFinestraPubblico();

			// Gestisco la chiusura per il rilascio del vm
			_pubblicoWindow.Closed += chiusoPubblicoWindow;

			// Visualizzo modeless
			_pubblicoWindow.Show();
			_pubblicoWindow.Topmost = true;

			// Devo rimenttere il fuoco sul componente che lo deteneva prima
			Keyboard.Focus( elementoKeyboardInfuocato );

			// Uso lo stesso viewmodel della gallery perché deve lavorare identico
			_pubblicoWindow.DataContext = fotoGalleryViewModel;

			return true;
		}

		/// <summary>
		/// Se la finestra dello slide show non è aperta (o non è istanziata)
		/// la creo sul momento
		/// </summary>
		public bool aprireFinestraSlideShow() {

			// Se è già aperta, non faccio niente
			if( _slideShowWindow != null )
				return false;

			IInputElement elementoKeyboardInfuocato = Keyboard.FocusedElement;

			// Creo
			_slideShowWindow = new SlideShowWindow();

			// Posiziono
			posizionaFinestraSlideShow();

			// Gestisco la chiusura per il rilascio del vm
			_slideShowWindow.Closed += chiusoSlideShowWindow;

			// Visualizzo modeless
			_slideShowWindow.Show();

			// Devo rimenttere il fuoco sul componente che lo deteneva prima
			Keyboard.Focus( elementoKeyboardInfuocato );

			return true;
		}

		/// <summary>
		/// Se la finestra dello slide show non è aperta (o non è istanziata)
		/// la creo sul momento
		/// </summary>
		private bool forseApriSnapshotPubblicoWindow() {

			// Se è già aperta, non faccio niente
			if( _snapshotPubblicoWindow != null )
				return false;

			IInputElement elementoKeyboardInfuocato = Keyboard.FocusedElement;

			// Apro la finestra modeless
			// Create a window and make this window its owner
			_snapshotPubblicoWindow = new SnapshotPubblicoWindow();
			_snapshotPubblicoWindow.Closed += chiusoSnapshotPubblicoWindow;

			if( _slideShowWindow != null ) {

				// Se ho lo ss attivo, allora devo prendere il suo posto
				if( _slideShowWindow.WindowState == WindowState.Maximized ) {
					// In questo caso non posso massimizzare a mia volta :-(
					// Se sono sul secondo monitor, devo lavorare come se fosse in normal
					WpfScreen scrn = WpfScreen.GetScreenFrom( _slideShowWindow );
					_snapshotPubblicoWindow.Height = scrn.WorkingArea.Height;
					_snapshotPubblicoWindow.Width = scrn.WorkingArea.Width;
					_snapshotPubblicoWindow.Top = scrn.WorkingArea.Top;
					_snapshotPubblicoWindow.Left = scrn.WorkingArea.Left;
				} else {
					_snapshotPubblicoWindow.Height = _slideShowWindow.Height;
					_snapshotPubblicoWindow.Width = _slideShowWindow.Width;
					_snapshotPubblicoWindow.Top = _slideShowWindow.Top;
					_snapshotPubblicoWindow.Left = _slideShowWindow.Left;
				}

				_snapshotPubblicoWindow.WindowState = WindowState.Normal;

			} else {

				// valorizzo la geometria del form = a quella del form slide show (praticamente lo ricopro)
				_snapshotPubblicoWindow.Height = Configurazione.UserConfigLumen.geometriaFinestraSlideShow.Height;
				_snapshotPubblicoWindow.Width = Configurazione.UserConfigLumen.geometriaFinestraSlideShow.Width;
				_snapshotPubblicoWindow.Top = Configurazione.UserConfigLumen.geometriaFinestraSlideShow.Top;
				_snapshotPubblicoWindow.Left = Configurazione.UserConfigLumen.geometriaFinestraSlideShow.Left;
				/*
				if( Configurazione.UserConfigLumen.fullScreen )
					_snapshotPubblicoWindow.WindowState = WindowState.Maximized;
				else
					_snapshotPubblicoWindow.WindowState = WindowState.Normal;
				 * */
			}

			_snapshotPubblicoWindow.Show();
			_snapshotPubblicoWindow.Topmost = true;

			// Riposiziono il fuoco sul componente che lo deteneva prima di aprire la finestra
			Keyboard.Focus( elementoKeyboardInfuocato );
			
			if( Configurazione.UserConfigLumen.geometriaFinestraSlideShow.fullScreen )
				_snapshotPubblicoWindow.WindowState = WindowState.Maximized;
			else
				_snapshotPubblicoWindow.WindowState = WindowState.Normal;

			return true;
		}


		/// <summary>
		/// Quando apro la snapshot pubblica, siccome deve andare sopra allo slideshow, allora lo fermo.
		/// </summary>
		private bool stavaGirandoLoSlideShow {
			get;
			set;
		}

		private bool stavaGirandoPubblico {
			get;
			set;
		}

		/// <summary>
		/// Quando lavoro con una singola foto, se vado sullo schermo del pubblico,
		/// devo fare vedere la foto più bella che ho (cioè quella grande)
		/// Il problema è che la foto grande, potrebbe ancora non essere stata calcolata
		/// </summary>
		/// <param name="fotografia"></param>
		/// 
		public void eseguiSnapshotSuFinestraPubblica( Fotografia fotografia, bool forzaAperturaWin ) {

			// Se la finestra è chiusa e il flag non mi forza l'apertura non faccio niente
			if( !forzaAperturaWin && _snapshotPubblicoWindow == null )
				return;
			
            FotoDisposeUtils.Instance().DisposeFotografia(fotografia);

			IdrataTarget quale = AiutanteFoto.qualeImmagineDaStampare( fotografia );

			AiutanteFoto.idrataImmagineDaStampare( fotografia );

			IImmagine immagine = AiutanteFoto.getImmagineFoto( fotografia, quale );

			forseApriSnapshotPubblicoWindow();
			
			snapshotPubblicoViewModel.snapshotImageSource = ((ImmagineWic)immagine).bitmapSource;
        }

		public void eseguiSnapshotSuFinestraPubblica( Visual sourceVisual, Visual targetVisual ) {
			eseguiSnapshotSuFinestraPubblica( sourceVisual, targetVisual, true );
		}

		/// <summary>
		/// Se è aperto lo slide show, allora lo metto in pausa e visualizzo la finestra nel suo posto preciso (stessa geometria).
		/// Se invece lo slide show è chiuso, allora apro la finestra nella posizione che mi ero memorizzata.
		/// 
		/// </summary>
		/// <param name="sourceVisual">Questa è la Window di riferimento, cioè quella che viene usata per parametrare le dimensioni dell'oggetto</param>
		/// <param name="targetVisual">Oggetto visuale da "fotografare"</param>

		public void eseguiSnapshotSuFinestraPubblica( Visual sourceVisual, Visual targetVisual, bool forzaAperturaWin ) {

			// Se la finestra è chiusa e il flag non mi forza l'apertura non faccio niente
			if( !forzaAperturaWin && _snapshotPubblicoWindow == null )
				return;

			if( _snapshotPubblicoWindow == null ) {
				stavaGirandoLoSlideShow = slideShowViewModel == null ? false : slideShowViewModel.isRunning;

				if( stavaGirandoLoSlideShow ) {
					// Fermo lo slide-show. Lo lascio in pausa. Poi quando chiuderò la finestra lo faccio ripartire.
					slideShowViewModel.stop();
				}
			}

			forseApriSnapshotPubblicoWindow();

			snapshotPubblicoViewModel.snapshotImageSource = SnapshotUtil.WindowSnapshotToImage( sourceVisual, targetVisual );
		}

		public void chiusoSlideShowWindow( object sender, EventArgs e ) {
			if (_slideShowWindow!=null)
			{
				_slideShowWindow.Closed -= chiusoSlideShowWindow;
				_slideShowWindow = null;
			}

			if( !azioneInCorso )
				stavaGirandoLoSlideShow = false;

			FormuleMagiche.attendiGcFinalizers();
        }

		public void chiusoPubblicoWindow( object sender, EventArgs e ) {
			if( _pubblicoWindow != null ) {
				_pubblicoWindow.Closed -= chiusoPubblicoWindow;
				_pubblicoWindow = null;

			}
			if( !azioneInCorso )
				stavaGirandoPubblico = false;

			FormuleMagiche.attendiGcFinalizers();
		}


		public void chiusoSnapshotPubblicoWindow( object sender, EventArgs e ) {
			_snapshotPubblicoWindow.Closed -= chiusoSnapshotPubblicoWindow;
			_snapshotPubblicoWindow = null;

			// Se ho aperto lo ss, lo riporto in primo piano.
			if( _slideShowWindow != null )
				_slideShowWindow.Topmost = true;

            FormuleMagiche.attendiGcFinalizers();
		}


		/// <summary>
		/// Posiziono la finestra e la ridimensiono, per come indicato nella nuova geometria desiderata.
		/// Verifico anche che la geometria indicata sia proiettabile nello schermo (quindi che si interna)
		/// Per gestire lo stato di massimizzato, vedere descrizione qui:
		/// https://social.msdn.microsoft.com/Forums/vstudio/en-US/2ca2fab6-b349-4c08-915f-373c71bd636a/show-and-maximize-wpf-window-on-a-specific-screen?forum=wpf
		/// In pratica: 
		/// in prima battuta visualizzo la finestra in stato normal. Poi dopo che questa si è caricata e visualizzata,
		/// tramite un evento, faccio chiamare una routine che la rende massimizzata.
		/// </summary>
		/// <param name="window">La finestra da ridimensionare/spostare</param>
		/// <param name="newGoem">La nuova posizione e dimensione desiderata</param>
		/// <returns>true se operazione a buon fine. false se condizioni errate</returns>

		public static bool posizionaFinestra( Window window, GeometriaFinestra newGoem ) {

			bool esito = verificaProiettabile( newGoem );

			if( esito ) {

				try {

					// In prima battuta, devo visualizzare la finestra in stato normal
					window.WindowState = WindowState.Normal;

					// Nessun intervento di centratura automatico. Faccio tutto io manualmente su tutti i parametri
					window.WindowStartupLocation = WindowStartupLocation.Manual;

					// Screen[] _screens = Screen.AllScreens;
					// System.Drawing.Rectangle rectangle1 = _screens[newGoem.deviceEnum].Bounds;
					// System.Drawing.Rectangle rectangle2 = _screens[newGoem.deviceEnum].WorkingArea;

					window.Left = newGoem.Left;
					window.Top = newGoem.Top;
					window.Width = newGoem.Width;
					window.Height = newGoem.Height;

					if( newGoem.fullScreen )
						window.Loaded += Window_Loaded_posizionaFinestraFullScreen;

					esito = true;

				} catch( Exception ) {
					esito = false;
				}
			}

			return esito;
		}

		/// <summary>
		/// Massimizzo la finestra solo dopo che questa è stata caricata
		/// </summary>
		/// <param name="sender">la finestra sorgente</param>
		/// <param name="e">evento di loaded</param>
		private static void Window_Loaded_posizionaFinestraFullScreen( object sender, RoutedEventArgs e ) {

			SlideShowWindow ssWindow = sender as SlideShowWindow;

			// Salvo il flag
			bool savePos = ssWindow.posizionamentoInCorso;
			ssWindow.posizionamentoInCorso = true;

			massimizzareFinestra( ssWindow );

			ssWindow.posizionamentoInCorso = savePos;
        }

		public bool posizionaFinestraSlideShow() {

			// carico la eventuale geometria salvata nella configurazione
			_slideShowWindow.posizionamentoInCorso = true;

			// Non ho ancora salvato una geometria. Ne creo una che va sicuramente bene
			if( geomSS == null || geomSS.isEmpty() )
				geomSS = Configurazione.creaGeometriaSlideShowDefault();

			// Primo tentativo
			bool esito;
			esito = posizionaFinestra( _slideShowWindow, geomSS );

			// Secondo tentativo con default
			if( !esito ) {
				geomSS = Configurazione.creaGeometriaSlideShowDefault();
				esito = posizionaFinestra( _slideShowWindow, geomSS );
			}

			_slideShowWindow.posizionamentoInCorso = false;

			return esito;
		}

		public bool posizionaFinestraPubblico() {

			// carico la eventuale geometria salvata nella configurazione
			// _pubblicoWindow.posizionamentoInCorso = true;

			// Come concordato con Ciccio, la finestra pubblico deve avere la stessa posizione di quella dello Slide Show
			// Quindi non ha una sua propria configurazione.
			GeometriaFinestra geomPub = (geomSS == null || geomSS.isEmpty()) ? Configurazione.creaGeometriaSlideShowDefault() : geomSS;

			// Primo tentativo
			bool esito;
			esito = posizionaFinestra( _pubblicoWindow, geomPub );

			// Secondo tentativo con default
			if( !esito ) {
				geomPub = Configurazione.creaGeometriaSlideShowDefault();
				esito = posizionaFinestra( _pubblicoWindow, geomPub );
			}

			// _pubblicoWindow.posizionamentoInCorso = false;

			return esito;
		}


		public static GeometriaFinestra creaGeometriaDaScreen( WpfScreen wpfScreen ) {

			// Creo una geometria dalle dimensioni del monitor.
			return new GeometriaFinestra() {
				Left = (int)wpfScreen.WorkingArea.Left,
				Top = (int)wpfScreen.WorkingArea.Top,
				Width = (int)wpfScreen.WorkingArea.Width,
				Height = (int)wpfScreen.WorkingArea.Height,

				fullScreen = false,  // ?? questo effettivamente non è un valore dello schermo ma della finestra
				deviceEnum = wpfScreen.deviceEnum
			};
		}

		/// <summary>
		/// Data una Window, estraggo i dati del suo posizionamento.
		/// </summary>
		/// <param name="window">La finestra da analizzare</param>
		/// <returns>La Geometria che rappresenta la sua posizione</returns>
		public static GeometriaFinestra creaGeometriaDaWindow( Window window ) {

			WpfScreen scr = WpfScreen.GetScreenFrom( window );

			GeometriaFinestra geo = new GeometriaFinestra();

			geo.Left = (int)window.Left;
			geo.Top = (int)window.Top;


			// In certi casi il valore è NaN
			// http://stackoverflow.com/questions/11013316/get-the-height-width-of-window-wpf
			if( Double.IsNaN( window.Width ) )
				geo.Width = (int)window.ActualWidth;
			else
				geo.Width = (int)window.Width;

			// In certi casi il valore è NaN
			// http://stackoverflow.com/questions/11013316/get-the-height-width-of-window-wpf
			if( Double.IsNaN( window.Height ) )
				geo.Height = (int)window.ActualHeight;
			else
				geo.Height = (int)window.Height;



			geo.Height = (int)window.Height;
			geo.Width = (int)window.Width;

			geo.fullScreen = window.WindowState == WindowState.Maximized;
			geo.deviceEnum = scr.deviceEnum;

			return geo;
        }


		/// <summary>
		/// Aggiorno solo in memoria (non persisto sulla configurazione)
		/// </summary>
		internal void memorizzaGeometriaFinestraSlideShow() {

			WpfScreen scrn = WpfScreen.GetScreenFrom( _slideShowWindow );

			this.geomSS = creaGeometriaDaWindow( _slideShowWindow );
		}

		/// <summary>
		/// Salvo la posizione e la dimensione della finestra dello SlideShow.
		/// I dati vengono persistiti nella configurazione utente.
		/// </summary>
		public bool salvaGeometriaFinestraSlideShow() {

			bool esito;

			if( this.geomSS == null || this.geomSS.isEmpty() ) {
				esito = false;

			} else {

				// Questo modifica geomSS ...
				memorizzaGeometriaFinestraSlideShow();

				// ... Questo lo persiste
				Configurazione.UserConfigLumen.geometriaFinestraSlideShow = this.geomSS;

				_giornale.Debug( "Devo salvare la configurazione utente su file xml" );
				UserConfigSerializer.serializeToFile( Configurazione.UserConfigLumen );
				_giornale.Info( "Salvata la configurazione utente su file xml" );

				esito = true;
			}

			return esito;
		}

		/// <summary>
		/// Uso lo schermo indicato nella geometria stessa
		/// </summary>
		/// <param name="geom"></param>
		/// <returns></returns>
		public static bool verificaProiettabile( GeometriaFinestra geom ) {

			// Verifico se lo schermo indicato esiste ancora. Potrebbe essere stato scollegato
			if( geom.deviceEnum >= 0 && geom.deviceEnum < Screen.AllScreens.Count() )
				return verificaProiettabile( WpfScreen.GetScreenFrom( geom.deviceEnum ), geom );
			else
				return false;
		}

		/// <summary>
		/// Controllo se la geometria indicata è "interna" allo schermo indicato.
		/// 
		/// </summary>
		/// <param name="scr">Schermo su cui disegnare. Esiste di sicuro</param>
		/// <param name="gf">Geometria della finestra da aprire nello schermo</param>
		/// <returns>true se la finestra è interna allo schermo (quindi ci sta)</returns>
		public static bool verificaProiettabile( WpfScreen scr, GeometriaFinestra gf ) {

			bool proiettabile = true;
			
			if( gf.fullScreen ) {
				return true;
			}

			if( proiettabile && gf.isEmpty() )
				proiettabile = false;

			if( proiettabile && gf.Left < 0 )
				proiettabile = false;

			if( proiettabile && gf.Top < 0 )
				proiettabile = false;

			if( proiettabile && gf.Width <= 10 )
				proiettabile = false;

			if( proiettabile && gf.Height <= 10 )
				proiettabile = false;

			if( proiettabile && gf.deviceEnum == WpfScreen.Primary.deviceEnum && (gf.Height + gf.Top > scr.WorkingArea.Height || gf.Width + gf.Left > scr.WorkingArea.Width) )
				proiettabile = false;

			if( proiettabile && gf.deviceEnum != WpfScreen.Primary.deviceEnum && (gf.Height + gf.Top - scr.DeviceBounds.Y > scr.WorkingArea.Height || gf.Width + gf.Left - scr.DeviceBounds.X > scr.WorkingArea.Width) )
				proiettabile = false;

			return proiettabile;
		}

		public void azioneResetSlideShow() {

			slideShowViewModel.reset();

			if( stavaGirandoPubblico )
				aprireFinestraPubblico();
		}

	}

	public class FotoDisposeUtils
    {
        private static FotoDisposeUtils _instance = null;

        private Fotografia _fotografiaCorrente = null;
        private FotoDisposeUtils()
        {

        }

        public static FotoDisposeUtils Instance()
        {
            if (_instance == null)
            {
                _instance = new FotoDisposeUtils();
            }
            return _instance;
        }

        public void DisposeFotografia(Fotografia fotografia)
        {
            if (_fotografiaCorrente == null)
            {
                _fotografiaCorrente = fotografia;
            }

            if (_fotografiaCorrente != null && !_fotografiaCorrente.Equals(fotografia))
            {

                AiutanteFoto.disposeImmagini(_fotografiaCorrente, IdrataTarget.Risultante);
                AiutanteFoto.disposeImmagini(_fotografiaCorrente, IdrataTarget.Originale);
                _fotografiaCorrente = fotografia;
            }

        }
    }
}
