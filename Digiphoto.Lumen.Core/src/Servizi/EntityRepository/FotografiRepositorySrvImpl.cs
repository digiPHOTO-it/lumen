using System;
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

		public override object getNextId() {

			String ultimoId = UnitOfWorkScope.currentDbContext.Fotografi.Max( f => f.id );

			// Controllo che siano tutte cifre
			int prossimo = 1;
			bool errore = false;
			if( ultimoId != null ) {
				for( int ii = 0; !errore && ii < ultimoId.Length; ii++ )
					if( !Char.IsDigit( ultimoId[ii] ) )
						errore = true;
				if( !errore )
					prossimo = Int32.Parse( ultimoId ) + 1;
			}

			// Faccio un controllo ulteriore di sicurezza
			for( int ii = prossimo; ii < 9999; ii++ ) {
				String test = ii.ToString( "0000" );
				var esito = UnitOfWorkScope.currentDbContext.Fotografi.SingleOrDefault( f => f.id == test );
				if( esito == null ) {
					prossimo = ii;
					break;
				}
				if( ii == 9999 )
					return null;
			}

			return prossimo.ToString( "0000" );
		}
	}
}
