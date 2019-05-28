using System;
using System.Windows.Input;
using System.Diagnostics;
using log4net;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.UI.Mvvm {

	public class RelayCommand : ICommand {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( RelayCommand ) );

		#region Fields

		readonly Action<object> _execute;
		readonly Predicate<object> _canExecute;
        readonly Action<object> _afterExecute;

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

        public RelayCommand(Action<object> execute, Predicate<object> canExecute, bool? salvaAllaFineDelComando) : this( execute, canExecute, salvaAllaFineDelComando,  null){
        }

        public RelayCommand( Action<object> execute, Predicate<object> canExecute, bool? salvaAllaFineDelComando,  Action<object> afterExecute) {

			if( execute == null )
				throw new ArgumentNullException( "execute" );

			_execute = execute;
			_canExecute = canExecute;
			this.salvaAllaFineDelComando = salvaAllaFineDelComando;
            _afterExecute = afterExecute;
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


		public void RaiseCanExecuteChanged() {
			CommandManager.InvalidateRequerySuggested();
		}

		public void Execute( object parameter ) {

			try {

				_giornale.Debug( "Eseguo RelayCommad: " + _execute.Method.ToString() + " parametro=" + parameter );

				esegui( parameter );

				if( _afterExecute != null ) {

					_giornale.Debug( "RelayCommad invoco after execute" );

					_afterExecute.Invoke( parameter );
				}

				_giornale.Debug( "Esecuzione RelayCommad completata: " + _execute.Method.ToString() + " parametro=" + parameter );

			} catch( OutOfMemoryException ofm ) {

				long memoryPrima = Process.GetCurrentProcess().WorkingSet64;

				FormuleMagiche.attendiGcFinalizers();

				long memoryDopo = Process.GetCurrentProcess().WorkingSet64;

				_giornale.Error( "finita la memoria: prima=" + memoryPrima + " dopo=" + memoryDopo );

				if( _execute.Target is ViewModelBase ) {
					ViewModelBase viewModel = _execute.Target as ViewModelBase;
					if( viewModel.dialogProvider != null )
						viewModel.dialogProvider.ShowError( ofm.GetType().Name + "\n" + ErroriUtil.estraiMessage( ofm ), "Errore imprevisto.", null );
				}

			} catch( Exception ee ) {
				
				_giornale.Error( _execute.Method.ToString(), ee );

				if( _execute.Target is ViewModelBase ) {
					ViewModelBase viewModel = _execute.Target as ViewModelBase;
					if( viewModel.dialogProvider != null )
						viewModel.dialogProvider.ShowError( ee.GetType().Name + "\n" + ErroriUtil.estraiMessage(ee), "Errore imprevisto. Consultare il Log", null );
				}

				// per ora non voglio far spaccare il programma altrimenti perdo il lavoro in corso.
				// anche se concettualmente sarebbe più corretto rilanciare l'errore.
				// L'unico problema è che non c'è modo di fare un try-catch di un Command.
				// throw ee;
			}
		}

		private void esegui( object parameter ) {

			// null significa che questo comando, non svolge operazioni sul database.
			if( salvaAllaFineDelComando == null )
				_execute( parameter );
			else {

				// Se per caso esiste già un una unit-of-work attiva, allora uso quella.
				if( UnitOfWorkScope.hasCurrent )
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