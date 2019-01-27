using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using log4net;
using Digiphoto.Lumen.Database;
using  System.Data.Entity.Core.Objects;
using System.Data.Entity.Validation;
using Digiphoto.Lumen.Eventi;
using System.Data.Entity;
using System.Data;

namespace Digiphoto.Lumen.Servizi.EntityRepository {

	public abstract class EntityRepositorySrvImpl<TEntity> : ServizioImpl, IEntityRepositorySrv<TEntity> where TEntity : class {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( EntityRepositorySrvImpl<TEntity> ) );

		public EntityRepositorySrvImpl() {
		}

		public virtual void addNew( TEntity entita ) {

			UnitOfWorkScope.currentDbContext.Set<TEntity>().Add( entita );

			// ObjectSet<TEntity> objectSet = UnitOfWorkScope.currentObjectContext.CreateObjectSet<TEntity>();
			// objectSet.AddObject( entita );
			_giornale.Info( "Creata nuova entità " + entita.GetType() + " " + entita.ToString() );
		}

		public virtual IEnumerable<TEntity> getAll() {

			return UnitOfWorkScope.currentDbContext.Set<TEntity>().AsEnumerable();
			
			// ObjectSet<TEntity> objectSet = UnitOfWorkScope.currentObjectContext.CreateObjectSet<TEntity>();
			// return objectSet.AsEnumerable();
		}

		public virtual TEntity getById( object id ) {
			throw new NotImplementedException();
		}

		public virtual IQueryable<TEntity> Query() {

			return UnitOfWorkScope.currentDbContext.Set<TEntity>().AsQueryable();

			// ObjectSet<TEntity> objectSet = UnitOfWorkScope.currentObjectContext.CreateObjectSet<TEntity>();
			// return objectSet.AsQueryable();
		}

		public virtual IQueryable<TEntity> Query( System.Linq.Expressions.Expression<Func<TEntity, bool>> filter ) {

			return UnitOfWorkScope.currentDbContext.Set<TEntity>().AsQueryable().Where( filter );
			
			// ObjectSet<TEntity> objectSet = UnitOfWorkScope.currentObjectContext.CreateObjectSet<TEntity>();
			// return objectSet.AsQueryable().Where( filter );
		}



		public virtual void update( ref TEntity entita ) {
			update( ref entita, false );
		}

		public virtual void update( ref TEntity entita, bool forzaDaModificare ) {

			// Riattacco l'entità
			OrmUtil.forseAttacca<TEntity>( ref entita );

			// Flaggo l'oggetto come modificato. In questo modo mi assicuro che quando chiamero il SaveChanges, questo verrà aggiornato
			if( forzaDaModificare )
				UnitOfWorkScope.currentObjectContext.ObjectStateManager.ChangeObjectState( entita, EntityState.Modified );
		}

		public virtual void delete( TEntity entita ) {
			ObjectSet<TEntity> objectSet = UnitOfWorkScope.currentObjectContext.CreateObjectSet<TEntity>();
			objectSet.DeleteObject( entita );
			_giornale.Info( "Cancellata entità " + entita.GetType() + " " + entita.ToString() );
		}

		public int saveChanges() {

			// Non fare try-catch. Se fallice deve saltare con eccezione.
			int quanti =  UnitOfWorkScope.currentDbContext.SaveChanges();

			// Notifico tutta l'applicazione che è successo qualcosa
			if( quanti > 0 ) {
				EntityCambiataMsg ecm = new EntityCambiataMsg( this );
				ecm.type = typeof( TEntity );
				pubblicaMessaggio( ecm );
			} else {
				_giornale.Warn( "Salvataggio con zero record. Strano. Controllare" );
			}

			return quanti;

		}

		/*
				public ObjectResult<TEntity> execute() {
					ObjectSet<TEntity> objectSet = UnitOfWorkScope.CurrentObjectContext.ObjectContext.CreateObjectSet<TEntity>();
					return objectSet.Execute( MergeOption.AppendOnly );
				}
		 */

		public void refresh( TEntity entita ) {

			// Se era staccato l'oggetto, allora lo riattacco.
			OrmUtil.forseAttacca<TEntity>( ref entita );

			// Poi lo rinfesco dal db
			UnitOfWorkScope.currentObjectContext.Refresh( RefreshMode.StoreWins, entita );
		}

		/// <summary>
		/// In alcuni casi mi servirebbe un ID tipo una sequenza o qualcosa di auto-generato
		/// </summary>
		/// <returns></returns>
		public virtual object getNextId() {
			return null;
		}

	}
}
