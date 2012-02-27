using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {

	public class SpecchioCorrettore : Correttore {

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			ImmagineWic imgSorgente = (ImmagineWic)immagineSorgente;
			BitmapSource bitmapSource = imgSorgente.bitmapSource;

			ScaleTransform scaleTransform = new ScaleTransform();
			scaleTransform.ScaleX = -1;

			// Create the TransformedBitmap to use as the Image source.
			TransformedBitmap tb = new TransformedBitmap( bitmapSource, scaleTransform );

			ImmagineWic modificata = new ImmagineWic( tb );
			return modificata;
		}
	}
}
