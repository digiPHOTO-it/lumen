using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Licensing;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Util;
using Microsoft.Win32;

namespace Digiphoto.Lumen.GestoreConfigurazione.UI.Licenze {

	public class LicenseEditorViewModel : ViewModelBase {


		public LicenseEditorViewModel() {

			// Calcolo e setto il numero di serie hardware
			machineCode = LicenseUtil.getMachineCode();

			// Carico il codice di licenza (eventuale) dal registry
			codiceLicenza = LicenseUtil.readCurrentLicenseKey();

			validareLicenza( codiceLicenza, false );
		}

		#region Fields

		RegistryLicense _registryLicense;
		
		#endregion Fields



		#region Proprietà

		public String machineCode {
			get;
			private set;
		}

		public String codiceLicenza {
			get;
			set;
		}

		public int? giorniRimastiLic {
			get {
				return _registryLicense == null ? null : (int?)_registryLicense.DaysLeft;
			}
		}

		public DateTime? dataScadenzaLic {
			get {
				return _registryLicense == null ? null : (DateTime?)_registryLicense.ExpireDate;
			}
		}

		public string msgValidazioneLic {
			get {

				if( _registryLicense == null )
					return "Codice licenza non valido oppure non corretto";
				else if( _registryLicense.IsExpired )
					return "Codice di licenza valido ma scaduto";
				else if( !_registryLicense.IsOnRightMachine )
					return "Questo codice di licenza è stato creato per un altro computer";
				else
					return "Codice licenza valido";
			}
		}

		public bool isLicenzaValida {
			get {
				return _registryLicense != null && _registryLicense.IsExpired == false && _registryLicense.IsOnRightMachine == true;
			}
		}

		#endregion Proprietà

		#region Metodi

		private bool possoValdareLicenza( string codLicenza ) {
			bool posso = true;

			posso = !String.IsNullOrWhiteSpace( codLicenza );
			return true;
		}


		private void validareLicenza( string codLicenza, bool emettiAvviso ) {

			try {

				_registryLicense = new RegistryLicense( codLicenza );

			} catch( Exception ) {

				_registryLicense = null;
			}

			OnPropertyChanged( "isLicenzaValida" );
			OnPropertyChanged( "msgValidazioneLic" );
			OnPropertyChanged( "giorniRimastiLic" );
			OnPropertyChanged( "dataScadenzaLic" );

			if( emettiAvviso )
				if( isLicenzaValida )
					dialogProvider.ShowMessage( msgValidazioneLic, "OK" );
				else
					dialogProvider.ShowError( msgValidazioneLic, "VALIDAZIONE FALLITA", null );
		}

		#endregion Metodi

		#region Comandi

		private RelayCommand _validareLicenzaCommand;
		public ICommand validareLicenzaCommand {
			get {
				if( _validareLicenzaCommand == null ) {
					_validareLicenzaCommand = new RelayCommand( codLic => this.validareLicenza( (string)codiceLicenza, true ),
				                                                codLic => this.possoValdareLicenza( (string)codiceLicenza ) );
				}
				return _validareLicenzaCommand;
			}
		}

		#endregion Comandi
	}




}
