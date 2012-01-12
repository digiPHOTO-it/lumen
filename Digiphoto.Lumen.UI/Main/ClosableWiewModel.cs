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

        #endregion // Fields

        #region Constructor

        protected ClosableWiewModel() {
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
        /// Raised when this workspace should be removed from the UI.
        /// </summary>
        public event EventHandler RequestClose;

        void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion // RequestClose [event]
    }
}


