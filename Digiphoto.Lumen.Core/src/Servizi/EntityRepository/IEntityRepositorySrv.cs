using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Digiphoto.Lumen.Servizi.EntityRepository {

	/** Questa interfaccia serve per realizzare le operazioni CRUD sulle entità semplici */
	public interface IEntityRepositorySrv<T> : IServizio {

		// Create
		void addNew( T entita );

		// Read all
		IEnumerable<T> getAll();

		// Read by id
		T getById( object id );

		// Read by quert
		IQueryable<T> Query( Expression<Func<T, bool>> filter );

		// Update
		void update( T entita );

		// Delete
		void delete( T entita );
	}

}
