using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Servizi.EntityRepository;

namespace Digiphoto.Lumen.Servizi.EntityRepository
{
    public class FormatiCartaRepositorySrvImpl : EntityRepositorySrvImpl<FormatoCarta>
    {
        public FormatiCartaRepositorySrvImpl() : base() {
		}

		public override FormatoCarta getById( object oid ) {
			Guid id = (Guid)oid;
			return UnitOfWorkScope.CurrentObjectContext.FormatiCarta.SingleOrDefault( f => f.id == id );
		}
    }
}
