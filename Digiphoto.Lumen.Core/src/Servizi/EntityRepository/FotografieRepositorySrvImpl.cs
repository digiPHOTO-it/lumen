using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using  System.Data.Entity.Core.Objects;

namespace Digiphoto.Lumen.Servizi.EntityRepository {

	public class FotografieRepositorySrvImpl : EntityRepositorySrvImpl<Fotografia> {

		public FotografieRepositorySrvImpl() : base() {
		}

		public override Fotografia getById( object oid ) {
			Guid id = (Guid)oid;
			return UnitOfWorkScope.currentDbContext.Fotografie.SingleOrDefault( f => f.id == id );
		}
	}
}
