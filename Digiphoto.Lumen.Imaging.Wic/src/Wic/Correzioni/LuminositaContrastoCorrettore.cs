using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Windows.Media.Effects;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {
	
	public class LuminositaContrastoCorrettore : Correttore {


		public LuminositaContrastoCorrettore() {
		}

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			// Purtroppo devo creare un array con un solo elemento. TODO migliorare
			LuminositaContrastoEffect lce = new LuminositaContrastoEffect();
			lce.Brightness = ((Luce)correzione).luminosita;
			lce.Contrast = ((Luce)correzione).contrasto;
			ShaderEffect [] _effetti = new ShaderEffect [] { lce };

			ImmagineWic iw = (ImmagineWic)immagineSorgente;

			BitmapSource modificata = EffectsUtil.RenderImageWithEffectsToBitmap( iw.bitmapSource, _effetti );

			return new ImmagineWic( modificata );
		}
	}
}