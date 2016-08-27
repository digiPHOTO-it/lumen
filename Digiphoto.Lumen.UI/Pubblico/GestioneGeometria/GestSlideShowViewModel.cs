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

		public bool possoAprire {
			get {
				if( IsInDesignMode )
					return true;
				else
					return !gestoreFinestrePubbliche.isSlideShowVisible;
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


		private void aprire() {
			gestoreFinestrePubbliche.aprireFinestraSlideShow();
		}

		private void chiudere() {
			gestoreFinestrePubbliche.chiudereFinestraSlideShow();
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
		private void salva()
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
		
		private RelayCommand _aprireCommand;
		public ICommand aprireCommand {
			get {
				if( _aprireCommand == null ) {
					_aprireCommand = new RelayCommand( param => aprire(), param => possoAprire );
				}
				return _aprireCommand;
			}
		}

		private RelayCommand _chiudereCommand;
		public ICommand chiudereCommand {
			get {
				if( _chiudereCommand == null ) {
					_chiudereCommand = new RelayCommand( param => chiudere() );
				}
				return _chiudereCommand;
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

		private RelayCommand _salvaCommand;
		public ICommand salvaCommand
		{
			get
			{
				if (_salvaCommand == null)
				{
					_salvaCommand = new RelayCommand(param => salva());
				}
				return _salvaCommand;
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
