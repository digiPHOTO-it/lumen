using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Linq;
using  System.Data.Entity.Core.Objects;

namespace Digiphoto.Lumen.Servizi.EntityRepository {

	/** Questa interfaccia serve per realizzare le operazioni CRUD sulle entità semplici */
	public interface IEntityRepositorySrv<T> : IServizio where T : class  {

		// Create
		void addNew( T entita );

		// Read all
		IEnumerable<T> getAll();

		// Read by id
		T getById( object id );

		// Read by quert
		IQueryable<T> Query( Expression<Func<T, bool>> filter );
		IQueryable<T> Query();

		/// <summary>
		/// Update in realtà non fa nulla. Si limita a riattaccare 
		/// una entità nel caso sia staccata.
		/// Occorre poi fare un SaveChanges separato.
		/// </summary>
		/// <param name="entita">L'entità da riattaccare</param>
		void update( ref T entita );
		void update( ref T entita, bool forzaDaModificare );

		/// <summary>
		/// Delete in realtà non fa nulla.
		/// Occorre poi fare un SaveChanges separato.
		/// </summary>
		/// <param name="entita">L'entità da cancellare</param>

		void delete( T entita );

		/// <summary>
		/// Attenzione che questo metodo salva tutto, 
		/// e non solo le modifiche fatte da questo servizio.
		/// Tutte le entità che sono pronte per essere salvate, subiscono
		/// la modifica.
		/// </summary>
		/// <returns>Il numero di entità coinvolte</returns>
		int saveChanges();

		/// <summary>
		/// Vince sempre lo store
		/// </summary>
		void refresh( T entita );

		object getNextId();
	}

}
