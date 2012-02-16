using Digiphoto.Lumen.Imaging.Correzioni;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Digiphoto.Lumen.Imaging.Correzioni;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {

	/// <summary>
	/// 
	/// </summary>
	public class RuotaCorrettore : Correttore {


		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			ImmagineWic imgSorgente = (ImmagineWic)immagineSorgente;
			RuotaCorrezione ruotaCorrezione = (RuotaCorrezione)correzione;

			// Create the TransformedBitmap to use as the Image source.
			TransformedBitmap tb = new TransformedBitmap();
			
			// Properties must be set between BeginInit and EndInit calls.
			tb.BeginInit();
			
			tb.Source = imgSorgente.bitmapSource;

			// Set image rotation.
			RotateTransform transform = new RotateTransform( (double) ruotaCorrezione.gradi );
			tb.Transform = transform;
			
			tb.EndInit();

			ImmagineWic modificata = new ImmagineWic( tb );
			return modificata;
		}

			
	}
}
