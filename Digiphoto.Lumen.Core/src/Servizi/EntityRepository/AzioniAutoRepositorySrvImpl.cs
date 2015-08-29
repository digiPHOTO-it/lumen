using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using  System.Data.Entity.Core.Objects;

namespace Digiphoto.Lumen.Servizi.EntityRepository {

	public class AzioniAutomaticheRepositorySrvImpl : EntityRepositorySrvImpl<AzioneAuto> {

		public AzioniAutomaticheRepositorySrvImpl() : base() {
		}

		public override AzioneAuto getById( object oid ) {
			Guid guid = (Guid)oid;
			return UnitOfWorkScope.currentDbContext.AzioniAutomatiche.SingleOrDefault( f => f.id.Equals( guid ) );
		}
	}
}
