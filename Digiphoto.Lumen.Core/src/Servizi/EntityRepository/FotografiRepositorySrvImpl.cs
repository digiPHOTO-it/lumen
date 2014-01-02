﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using  System.Data.Entity.Core.Objects;

namespace Digiphoto.Lumen.Servizi.EntityRepository {

	public class FotografiRepositorySrvImpl : EntityRepositorySrvImpl<Fotografo> {

		public FotografiRepositorySrvImpl() : base() {
		}

		public override Fotografo getById( object oid ) {
			string id = (string)oid;
			return UnitOfWorkScope.currentDbContext.Fotografi.SingleOrDefault( f => f.id == id );
		}
	}
}
