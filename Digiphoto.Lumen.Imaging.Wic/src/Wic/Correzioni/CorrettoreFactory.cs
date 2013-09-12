using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Windows.Media.Effects;

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
				} else if( tipoCorrezione == typeof( Gimp ) ) {
					correttore = new GimpCorrettore();
				} else if( tipoCorrezione == typeof( Dominante ) ) {
					correttore = new DominantiCorrettore();
				}

				// Faccio un ultimo tentativo
				if( correttore == null && !typeof( Correzione ).IsAssignableFrom( tipoCorrezione ) ) {
					Type tipo2correz = dimmiQualeCorrezioneCorrispondente( tipoCorrezione );
					correttore = creaCorrettore( tipo2correz );
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


		private Type dimmiQualeCorrezioneCorrispondente( Type obj ) {

			if( obj == typeof(SepiaEffect) )
				return typeof(Sepia);

			if( obj == typeof(LuminositaContrastoEffect) )
				return typeof(Luce);

			if( obj == typeof(GrayscaleEffect) )
				return typeof(BiancoNero);

			if( obj == typeof(DominantiEffect) )
				return typeof(Dominante);

			if( obj == typeof( System.Windows.Media.ScaleTransform ) )
				return typeof( Specchio);

			if( obj == typeof( System.Windows.Media.RotateTransform ) )
				return typeof( Ruota );

			System.Diagnostics.Debugger.Break();

			return null;
		}

	}
}
