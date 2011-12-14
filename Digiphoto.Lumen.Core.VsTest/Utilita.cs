using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Data;
using System.Data.Objects;

namespace Digiphoto.Lumen.Core.VsTest {
	
	public class Utilita {

		const string idMario = "ROSSIMARIO";

		public static Fotografo ottieniFotografoMario2( LumenEntities dbContext ) {

			ObjectQuery<Fotografo> query =
				dbContext.Fotografi.Where( "it.id = @idMario", new ObjectParameter( "idMario", idMario ) );

			Fotografo mario = query.SingleOrDefault<Fotografo>();
			if( mario == null ) {
				mario = new Fotografo();
				mario.id = idMario;
				mario.iniziali = "RM";
				mario.attivo = true;
				mario.cognomeNome = "Rossi Mario";
				dbContext.Fotografi.AddObject( mario );
			}
			return mario;
		}

		// NON FUNZIONA
		public static Fotografo ottieniFotografoMario3( LumenEntities dbContext ) {

			EntityKey key = new EntityKey( "LumenEntities.Fotografi", "id", idMario );
			Fotografo mario;
			Object entity;
			bool trovato = dbContext.TryGetObjectByKey( key, out entity );

			if( !trovato ) {
				mario = new Fotografo();
				mario.id = idMario;
				mario.iniziali = "RM";
				mario.attivo = true;
				mario.cognomeNome = "Rossi Mario";
				dbContext.Fotografi.AddObject( mario );
			} else
				mario = (Fotografo)entity;

			return mario;
		}

		public static Fotografo ottieniFotografoMario( LumenEntities dbContext ) {

			Fotografo mario;

			// ATTENZIONE:
			// incredibile, ma le entità che sono state appena aggiunte con addObject
			// e che non sono state committate, non si vedono nella seguente query
			// CHE SCHIFO!  ...


			// ... quindi sono costretto a riprovare a vedere se esiste nelle entita appena aggiunte o modificate
			mario = dbContext.ObjectStateManager.GetObjectStateEntries( System.Data.EntityState.Added | EntityState.Modified )
								   .Where( e => !e.IsRelationship )
								   .Select( e => e.Entity )
								   .OfType<Fotografo>()
								   .SingleOrDefault( m => m.id.Equals( idMario ) );

			if( mario == null ) {

				mario = dbContext.Fotografi.SingleOrDefault( f => f.id == idMario );

				if( mario == null ) {
					mario = new Fotografo();
					mario.id = idMario;
					mario.iniziali = "RM";
					mario.attivo = true;
					mario.cognomeNome = "Rossi Mario";
					dbContext.Fotografi.AddObject( mario );
				}
			}
			return mario;
		}

		public static FormatoCarta ottieniFormatoCarta( LumenEntities dbContext, string formato ) {

			FormatoCarta fc;

			// Provo a vedere se esiste nelle entita appena aggiunte
			fc = dbContext.ObjectStateManager.GetObjectStateEntries( System.Data.EntityState.Added )
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
					dbContext.FormatiCarta.AddObject( fc );
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
