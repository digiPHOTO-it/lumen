using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Input;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.Pubblico.GestioneGeometria;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Forms;

namespace Digiphoto.Lumen.UI.Pubblico {
	public class GestSlideShowViewModel : ViewModelBase
	{

		public GestSlideShowViewModel() {
			caricaSchermi();
		}


		#region Proprietà

		public ICollectionView schermiCV {
			get;
			private set;
		}


		private GestoreFinestrePubbliche gestoreFinestrePubbliche {
			get {
				return ((App)System.Windows.Application.Current).gestoreFinestrePubbliche;
			}
		}

		private SlideShowViewModel slideShowViewModel
		{
			get
			{
				if (IsInDesignMode)
					return null;

				return gestoreFinestrePubbliche.slideShowViewModel;
			}
		}

		public short deviceEnum { 
			get {
				return gestoreFinestrePubbliche.geomSS.deviceEnum;
			}
		}

		public bool fullScreen {
			get {
				return gestoreFinestrePubbliche.geomSS.fullScreen;
			}
		}

		public bool possoAprireSlideShow {
			get {
				if( IsInDesignMode )
					return true;
				else
					return !gestoreFinestrePubbliche.isSlideShowVisible;
			}
		}
		
		public bool possoChiudereSlideShow {
			get {
				if( IsInDesignMode )
					return true;
				else
					return gestoreFinestrePubbliche.isSlideShowVisible;
			}
		}

		public bool possoSalvarePosizioneSlideShow {
			get {
				if( IsInDesignMode )
					return true;
				else
					return gestoreFinestrePubbliche.isSlideShowVisible;
			}
		}


		public bool possoAprirePubblico {
			get {
				if( IsInDesignMode )
					return true;
				else
					return !gestoreFinestrePubbliche.isPubblicoVisible;
			}
		}

		public bool possoChiuderePubblico {
			get {
				if( IsInDesignMode )
					return true;
				else
					return gestoreFinestrePubbliche.isPubblicoVisible;
			}
		}


		public bool possoMassimizzare {
			get {
				if( IsInDesignMode )
					return true;
				else
					return gestoreFinestrePubbliche.isSlideShowVisible;
			}
		}

		public bool isPossibileMassimizzareSulMonitor2 { 
			get {
				return (Screen.AllScreens.Length > 1);
            }
		}


		#endregion Proprieta

		#region Metodi

		void caricaSchermi() {

			schermiCV = CollectionViewSource.GetDefaultView( WpfScreen.AllScreens() );
			OnPropertyChanged( "schermiCV" );

		}



		private void aprireSlideShow() {
			gestoreFinestrePubbliche.aprireFinestraSlideShow();
		}

		private void chiudereSlideShow() {
			gestoreFinestrePubbliche.chiudereFinestraSlideShow();
		}

		private void aprirePubblico() {
			gestoreFinestrePubbliche.aprireFinestraPubblico();
		}

		private void chiuderePubblico() {
			gestoreFinestrePubbliche.chiudereFinestraPubblico();
		}

		private void refreshCampi() {
			// Aggiorno i dati visibili a video
			OnPropertyChanged( "fullScreen" );
			OnPropertyChanged( "deviceEnum" );
		}

		/// <summary>
		/// Devo memorizzare la geometria attuale della finestra dello SS
		/// nel file di configurazione
		/// </summary>
		private void salvarePosizioneSlideShow()
		{
			if( gestoreFinestrePubbliche.salvaGeometriaFinestraSlideShow() ) {

				refreshCampi();

				dialogProvider.ShowMessage( "La posizione della finestra dello Slide Show\nè stata salvata correttamente", "Avviso" );
			} else {
				dialogProvider.ShowError( "La posizione della finestra dello Slide Show\nè stata salvata correttamente", "Errore", null );
			}
		}

		private void ripristina()
		{
			gestoreFinestrePubbliche.ripristinaFinestraSlideShow();
			refreshCampi();

			dialogProvider.ShowMessage("La posizione dello slideShow è stata ripristinata\nPremere salva per confermare","Avviso");
		}

