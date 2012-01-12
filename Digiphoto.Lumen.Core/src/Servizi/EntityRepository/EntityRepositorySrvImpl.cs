using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using System.Data.Objects.DataClasses;
using System.Data.Objects;
using log4net;

namespace Digiphoto.Lumen.Servizi.EntityRepository {

	public abstract class EntityRepositorySrvImpl<TEntity> : ServizioImpl, IEntityRepositorySrv<TEntity> where TEntity : class {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( EntityRepositorySrvImpl<TEntity> ) );

		public EntityRepositorySrvImpl() {
		}

		public void addNew( TEntity entita ) {
			ObjectSet<TEntity> objectSet = UnitOfWorkScope.CurrentObjectContext.CreateObjectSet<TEntity>();
			objectSet.AddObject( entita );
			_giornale.Info( "Creata nuova entità " + entita.GetType() + " " + entita.ToString() );
		}

		public IEnumerable<TEntity> getAll() {
			ObjectSet<TEntity> objectSet = UnitOfWorkScope.CurrentObjectContext.CreateObjectSet<TEntity>();
			return objectSet.AsEnumerable();
		}

		public virtual TEntity getById( object id ) {
			throw new NotImplementedException();
		}

		public IQueryable<TEntity> Query( System.Linq.Expressions.Expression<Func<TEntity, bool>> filter ) {
			// TODO da fare non so ancora bene come
			throw new NotImplementedException();
		}

		public void update( TEntity entita ) {
			UnitOfWorkScope.CurrentObjectContext.SaveChanges();
		}

		public void delete( TEntity entita ) {
			ObjectSet<TEntity> objectSet = UnitOfWorkScope.CurrentObjectContext.CreateObjectSet<TEntity>();
			objectSet.DeleteObject( entita );
			_giornale.Info( "Cancellata entità " + entita.GetType() + " " + entita.ToString() );
		}
	}
}
