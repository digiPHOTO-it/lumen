using System;
using System.Windows.Input;
using Digiphoto.Lumen.UI.Mvvm;

namespace Digiphoto.Lumen.UI {

    /// <summary>
    /// Questa classe è astratta e implementa la richiesta di chiusura della finestra 
	/// tramite un evento.
	/// In generale, infatti, prima di poter chiudere una finestra (come per esempio la MainWindow)
	/// occorre che il programma stesso dia il consenso. Per esempio se faccio "X" sulla finestra
	/// del Notepad ed ho un documento non salvato, prima di uscire mi chiede se voglio salvare oppure no.
	/// In pratica non posso buttare giù brutalmente, ma devo notificare la UI specifica che 
	/// desidero uscire.
	/// Implementa un CloseCommand
	/// </summary>
    public abstract class ClosableWiewModel : ViewModelBase {

        #region Fields

        RelayCommand _closeCommand;

		public bool abilitoShutdown {
			get;
			set;
		}

        #endregion // Fields

        #region Constructor

        protected ClosableWiewModel() {
			abilitoShutdown = false;
        }

        #endregion // Constructor

        #region CloseCommand

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to remove this workspace from the user interface.
        /// </summary>
        public ICommand CloseCommand {
            get {
                if (_closeCommand == null)
                    _closeCommand = new RelayCommand(param => this.OnRequestClose());

                return _closeCommand;
            }
        }

        #endregion // CloseCommand

        #region RequestClose [event]

        /// <summary>
        /// Raised when this windows is closed by the user.
        /// </summary>
        public event EventHandler RequestClose;

        protected virtual void OnRequestClose()
        {
			bool spegni = false;

			if( abilitoShutdown ) {
				if( dialogProvider != null ) {
					dialogProvider.ShowConfirmation( "Vuoi spegnere il computer", "Uscita",
						( sino ) => {
							if( spegni = sino )
								return;
						} );
				}
			}

            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);

			if( spegni )
				System.Diagnostics.Process.Start( "shutdown.exe", "-s -t 05" );
        }

        #endregion // RequestClose [event]
    }
}


