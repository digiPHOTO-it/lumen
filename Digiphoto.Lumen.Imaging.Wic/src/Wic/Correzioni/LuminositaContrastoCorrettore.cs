using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Windows.Media.Effects;
using System.ComponentModel;
using System.Globalization;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {
	
	public class LuminositaContrastoCorrettore : Correttore {


		public LuminositaContrastoCorrettore() {
		}

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			// Purtroppo devo creare un array con un solo elemento. TODO migliorare
			ShaderEffect lce = (ShaderEffect)ConvertTo( correzione, typeof(ShaderEffect) );
			ShaderEffect [] _effetti = new ShaderEffect [] { lce };

			ImmagineWic iw = (ImmagineWic)immagineSorgente;

			BitmapSource modificata = EffectsUtil.RenderImageWithEffectsToBitmap( iw.bitmapSource, _effetti );

			return new ImmagineWic( modificata );
		}

		public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType ) {

			return sourceType == typeof( LuminositaContrastoEffect );
		}

		public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value ) {

			if( value is LuminositaContrastoEffect )
				return new Luce {
					luminosita = ((LuminositaContrastoEffect)value).Brightness,
					contrasto = ((LuminositaContrastoEffect)value).Contrast
				};
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + value.GetType() + " valore=" + value );
		}

		public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType ) {

			return destinationType.IsAssignableFrom( typeof( LuminositaContrastoEffect ) );
		}

		public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object objCorrezione, Type destinationType ) {

			if( objCorrezione is Luce )
				return new LuminositaContrastoEffect {
					Brightness = ((Luce)objCorrezione).luminosita,
					Contrast = ((Luce)objCorrezione).contrasto
				};
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + objCorrezione.GetType() + " valore=" + objCorrezione );
		}


		public override Type getTypeOfCorrezione() {
			return typeof(Luce);
		}
	}
}