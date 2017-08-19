using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.BarCode;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.UI.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Digiphoto.Lumen.UI.BarcodeScanner {

	public class BarcodeScannerViewModel : ViewModelBase {

		public BarcodeScannerViewModel() {
			this.giornataDaElaborare = DateTime.Today;
			this.dalNumFoto = 1;
		}

		#region Proprietà

		private DateTime _giornataDaElaborare;
		public DateTime giornataDaElaborare {
			get {
				return _giornataDaElaborare;
			}
			set {
				if( _giornataDaElaborare != value ) {
					_giornataDaElaborare = value;
					OnPropertyChanged( "giornataDaElaborare" );
				}
			}
		}

		public bool elaborazioneInCorso {
			get {
				if( IsInDesignMode )
					return false;
				else
					return barcodeSrv != null && barcodeSrv.isRunning;
			}
		}

		private int _percProgresso;
		public int percProgresso {
			get {
				return _percProgresso;
			}
			set	{
				if( _percProgresso != value ) {
					_percProgresso = value;
					OnPropertyChanged( "percProgresso" );
				}
			}
		}

		private string _messaggio;
		public string messaggio {
			get {
				return _messaggio;
			}
			set {
				if( _messaggio != value ) {
					_messaggio = value;
					OnPropertyChanged( "messaggio" );
				}
			}
		}

		public bool possoScansionare {
			get {
				return !elaborazioneInCorso;
			}
		}

		public bool possoAnnullareScansione
		{
			get
			{
				return elaborazioneInCorso;
			}
		}

		private int _dalNumFoto;
		public int dalNumFoto {
			get {
				return _dalNumFoto;
			}
			set {
				if( _dalNumFoto != value ) {
					_dalNumFoto = value;
					OnPropertyChanged( "dalNumFoto" );
				}
			}
		}


		IBarCodeSrv barcodeSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IBarCodeSrv>();
			}
		}

		#endregion Proprietà

		#region Metodi

		void scansionare( bool start ) {

			if( start )
				scansionareStart();
			else
				scansionareStop();
		}

		void refreshStato() {
			OnPropertyChanged( "possoScansionare" );
			OnPropertyChanged( "possoAnnullareScansione" );
			OnPropertyChanged( "elaborazioneInCorso" );
		}

		void scansionareStart() {

			ParamCercaFoto param = new ParamCercaFoto();
			param.giornataIniz = this.giornataDaElaborare;
			param.giornataFine = param.giornataIniz;
			if( this.dalNumFoto >= 0 ) {
				// Prendo dal numero fotogramma indicato in avanti
				param.numeriFotogrammi = dalNumFoto + "-";
			}
			param.didascalia = "<IsNull>";

			this.messaggio = "Inizio scansione";
			this.percProgresso = 0;

			barcodeSrv.prepareToScan( param, scansionatore_ProgressChanged, scansionatore_RunWorkerCompleted );
			barcodeSrv.start();

			refreshStato();
		}

		void scansionareStop() {
			barcodeSrv.stop();

			refreshStato();
		}

		private void scansionatore_ProgressChanged( object sender, System.ComponentModel.ProgressChangedEventArgs e ) {
			this.percProgresso = e.ProgressPercentage;
			StatoScansione ss = (StatoScansione)e.UserState;
			this.messaggio = String.Format( "Scansionate {0,4} foto di {1,4}.\r\nTrovati {2,4} barcode", ss.attuale, ss.totale, ss.barcodeTrovati );
		}

		private void scansionatore_RunWorkerCompleted( object sender, System.ComponentModel.RunWorkerCompletedEventArgs e ) {

			StatoScansione ss = (StatoScansione) e.Result;
			if( ss.totale == 0 )
				this.messaggio = "Nessuna foto trovata con i parametri indicati";
			else
				this.messaggio = String.Format( "Scansionate {0,4} foto di {1,4}.\r\nTrovati {2,4} barcode", ss.attuale, ss.totale, ss.barcodeTrovati );

			scansionareStop();
		}

		#endregion Metodi

		#region Comandi

		private RelayCommand _scansionareCommand;
		public ICommand scansionareCommand
		{
			get
			{
				if( _scansionareCommand == null ) {
					_scansionareCommand = new RelayCommand( param => scansionare( "start".Equals( param ) ), 
					                                        param => "start".Equals( param ) ? possoScansionare : possoAnnullareScansione, 
															false );
				}
				return _scansionareCommand;
			}
		}

		#endregion Comandi

	}
}
