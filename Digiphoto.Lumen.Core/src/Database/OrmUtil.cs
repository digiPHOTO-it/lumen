using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Digiphoto.Lumen.Core.Database;

namespace Digiphoto.Lumen.Database {

	public static class OrmUtil {


		public static void forseAttacca<T>( string entitySetName, ref T entity ) {
			ObjectContext ctx =  UnitOfWorkScope.CurrentObjectContext.ObjectContext;
			forseAttacca( ctx, entitySetName, ref entity );
		}

		public static void forseAttacca<T>( this ObjectContext context, string entitySetName, ref T entity ) {

			ObjectStateEntry entry;
			
			// Track whether we need to perform an attach
			bool attach = false;
			if( context.ObjectStateManager.TryGetObjectStateEntry( context.CreateEntityKey( entitySetName, entity ), out entry ) ) {
				// Re-attach if necessary
				attach = entry.State == EntityState.Detached;
				// Get the discovered entity to the ref
				entity = (T)entry.Entity;
			} else {
				// Attach for the first time
				attach = true;
			}
			if( attach )
				context.AttachTo( entitySetName, entity );
		}

		public static void Evict( DbContext ctx, Type t, string primaryKeyName, object id ) {

			vediEntitaInCache( ctx, t );

			var cachedEnt =
				ctx.ChangeTracker.Entries().Where( x =>
					ObjectContext.GetObjectType( x.Entity.GetType() ) == t )
					.SingleOrDefault( x => {
					Type entType = x.Entity.GetType();
					object value = entType.InvokeMember( primaryKeyName,
										System.Reflection.BindingFlags.GetProperty, null,
										x.Entity, new object [] { } );

					return value.Equals( id );
				} );

			if( cachedEnt != null )
				ctx.Entry( cachedEnt.Entity ).State = EntityState.Detached;
		}

		public static void vediEntitaInCache( DbContext ctx, Type t ) {

			var cachedEnt = ctx.ChangeTracker.Entries().Where( x =>  ObjectContext.GetObjectType( x.Entity.GetType() ) == t );

			foreach( var ent in cachedEnt ) {
				System.Diagnostics.Trace.WriteLine( "entità = " + ent + " " + ent.Property( "id" ) );
			}

		}

		/// <summary>
		/// Controllo se l'entità è valida
		/// </summary>
		/// <param name="entity"></param>
		/// <returns>true se validata</returns>
		public static bool isValido( object entity ) {
			IValidatableObject ivo = (IValidatableObject)entity;
			return ivo.Validate( null ).Count() == 0;
		}

	}
}
