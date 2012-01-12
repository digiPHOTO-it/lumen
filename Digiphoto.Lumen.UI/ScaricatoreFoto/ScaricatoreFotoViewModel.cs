using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Applicazione;
using System.Windows.Input;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.UI {



	public class ScaricatoreFotoViewModel : ViewModelBase {

		public ScaricatoreFotoViewModel() {
			paramScarica = new ParamScarica();
		}

		/** Parametri da dare in pasto al servizio per iniziare lo scarico */
		public ParamScarica paramScarica {
			get;
			set;
		}

		public Fotografo fotografo {
			get;
			set;
		}

		public IScaricatoreFotoSrv scaricatoreFotoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IScaricatoreFotoSrv>();
			}
		}

		private RelayCommand _scaricareCommand;
		public ICommand scaricareCommand {
			get {
				if( _scaricareCommand == null ) {
					_scaricareCommand = new RelayCommand( param => this.scaricare(),
														param => this.possoScaricare,
														true );
				}
				return _scaricareCommand;
			}
		}

		private void scaricare() {
			
			Console.WriteLine( "STOP" );

			// TODO

			scaricatoreFotoSrv.scarica( paramScarica );
		}

		public bool possoScaricare {
			get {
				return true;
			}
		}

	}




}
