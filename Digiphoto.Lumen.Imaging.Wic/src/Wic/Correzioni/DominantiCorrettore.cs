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

		public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType ) {

			return sourceType == typeof( DominantiEffect );
		}

		public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value ) {

			if( value is DominantiEffect )
				return new Dominante {
					rosso = ((DominantiEffect)value).Red,
					verde = ((DominantiEffect)value).Green,
					blu = ((DominantiEffect)value).Blue
				};
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + value.GetType() + " valore=" + value );
		}

		public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType ) {

			return destinationType.IsAssignableFrom( typeof( DominantiEffect ) );
		}

		public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object objCorrezione, Type destinationType ) {

			if( objCorrezione is Dominante )
				return new DominantiEffect {
					Red = ((Dominante)objCorrezione).rosso,
					Green = ((Dominante)objCorrezione).verde,
					Blue = ((Dominante)objCorrezione).blu
				};
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + objCorrezione.GetType() + " valore=" + objCorrezione );
		}

		public override Type getTypeOfCorrezione() {
			return typeof( Dominante );
		}
	}
}