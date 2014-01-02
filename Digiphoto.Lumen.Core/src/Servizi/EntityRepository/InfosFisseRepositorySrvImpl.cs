using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;

namespace Digiphoto.Lumen.Servizi.EntityRepository {

	public class InfosFisseRepositorySrvImpl : EntityRepositorySrvImpl<InfoFissa> {

		public InfosFisseRepositorySrvImpl() : base() {
		}

		public override InfoFissa getById( object id ) {
			return UnitOfWorkScope.currentDbContext.InfosFisse.FirstOrDefault();
		}

	}
}
