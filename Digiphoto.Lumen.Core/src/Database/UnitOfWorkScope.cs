using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Config;
using log4net;
using Digiphoto.Lumen.Util;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

namespace Digiphoto.Lumen.Core.Database {

	/// <summary>
	/// Defines a scope wherein only one ObjectContext instance is created, 
	/// and shared by all of those who use it. Instances of this class are 
	/// supposed to be used in a using() statement.
	/// </summary>
	public sealed class UnitOfWorkScope : IDisposable {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( UnitOfWorkScope ) );

		[ThreadStatic]
		private static UnitOfWorkScope _currentScope;
		
		private LumenEntities _dbContext;
		private bool _isDisposed, _saveAllChangesAtEndOfScope;
		/// <summary>
		/// Gets or sets a boolean value that indicates whether to automatically save 
		/// all object changes at end of the scope.
		/// </summary>
		public bool SaveAllChangesAtEndOfScope {
			get {
				return _saveAllChangesAtEndOfScope;
			}
			set {
				_saveAllChangesAtEndOfScope = value;
			}
		}
		/// <summary>
		/// Returns a reference to the Lumen Object Context that is created 
		/// for the current scope. If no scope currently exists, null is returned.
		/// </summary>
		public static LumenEntities currentDbContext {
			get {
				return _currentScope != null ? _currentScope._dbContext : null;
			}
		}

		public static ObjectContext currentObjectContext {
			get {
				return _currentScope == null ? null : ((IObjectContextAdapter)UnitOfWorkScope.currentDbContext).ObjectContext;
			}
		}

		/// <summary>
		/// Mi dice se ho un contesto corrente attivo
		/// </summary>
		public static bool hasCurrent {
			get {
				return currentDbContext != null;
			}
		}

		/// <summary>
		/// Default constructor. Object changes are not automatically saved 
		/// at the end of the scope.
		/// </summary>
		public UnitOfWorkScope()
			: this( false ) {
		}

		public UnitOfWorkScope( bool saveAllChangesAtEndOfScope ) 
			: this( saveAllChangesAtEndOfScope, null ) {
		}

		/// <summary>
		/// Parameterized constructor.
		/// </summary>
		/// <param name="saveAllChangesAtEndOfScope">
		/// A boolean value that indicates whether to automatically save 
		/// all object changes at end of the scope.
		/// </param>
		public UnitOfWorkScope( bool saveAllChangesAtEndOfScope, string connectionString ) {

			if( _currentScope != null && !_currentScope._isDisposed )
				throw new InvalidOperationException( "ObjectContextScope instances " +
																"cannot be nested." );
			_saveAllChangesAtEndOfScope = saveAllChangesAtEndOfScope;


			/* Create a new ObjectContext instance: uso il proxy */
			if( String.IsNullOrEmpty( connectionString ) )
				_dbContext = new LumenEntities();
			else {
				_dbContext = new LumenEntities( connectionString );
			}

//			_dbContext.Configuration.ProxyCreationEnabled = false;


			_isDisposed = false;
			Thread.BeginThreadAffinity();
			/* Set the current scope to this UnitOfWorkScope object: */
			_currentScope = this;
		}

		/// <summary>
		/// Called on the end of the scope. Disposes the ObjectContext.
		/// </summary>
		public void Dispose() {

			if( !_isDisposed ) {

				try {

					/* End of scope, so clear the thread static 
					 * _currentScope member: */
					_currentScope = null;
					Thread.EndThreadAffinity();

					if( _saveAllChangesAtEndOfScope ) {
						// Qui ci potrebbero essere delle eccezioni
						_dbContext.SaveChanges();
					}

				} catch( Exception ee ) {

					_giornale.Error( "Salvataggio sul db fallito: " + ErroriUtil.estraiMessage( ee ), ee );
					throw ee;

				} finally {
					// In ogni caso devo chiudere tutto
					_dbContext.Dispose();
					_dbContext = null;
					_isDisposed = true;
				}

			} else {
				_giornale.Warn( "Come mai casco qui ?? Impossibile. Debuggare!" );
				if( System.Diagnostics.Debugger.IsAttached )
					System.Diagnostics.Debugger.Break();
			}

		}
	}

}
