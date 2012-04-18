using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data;
using System.ComponentModel.DataAnnotations;

namespace Digiphoto.Lumen.Database {

	public static class OrmUtil {

		public static void AttachToOrGet<T>( this ObjectContext context, string entitySetName, ref T entity ) where T : IEntityWithKey {

			ObjectStateEntry entry;
			
			// Track whether we need to perform an attach
			bool attach = false;
			if( context.ObjectStateManager.TryGetObjectStateEntry( context.CreateEntityKey( entitySetName, entity ),	out entry ) ) {
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
