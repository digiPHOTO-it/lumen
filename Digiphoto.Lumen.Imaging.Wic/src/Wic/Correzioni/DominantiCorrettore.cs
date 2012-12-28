using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Windows.Media.Effects;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {
	
	public class DominantiCorrettore : Correttore {


		public DominantiCorrettore() {
		}

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			// Purtroppo devo creare un array con un solo elemento. TODO migliorare
			DominantiEffect de = new DominantiEffect();
			de.Red = ((Dominante)correzione).rosso;
			de.Green = ((Dominante)correzione).verde;
			de.Blue = ((Dominante)correzione).blu;
			ShaderEffect [] _effetti = new ShaderEffect [] { de };

			ImmagineWic iw = (ImmagineWic)immagineSorgente;

			BitmapSource modificata = EffectsUtil.RenderImageWithEffectsToBitmap( iw.bitmapSource, _effetti );

			return new ImmagineWic( modificata );
		}
	}
}