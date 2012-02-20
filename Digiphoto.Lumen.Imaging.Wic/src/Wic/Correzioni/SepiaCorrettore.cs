using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Windows.Media.Effects.Sepia;
using System.Windows.Media.Effects;
using Digiphoto.Lumen.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {
	
	public class SepiaCorrettore : Correttore {

		private ShaderEffect [] _effetti;

		public SepiaCorrettore() {
			// Purtroppo devo creare un array con un solo elemento. TODO migliorare
			_effetti = new ShaderEffect []  { new SepiaEffect() };
		}

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			ImmagineWic iw = (ImmagineWic)immagineSorgente;

			BitmapSource modificata = EffectsUtil.RenderImageWithEffectsToBitmap( iw.bitmapSource, _effetti );

			return new ImmagineWic( modificata );
		}
	}
}
