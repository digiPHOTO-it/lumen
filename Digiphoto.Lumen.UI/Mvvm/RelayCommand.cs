using System;
using System.Windows.Input;
using System.Diagnostics;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;

namespace Digiphoto.Lumen.UI.Mvvm {

	public class RelayCommand : ICommand {

		#region Fields

		readonly Action<object> _execute;
		readonly Predicate<object> _canExecute;

		/// <summary>
		/// Ogni comando può essere eseguito in una precisa UnitOfWork 
		/// come se fosse una transazione.
		/// Se questo attributo è nullo, allora non apro una UnitOfWork
		/// Se invece è valorizzato, allora
		/// </summary>
		protected bool? salvaAllaFineDelComando {
			get;
			set;
		}

		#endregion // Fields

		#region Constructors

		public RelayCommand( Action<object> execute ) : this( execute, null ) {
		}

		public RelayCommand( Action<object> execute, Predicate<object> canExecute ) : this( execute, canExecute, null ) {
		}

		public RelayCommand( Action<object> execute, Predicate<object> canExecute, bool? salvaAllaFineDelComando ) {

			if( execute == null )
				throw new ArgumentNullException( "execute" );

			_execute = execute;
			_canExecute = canExecute;
			this.salvaAllaFineDelComando = salvaAllaFineDelComando;
		}

		#endregion // Constructors

		#region ICommand Members

		[DebuggerStepThrough]
		public bool CanExecute( object parameter ) {
			return _canExecute == null ? true : _canExecute( parameter );
		}

		public event EventHandler CanExecuteChanged {
			add {
				CommandManager.RequerySuggested += value;
			}
			remove {
				CommandManager.RequerySuggested -= value;
			}
		}

		public void Execute( object parameter ) {

			// null significa che questo comando, non svolge operazioni sul database.
			if( salvaAllaFineDelComando == null )
				_execute( parameter );
			else {

				// Se per caso esiste già un una unit-of-work attiva, allora uso quella.
				if( UnitOfWorkScope.CurrentObjectContext != null )
					_execute( parameter );
				else {
					// creo una unit-of-work nuova che verrà subito chiusa. In questo modo stacco sempre le entità e chiudo la sessione di lavoro.
					using( new UnitOfWorkScope( (bool)salvaAllaFineDelComando ) ) {
						_execute( parameter );
					}
				}
			}
		}

		#endregion // ICommand Members
	}
}