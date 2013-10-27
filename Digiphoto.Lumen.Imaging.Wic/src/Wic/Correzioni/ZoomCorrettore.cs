using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Imaging.Correzioni;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {

	public class ZoomCorrettore : Correttore {

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			ImmagineWic imgSorgente = (ImmagineWic)immagineSorgente;
			BitmapSource bitmapSource = imgSorgente.bitmapSource;

			ScaleTransform scaleTransform = new ScaleTransform();
			scaleTransform.ScaleX = scaleTransform.ScaleY = ((Zoom)correzione).fattore;

			// Create the TransformedBitmap to use as the Image source.
			TransformedBitmap tb = new TransformedBitmap( bitmapSource, scaleTransform );

			ImmagineWic modificata = new ImmagineWic( tb );
			return modificata;
		}


		public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType ) {
			return sourceType == typeof( ScaleTransform );
		}

		public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object objCorrezione ) {

			if( objCorrezione is ScaleTransform && ((ScaleTransform)objCorrezione).ScaleX > 0 )
				return new Zoom {
					fattore = ((ScaleTransform)objCorrezione).ScaleX
				};
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + objCorrezione.GetType() + " valore=" + objCorrezione );
		}

		public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType ) {

			return destinationType.IsAssignableFrom( typeof( ScaleTransform ) );
		}

		public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object objCorrezione, Type destinationType ) {

			if( objCorrezione is Zoom )
				return new ScaleTransform {
					ScaleX = ((Zoom)objCorrezione).fattore,
					ScaleY = ((Zoom)objCorrezione).fattore
				};
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + objCorrezione.GetType() + " valore=" + objCorrezione );
		}


	}
}