using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.ScreenCapture;

namespace Digiphoto.Lumen.UI.Pubblico {


	/// <summary>
	/// Questa classe si occupa di mantenere lo stato delle finestre pubbliche, e si sollevare eventi
	/// quando queste vengono aperte o chiuse
	/// </summary>
	public class GestoreFinestrePubbliche {

		private SlideShowWindow _slideShowWindow;
		private SnapshotPubblicoWindow _snapshotPubblicoWindow;
		private MainWindow _mainWindow;
		

		public SlideShowViewModel slideShowViewModel {
			get {
				return _slideShowWindow == null ? null : (SlideShowViewModel)_slideShowWindow.DataContext;
			}
		}

		public SnapshotPubblicoViewModel snapshotPubblicoViewModel {
			get {
				return _snapshotPubblicoWindow == null ? null : (SnapshotPubblicoViewModel)_snapshotPubblicoWindow.DataContext;
			}
		}

		public void creaMainWindow() {
			_mainWindow = new MainWindow();
			_mainWindow.Show();			
		}

		/// <summary>
		/// Stiamo per uscire. chiudo tutto
		/// </summary>
		public void chiudiTutto() {

			chiudiSlideShowWindow();
			chiudiSnapshotPubblicoWindow();
			chiudiMainWindow();

			Application.Current.Shutdown();
		}

		public void chiudiSlideShowWindow() {
			if( _slideShowWindow != null ) {

				ClosableWiewModel cvm = (ClosableWiewModel)slideShowViewModel;
				Object param = null;
				if( cvm.CloseCommand.CanExecute( param ) )
					cvm.CloseCommand.Execute( param );
				_slideShowWindow = null;
			}

			if( stavaGirandoLoSlideShow ) {
				// Probabilmente quando ho aperto questa finestra ho stoppato lo ss. Ora lo riattivo.
				if( slideShowViewModel != null )
					slideShowViewModel.start();
			}

			stavaGirandoLoSlideShow = false;
		}

		private void chiudiMainWindow() {
			if( _mainWindow != null ) {

				ClosableWiewModel cvm = (ClosableWiewModel)_mainWindow.DataContext;
				Object param = null;
				if( cvm.CloseCommand.CanExecute( param ) )
					cvm.CloseCommand.Execute( param );
				_mainWindow = null;
			}
		}

		public void chiudiSnapshotPubblicoWindow() {
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

		/// <summary>
		/// Se la finestra dello slide show non è aperta (o non è istanziata)
		/// la creo sul momento
		/// </summary>
		public bool forseApriSlideShowWindow() {

			// Se è già aperta, non faccio niente
			if( _slideShowWindow != null )
				return false;

			IInputElement elementoKeyboardInfuocato = Keyboard.FocusedElement;

			// Apro la finestra modeless
			// Create a window and make this window its owner
			_slideShowWindow = new SlideShowWindow();
			_slideShowWindow.Closed += chiusoSlideShowWindow;
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
				// Se ho lo ss attivot, allora devo prendere il suo posto
				_snapshotPubblicoWindow.Height = _slideShowWindow.Height;
				_snapshotPubblicoWindow.Width = _slideShowWindow.Width;
				_snapshotPubblicoWindow.Top = _slideShowWindow.Top;
				_snapshotPubblicoWindow.Left = _slideShowWindow.Left;
				_snapshotPubblicoWindow.WindowState = _slideShowWindow.WindowState;

			} else {

				// valorizzo la geometria del form = a quella del form slide show (praticamente lo ricopro)
				_snapshotPubblicoWindow.Height = Configurazione.UserConfigLumen.slideHeight;
				_snapshotPubblicoWindow.Width = Configurazione.UserConfigLumen.slideWidth;
				_snapshotPubblicoWindow.Top = Configurazione.UserConfigLumen.slideTop;
				_snapshotPubblicoWindow.Left = Configurazione.UserConfigLumen.slideLeft;
				if( Configurazione.UserConfigLumen.fullScreen )
					_snapshotPubblicoWindow.WindowState = WindowState.Maximized;
				else
					_snapshotPubblicoWindow.WindowState = WindowState.Normal;
			}

			_snapshotPubblicoWindow.Show();
			_snapshotPubblicoWindow.Topmost = true;

			// Riposiziono il fuoco sul componente che lo deteneva prima di aprire la finestra
			Keyboard.Focus( elementoKeyboardInfuocato );

			return true;
		}


		/// <summary>
		/// Quando apro la snapshot pubblica, siccome deve andare sopra allo slideshow, allora lo fermo.
		/// </summary>
		private bool stavaGirandoLoSlideShow {
			get;
			set;
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
			_slideShowWindow.Closed -= chiusoSlideShowWindow;
			_slideShowWindow = null;
		}

		public void chiusoSnapshotPubblicoWindow( object sender, EventArgs e ) {
			_snapshotPubblicoWindow.Closed -= chiusoSnapshotPubblicoWindow;
			_snapshotPubblicoWindow = null;

			// Se ho aperto lo ss, lo riporto in primo piano.
			if( _slideShowWindow != null )
				_slideShowWindow.Topmost = true;
		}

	}
}