		/// <summary>
		/// Resetto la posizione della finestra alla geometria di default
		/// </summary>
		private void normalizzareSulMonitor1()
		{
			gestoreFinestrePubbliche.normalizzareFinestraSlideShowSulMonitor1();
		}

		private void massimizzare() {
			gestoreFinestrePubbliche.massimizzareFinestraSlideShow();
		}

		private void massimizzareSulMonitor2() {

			// Per prima cosa apro la finestra (se non è aperta)
			gestoreFinestrePubbliche.massimizzareFinestraSlideShowSulMonitor2();

		}

		/// <summary>
		/// Riposiziono la finestra dello SS usando la geometria corrente
		/// </summary>
		public void riposiziona()
		{
			gestoreFinestrePubbliche.posizionaFinestraSlideShow();
        }

		#endregion Metodi

		#region Comandi
		
		private RelayCommand _aprireSlideShowCommand;
		public ICommand aprireSlideShowCommand {
			get {
				if( _aprireSlideShowCommand == null ) {
					_aprireSlideShowCommand = new RelayCommand( param => aprireSlideShow(), param => possoAprireSlideShow );
				}
				return _aprireSlideShowCommand;
			}
		}

		private RelayCommand _chiudereSlideShowCommand;
		public ICommand chiudereSlideShowCommand {
			get {
				if( _chiudereSlideShowCommand == null ) {
					_chiudereSlideShowCommand = new RelayCommand( param => chiudereSlideShow(), param => possoChiudereSlideShow );
				}
				return _chiudereSlideShowCommand;
			}
		}

		private RelayCommand _aprirePubblicoCommand;
		public ICommand aprirePubblicoCommand {
			get {
				if( _aprirePubblicoCommand == null ) {
					_aprirePubblicoCommand = new RelayCommand( param => aprirePubblico(), param => possoAprirePubblico );
				}
				return _aprirePubblicoCommand;
			}
		}

		private RelayCommand _chiuderePubblicoCommand;
		public ICommand chiuderePubblicoCommand {
			get {
				if( _chiuderePubblicoCommand == null ) {
					_chiuderePubblicoCommand = new RelayCommand( param => chiuderePubblico(), param => possoChiuderePubblico );
				}
				return _chiuderePubblicoCommand;
			}
		}

		private RelayCommand _massimizzareCommand;
		public ICommand massimizzareCommand {
			get {
				if( _massimizzareCommand == null ) {
					_massimizzareCommand = new RelayCommand( param => massimizzare(), p => possoMassimizzare );
				}
				return _massimizzareCommand;
			}
		}

		private RelayCommand _salvarePosizioneSlideShowCommand;
		public ICommand salvarePosizioneSlideShowCommand
		{
			get
			{
				if (_salvarePosizioneSlideShowCommand == null)
				{
					_salvarePosizioneSlideShowCommand = new RelayCommand(param => salvarePosizioneSlideShow(), p => possoSalvarePosizioneSlideShow );
				}
				return _salvarePosizioneSlideShowCommand;
			}
		}

		private RelayCommand _ripristinaCommand;
		public ICommand ripristinaCommand
		{
			get
			{
				if (_ripristinaCommand == null)
				{
					_ripristinaCommand = new RelayCommand(param => ripristina());
				}
				return _ripristinaCommand;
			}
		}

		private RelayCommand _resetCommand;
		public ICommand resetCommand
		{
			get
			{
				if (_resetCommand == null)
				{
					_resetCommand = new RelayCommand(param => normalizzareSulMonitor1());
				}
				return _resetCommand;
			}
		}

		private RelayCommand _massimizzareSulMonitor2Command;
		public ICommand massimizzareSulMonitor2Command {
			get {
				if( _massimizzareSulMonitor2Command == null ) {
					_massimizzareSulMonitor2Command = new RelayCommand( param => massimizzareSulMonitor2() );
				}
				return _massimizzareSulMonitor2Command;
			}
		}


		#endregion Comandi
	}
}
