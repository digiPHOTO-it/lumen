using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Core.VsTest {
	
	public class Utilita {

		public static Fotografo ottieniFotografoMario( LumenEntities dbContext ) {

			Fotografo mario = dbContext.Fotografi.SingleOrDefault( f => f.id == "ROSSIMARIO" );
			if( mario == null ) {
				mario = new Fotografo();
				mario.id = "ROSSIMARIO";
				mario.iniziali = "RM";
				mario.attivo = true;
				mario.cognomeNome = "Rossi Mario";
				dbContext.Fotografi.AddObject( mario );
			}
			return mario;
		}

		public static FormatoCarta ottieniFormatoCartaA4( LumenEntities dbContext ) {

			FormatoCarta fc = dbContext.FormatiCarta.FirstOrDefault<FormatoCarta>( f => f.descrizione == "A4" );
			if( fc == null ) {
				fc = new FormatoCarta();
				fc.id = Guid.NewGuid();
				fc.prezzo = 12;
				fc.descrizione = "A4";
				dbContext.FormatiCarta.AddObject( fc );
			}

			return fc;
		}

	}
}
