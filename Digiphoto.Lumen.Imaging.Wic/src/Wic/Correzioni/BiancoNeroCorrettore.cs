using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Digiphoto.Lumen.Windows.Media.Effects;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {

	internal class BiancoNeroCorrettore : Correttore {

		private ShaderEffect [] _effetti;

		public BiancoNeroCorrettore() {
			// Purtroppo devo creare un array con un solo elemento. TODO migliorare
			_effetti = new ShaderEffect []  { new GrayscaleEffect() };
		}

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			ImmagineWic iw = (ImmagineWic)immagineSorgente;

			BitmapSource modificata = EffectsUtil.RenderImageWithEffectsToBitmap( iw.bitmapSource, _effetti );

			return new ImmagineWic( modificata );
		}

		public IImmagine applicaOld( IImmagine immagineSorgente, Correzione correzione ) {

			FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();

			// BitmapSource objects like FormatConvertedBitmap can only have their properties
			// changed within a BeginInit/EndInit block.
			newFormatedBitmapSource.BeginInit();

			// Use the BitmapSource object defined above as the source for this new 
			// BitmapSource (chain the BitmapSource objects together).
			ImmagineWic imgWic = (ImmagineWic)immagineSorgente;
			newFormatedBitmapSource.Source = imgWic.bitmapSource;

			// Set the new format to Gray32Float (grayscale).
			newFormatedBitmapSource.DestinationFormat = PixelFormats.Gray32Float;
			newFormatedBitmapSource.EndInit();

			ImmagineWic corretta = new ImmagineWic( newFormatedBitmapSource );
			return corretta;
		}

	}
}
