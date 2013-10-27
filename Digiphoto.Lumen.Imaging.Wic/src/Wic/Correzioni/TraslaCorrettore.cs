using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Imaging.Correzioni;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {

	public class TraslaCorrettore : Correttore {

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			ImmagineWic imgSorgente = (ImmagineWic)immagineSorgente;
			BitmapSource bitmapSource = imgSorgente.bitmapSource;

			TranslateTransform tt = (TranslateTransform)this.ConvertTo( correzione, typeof( TranslateTransform ) );
			
			// Create the TransformedBitmap to use as the Image source.
			TransformedBitmap tb = new TransformedBitmap( bitmapSource, tt );

			ImmagineWic modificata = new ImmagineWic( tb );
			return modificata;

		}

		public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType ) {
			return sourceType == typeof( TranslateTransform );
		}

		public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object objCorrezione ) {

			if( objCorrezione is TranslateTransform ) {
				return new Trasla {
					offsetX = ((TranslateTransform)objCorrezione).X,
					offsetY = ((TranslateTransform)objCorrezione).Y
				};
			} else
				throw new NotSupportedException( "Impossibile convertire tipo=" + objCorrezione.GetType() + " valore=" + objCorrezione );
		}

		public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType ) {

			return destinationType.IsAssignableFrom( typeof( ScaleTransform ) );
		}

		public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object objCorrezione, Type destinationType ) {

			if( objCorrezione is Trasla )
				return new TranslateTransform {
					X = ((Trasla)objCorrezione).offsetX,
					Y = ((Trasla)objCorrezione).offsetY
				};
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + objCorrezione.GetType() + " valore=" + objCorrezione );
		}


	}
}
