using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core;

namespace Digiphoto.Lumen.Core.Test.Util {
	
	public class Utilita {

		const string idMario = "ROSSIMARIO";

		public static Fotografo ottieniFotografoMario2( LumenEntities dbContext ) {

			Fotografo mario = (from f in dbContext.Fotografi
							   where f.id == idMario
							   select f).SingleOrDefault();

			if( mario == null ) {
				mario = new Fotografo();
				mario.id = idMario;
				mario.iniziali = "RM";
				mario.attivo = true;
				mario.cognomeNome = "Rossi Mario";
				dbContext.Fotografi.Add( mario );
			}
			return mario;
		}

		// NON FUNZIONA
		public static Fotografo ottieniFotografoMario3( LumenEntities dbContext ) {

			EntityKey key = new EntityKey( "LumenEntities.Fotografi", "id", idMario );
			Fotografo mario;
			Object entity;
			bool trovato = ((IObjectContextAdapter)dbContext).ObjectContext.TryGetObjectByKey( key, out entity );

			if( !trovato ) {
				mario = new Fotografo();
				mario.id = idMario;
				mario.iniziali = "RM";
				mario.attivo = true;
				mario.cognomeNome = "Rossi Mario";
				dbContext.Fotografi.Add( mario );
			} else
				mario = (Fotografo)entity;

			return mario;
		}

		public static Fotografo ottieniFotografoMario( LumenEntities dbContext ) {

			Fotografo mario;

			mario = dbContext.Fotografi.SingleOrDefault( f => f.id == idMario );

			if( mario == null ) {

				mario = dbContext.Fotografi.SingleOrDefault( f => f.id == idMario );

				if( mario == null ) {
					mario = new Fotografo();
					mario.id = idMario;
					mario.iniziali = "RM";
					mario.attivo = true;
					mario.cognomeNome = "Rossi Mario";
					dbContext.Fotografi.Add( mario );
				}
			}
			return mario;
		}

		public static FormatoCarta ottieniFormatoCarta( LumenEntities dbContext, string formato ) {

			FormatoCarta fc;

			// Provo a vedere se esiste nelle entita appena aggiunte
			fc = ((IObjectContextAdapter)dbContext).ObjectContext.ObjectStateManager.GetObjectStateEntries( EntityState.Added )
								   .Where( e => !e.IsRelationship )
								   .Select( e => e.Entity )
								   .OfType<FormatoCarta>()
								   .FirstOrDefault( m => m.descrizione.Equals( formato ) );
			if( fc == null ) {

				fc = dbContext.FormatiCarta.FirstOrDefault<FormatoCarta>( f => f.descrizione == formato );

				if( fc == null ) {
					fc = new FormatoCarta();
					fc.id = Guid.NewGuid();
					fc.prezzo = new Decimal( numeroRandom( 3, 15 ) );
					fc.descrizione = formato;
					dbContext.FormatiCarta.Add( fc );
				}
			}

			return fc;
		}

		private static int numeroRandom( int min, int max ) {
			Random random = new Random();
			return random.Next( min, max );
		}
	}
}
