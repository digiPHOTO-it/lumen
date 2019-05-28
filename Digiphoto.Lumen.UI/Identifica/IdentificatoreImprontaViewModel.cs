using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Servizi.Impronte;
using Digiphoto.Lumen.UI.FingerprintServiceReference;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Digiphoto.Lumen.UI.Identifica {

	public class IdentificatoreImprontaViewModel : ViewModelBase {

		public IdentificatoreImprontaViewModel() {

			statoScanner = "()";

			nomeFileBmpImpronta = Path.Combine( Path.GetTempPath(), "impronta.bmp" );
			if( File.Exists( nomeFileBmpImpronta ) )
				File.Delete( nomeFileBmpImpronta );

			if( scanControlCommand.CanExecute( true ) )
				scanControlCommand.Execute( true );
		}

		#region Proprietà

		private string _esitoIdentificazione;
		public string esitoIdentificazione {
			get {
				return _esitoIdentificazione;
			}
			private set {
				if( _esitoIdentificazione != value ) {
					_esitoIdentificazione = value;
					OnPropertyChanged( "esitoIdentificazione" );
				}
			}
		}

		private string _nomeIdentificato;
		public string nomeIdentificato {
			get {
				return _nomeIdentificato;
			}
			private set {
				if( _nomeIdentificato != value ) {
					_nomeIdentificato = value;
					OnPropertyChanged( "nomeIdentificato" );
				}
			}
		}

		private string _statoScanner;
		public string statoScanner {
			get {
				return _statoScanner;
			}
			set {
				if( _statoScanner != value ) {
					_statoScanner = value;
					OnPropertyChanged( "statoScanner" );
				}
			}
		}

		public string nomeFileBmpImpronta {
			get;
			private set;
		}

		public bool possoIdentificare {
			get {
				return strBase64Template != null;
			}
		}

		private string strBase64Template;

		#endregion

		#region Comandi

		private RelayCommand _scanControlCommand;
		public ICommand scanControlCommand {
			get {
				if( _scanControlCommand == null ) {
					_scanControlCommand = new RelayCommand( onOff => scanControl( Convert.ToBoolean( onOff ) ),
						                                    onOff => (Convert.ToBoolean( onOff ) ? true : true ) );
				}
				return _scanControlCommand;
			}
		}

		private RelayCommand _identificareCommand;
		public ICommand identificareCommand {
			get {
				if( _identificareCommand == null ) {
					_identificareCommand = new RelayCommand( p => identificare(), p => possoIdentificare );
				}
				return _identificareCommand;
			}
		}

		#endregion

		#region Metodi

		private void scanControl( bool onOff ) {

			IImpronteSrv srv = LumenApplication.Instance.getServizioAvviato<IImpronteSrv>();

			if( srv.isRunning ) {
				if( onOff == false ) {
					statoScanner = "Disabilitato";
					srv.stop();
				}
			} else {
				if( onOff == true ) {
					srv.start();
					statoScanner = "Abilitato";
				}
			}

			if( onOff == true ) {
				statoScanner = "In ascolto...";
				srv.Listen( OnImprontaAcquisita, true );
			}

		}

		void identificare() {

			FingerprintServiceClient fpClient = new FingerprintServiceClient();

			if( fpClient != null ) {

				nomeIdentificato = fpClient.Identifica( strBase64Template );

				esitoIdentificazione = (nomeIdentificato == null ? "NO MATCH" : "MATCH");

				fpClient.Close();

			} else {
				throw new LumenException( "Impossibile connettersi a Fingerprint-Service. Controllare se in esecuzione" );
			}

		}

		private void OnImprontaAcquisita( object sender, ScansioneEvent eventArgs ) {

			esitoIdentificazione = "SCAN";

			if( eventArgs.isValid ) {

				if( File.Exists( nomeFileBmpImpronta ) )
					File.Delete( nomeFileBmpImpronta );
				File.Move( eventArgs.bmpFileName, nomeFileBmpImpronta );

				strBase64Template = eventArgs.strBase64Template;

			} else {
				nomeFileBmpImpronta = null;
				strBase64Template = null;
			}

			// Forzo il refersh del pulsante ma nella UI perché qui sono in un thread di callback e non avrebbe effetto
			App.Current.Dispatcher.BeginInvoke( new Action( () => {
				OnPropertyChanged( "possoIdentificare" );
				OnPropertyChanged( "nomeFileBmpImpronta" );

				_identificareCommand.RaiseCanExecuteChanged();
			} ) );


		}

		protected override void OnDispose() {

			scanControl( false );

		}

		#endregion
	}
}
