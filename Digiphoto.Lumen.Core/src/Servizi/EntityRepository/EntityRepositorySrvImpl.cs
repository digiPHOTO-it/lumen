using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using log4net;
using Digiphoto.Lumen.Database;
using System.Data.Objects;
using System.Data.Entity.Validation;

namespace Digiphoto.Lumen.Servizi.EntityRepository {

	public abstract class EntityRepositorySrvImpl<TEntity> : ServizioImpl, IEntityRepositorySrv<TEntity> where TEntity : class {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( EntityRepositorySrvImpl<TEntity> ) );

		public EntityRepositorySrvImpl() {
		}

		public virtual void addNew( TEntity entita ) {
			ObjectSet<TEntity> objectSet = UnitOfWorkScope.CurrentObjectContext.ObjectContext.CreateObjectSet<TEntity>();
			objectSet.AddObject( entita );
			_giornale.Info( "Creata nuova entità " + entita.GetType() + " " + entita.ToString() );
		}

		public virtual IEnumerable<TEntity> getAll() {
			ObjectSet<TEntity> objectSet = UnitOfWorkScope.CurrentObjectContext.ObjectContext.CreateObjectSet<TEntity>();
			return objectSet.AsEnumerable();
		}

		public virtual TEntity getById( object id ) {
			throw new NotImplementedException();
		}

		public virtual IQueryable<TEntity> Query() {

			ObjectSet<TEntity> objectSet = UnitOfWorkScope.CurrentObjectContext.ObjectContext.CreateObjectSet<TEntity>();
			return objectSet.AsQueryable();
		}

		public virtual IQueryable<TEntity> Query( System.Linq.Expressions.Expression<Func<TEntity, bool>> filter ) {

			ObjectSet<TEntity> objectSet = UnitOfWorkScope.CurrentObjectContext.ObjectContext.CreateObjectSet<TEntity>();
			return objectSet.AsQueryable().Where( filter );
		}




		public virtual void update( ref TEntity entita ) {

			OrmUtil.forseAttacca<TEntity>( ref entita );
		}

		public virtual void delete( TEntity entita ) {
			ObjectSet<TEntity> objectSet = UnitOfWorkScope.CurrentObjectContext.ObjectContext.CreateObjectSet<TEntity>();
			objectSet.DeleteObject( entita );
			_giornale.Info( "Cancellata entità " + entita.GetType() + " " + entita.ToString() );
		}

		public int saveChanges() {

			// Non fare try-catch. Se fallice deve saltare con eccezione.
			return UnitOfWorkScope.CurrentObjectContext.SaveChanges();

		}

		/*
				public ObjectResult<TEntity> execute() {
					ObjectSet<TEntity> objectSet = UnitOfWorkScope.CurrentObjectContext.ObjectContext.CreateObjectSet<TEntity>();
					return objectSet.Execute( MergeOption.AppendOnly );
				}
		 */

		public void refresh( TEntity entita ) {
			UnitOfWorkScope.CurrentObjectContext.ObjectContext.Refresh( RefreshMode.StoreWins, entita );
		}
	}
}
