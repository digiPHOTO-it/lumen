using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.UI.SelettoreDischi;
using System.IO;
using System.Windows.Input;
using System;

namespace Digiphoto.Lumen.UI.Carrelli.Masterizzare {

	public class ScegliMasterizzaTargetViewModel : ClosableWiewModel {

		public ScegliMasterizzaTargetViewModel() {

			// Propongo per default il masterizzatore in configurazione
			selettoreDiscoViewModel = new SelettoreDiscoViewModel( Configurazione.UserConfigLumen.defaultMasterizzatore );

			// Propongo per default la chiavetta in configurazione
			cartella = Configurazione.UserConfigLumen.defaultChiavetta;


		}

		public SelettoreDiscoViewModel selettoreDiscoViewModel {
			get;
			private set;
		}

		private string _cartella;
		public string cartella {
			get {
				return _cartella;
			}
			set {
				if( _cartella != value ) {
					_cartella = value;
					OnPropertyChanged( "cartella" );
				}
			}
		}

		
		void confermare( MasterizzaTarget target ) {

			// il risultato lo metto sempre in "cartella"
			if( target == MasterizzaTarget.DriveRimovibili ) {
				this.cartella = selettoreDiscoViewModel.discoSelezionato.Name;	
			}

			this.CloseCommand.Execute( null );
		}

		public bool possoConfermare( MasterizzaTarget target ) {

			bool posso = false;

			if( target == MasterizzaTarget.DriveRimovibili &&
				this.selettoreDiscoViewModel.discoSelezionato != null 
				/*
				&&
				this.selettoreDiscoViewModel.discoSelezionato.IsReady == true 
				*/
				)
				posso = true;

			if( target == MasterizzaTarget.Cartella ) {
				if( cartella != null && Directory.Exists( cartella ) )
					posso = true;
			}

			return posso;
		}

		private RelayCommand _confermareCommand;
		public ICommand confermareCommand {
			get {
				if( _confermareCommand == null ) {
					_confermareCommand = new RelayCommand( param => confermare( (MasterizzaTarget)param ),
														   param => possoConfermare( (MasterizzaTarget)param ), false );
				}
				return _confermareCommand;
			}
		}

		public DriveType getDriveType( string cartella ) {

			DriveType ret = DriveType.Unknown;

			for( int ii = 0; ii < selettoreDiscoViewModel.dischiCW.Count; ii++ ) {
				DriveInfo driveInfo = ((DriveInfo)selettoreDiscoViewModel.dischiCW.GetItemAt( ii ));
				if( driveInfo.Name == cartella ) {
					ret = driveInfo.DriveType;
					break;
				}
			}
			return ret;
		}


	}
}
