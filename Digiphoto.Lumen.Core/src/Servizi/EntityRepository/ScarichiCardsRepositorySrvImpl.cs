using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using  System.Data.Entity.Core.Objects;

namespace Digiphoto.Lumen.Servizi.EntityRepository {

	public class ScarichiCardsRepositorySrvImpl : EntityRepositorySrvImpl<ScaricoCard> {

		public ScarichiCardsRepositorySrvImpl() : base() {
		}

		public override ScaricoCard getById( object oid ) {
			Guid id = (Guid)oid;
			return UnitOfWorkScope.currentDbContext.ScarichiCards.SingleOrDefault( f => f.id.Equals(id) );
		}

	}
}
