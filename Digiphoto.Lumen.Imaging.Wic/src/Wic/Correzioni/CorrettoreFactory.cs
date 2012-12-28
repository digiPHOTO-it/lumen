using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Servizi.Ritoccare;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {

	internal class CorrettoreFactory : ICorrettoreFactory {

		Dictionary<Type, Correttore> _cache;

		public CorrettoreFactory() {
			_cache = new Dictionary<Type, Correttore>();
		}

		public Correttore creaCorrettore( Type tipoCorrezione ) {

			Correttore correttore = null;

			// Prima controllo in cache
			if( _cache.ContainsKey( tipoCorrezione ) ) {
				correttore = _cache[tipoCorrezione];
			} else {

				// In base al tipo di correzione, istanzio il giusto correttore
				if( tipoCorrezione == typeof( BiancoNero ) ) {
					correttore = new BiancoNeroCorrettore();
				} else if( tipoCorrezione == typeof( Resize ) ) {
					correttore = new ResizeCorrettore();
				} else if( tipoCorrezione == typeof( Sepia ) ) {
					correttore = new SepiaCorrettore();
				} else if( tipoCorrezione == typeof( Ruota ) ) {
					correttore = new RuotaCorrettore();
				} else if( tipoCorrezione == typeof( Specchio ) ) {
					correttore = new SpecchioCorrettore();
				} else if( tipoCorrezione == typeof( Luce ) ) {
					correttore = new LuminositaContrastoCorrettore();
				} else if( tipoCorrezione == typeof( Crop ) ) {
					correttore = new CropCorrettore();
				} else if( tipoCorrezione == typeof( Dominante ) ) {
					correttore = new DominantiCorrettore();
				}

				if( correttore == null )
					throw new ArgumentOutOfRangeException( "correzioneNuova non gestita: " + tipoCorrezione );

				// Metto in cache
				_cache.Add( tipoCorrezione, correttore );
			}

			return correttore;
		}

		public Correttore creaCorrettore<T>() where T : Correzione {
			throw new NotImplementedException();
		}
	}
}
