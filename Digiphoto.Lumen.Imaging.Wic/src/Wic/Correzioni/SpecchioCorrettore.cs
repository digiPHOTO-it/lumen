using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Globalization;

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


		public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType ) {

			return sourceType == typeof( ScaleTransform );
		}

		public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object objCorrezione ) {

			if( objCorrezione is ScaleTransform )
				return new Specchio();
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + objCorrezione.GetType() + " valore=" + objCorrezione );
		}

		public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType ) {

			return destinationType.IsAssignableFrom( typeof( ScaleTransform ) );
		}

		public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object objCorrezione, Type destinationType ) {

			if( objCorrezione is Specchio )
				return new ScaleTransform {
					ScaleX = -1
				};
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + objCorrezione.GetType() + " valore=" + objCorrezione );
		}




		public override Type getTypeOfCorrezione() {
			return typeof(Specchio);
		}
	}
}
