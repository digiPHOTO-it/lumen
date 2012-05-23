using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.Core.Database {

	/// <summary>
	/// Defines a scope wherein only one ObjectContext instance is created, 
	/// and shared by all of those who use it. Instances of this class are 
	/// supposed to be used in a using() statement.
	/// </summary>
	public sealed class UnitOfWorkScope : IDisposable {

		[ThreadStatic]
		private static UnitOfWorkScope _currentScope;
		
		private LumenEntities _objectContext;
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
		internal static LumenEntities CurrentObjectContext {
			get {
				return _currentScope != null ? _currentScope._objectContext : null;
			}
		}
		/// <summary>
		/// Default constructor. Object changes are not automatically saved 
		/// at the end of the scope.
		/// </summary>
		public UnitOfWorkScope()
			: this( false ) {
		}
		/// <summary>
		/// Parameterized constructor.
		/// </summary>
		/// <param name="saveAllChangesAtEndOfScope">
		/// A boolean value that indicates whether to automatically save 
		/// all object changes at end of the scope.
		/// </param>
		public UnitOfWorkScope( bool saveAllChangesAtEndOfScope ) {
			if( _currentScope != null && !_currentScope._isDisposed )
				throw new InvalidOperationException( "ObjectContextScope instances " +
																"cannot be nested." );
			_saveAllChangesAtEndOfScope = saveAllChangesAtEndOfScope;
			/* Create a new ObjectContext instance: */
			_objectContext = new LumenEntities();
			_isDisposed = false;
			Thread.BeginThreadAffinity();
			/* Set the current scope to this UnitOfWorkScope object: */
			_currentScope = this;
		}
		/// <summary>
		/// Called on the end of the scope. Disposes the NorthwindObjectContext.
		/// </summary>
		public void Dispose() {
			if( !_isDisposed ) {
				/* End of scope, so clear the thread static 
				 * _currentScope member: */
				_currentScope = null;
				Thread.EndThreadAffinity();
				if( _saveAllChangesAtEndOfScope )
					_objectContext.SaveChanges();
				/* Dispose the scoped ObjectContext instance: */
				_objectContext.Dispose();
				_isDisposed = true;
			}
		}
	}

}
