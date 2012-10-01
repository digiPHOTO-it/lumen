using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;

namespace Digiphoto.Lumen.Servizi.EntityRepository {

	public class GiornateRepositorySrvImpl : EntityRepositorySrvImpl<Giornata> {

		public GiornateRepositorySrvImpl() : base() {
		}

		public override Giornata getById( object oid ) {
			DateTime giorno = (DateTime)oid;
			return UnitOfWorkScope.CurrentObjectContext.Giornate.SingleOrDefault( g => g.id.Equals( giorno ) );
		}
	}
}
