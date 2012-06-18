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
using System.Linq.Expressions;

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
			bool trovato = context.ObjectStateManager.TryGetObjectStateEntry( entity, out entry );
			
			if( trovato ) {
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
				System.Diagnostics.Trace.WriteLine( "entità = " + ent.Entity + "\tstato=" + ent.State + "\tid=" + ent.Property( "id" ) );
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


		private static Expression<Func<TElement, bool>> GetWhereInExpression<TElement, TValue>( Expression<Func<TElement, TValue>> propertySelector, IEnumerable<TValue> values ) {
			ParameterExpression p = propertySelector.Parameters.Single();
			if( !values.Any() )
				return e => false;

			var equals = values.Select( value => (Expression)Expression.Equal( propertySelector.Body, Expression.Constant( value, typeof( TValue ) ) ) );
			var body = equals.Aggregate<Expression>( ( accumulate, equal ) => Expression.Or( accumulate, equal ) );

			return Expression.Lambda<Func<TElement, bool>>( body, p );
		}

		/// <summary> 
		/// Return the element that the specified property's value is contained in the specifiec values 
		/// </summary> 
		/// <typeparam name="TElement">The type of the element.</typeparam> 
		/// <typeparam name="TValue">The type of the values.</typeparam> 
		/// <param name="source">The source.</param> 
		/// <param name="propertySelector">The property to be tested.</param> 
		/// <param name="values">The accepted values of the property.</param> 
		/// <returns>The accepted elements.</returns> 
		public static IQueryable<TElement> WhereIn<TElement, TValue>( this IQueryable<TElement> source, Expression<Func<TElement, TValue>> propertySelector, params TValue [] values ) {
			return source.Where( GetWhereInExpression( propertySelector, values ) );
		}

		/// <summary> 
		/// Return the element that the specified property's value is contained in the specifiec values 
		/// </summary> 
		/// <typeparam name="TElement">The type of the element.</typeparam> 
		/// <typeparam name="TValue">The type of the values.</typeparam> 
		/// <param name="source">The source.</param> 
		/// <param name="propertySelector">The property to be tested.</param> 
		/// <param name="values">The accepted values of the property.</param> 
		/// <returns>The accepted elements.</returns> 
		public static IQueryable<TElement> WhereIn<TElement, TValue>( this IQueryable<TElement> source, Expression<Func<TElement, TValue>> propertySelector, IEnumerable<TValue> values ) {
			return source.Where( GetWhereInExpression( propertySelector, values ) );
		}


		/// <summary>
		/// Visto che la IN non sempre funziona, la sostituisco con una sfilza di OR.
		/// </summary>
		public static Expression<Func<TElement, bool>> BuildOrExpression<TElement, TValue>(	Expression<Func<TElement, TValue>> valueSelector, IEnumerable<TValue> values ) {
			if( null == valueSelector )
				throw new ArgumentNullException( "valueSelector" );
			if( null == values )
				throw new ArgumentNullException( "values" );
			ParameterExpression p = valueSelector.Parameters.Single();

			if( !values.Any() )
				return e => false;

			var equals = values.Select( value =>
				(Expression)Expression.Equal(
					 valueSelector.Body,
					 Expression.Constant(
						 value,
						 typeof( TValue )
					 )
				)
			);
			var body = equals.Aggregate<Expression>(
					 ( accumulate, equal ) => Expression.Or( accumulate, equal )
			 );

			return Expression.Lambda<Func<TElement, bool>>( body, p );
		}

	}
}
