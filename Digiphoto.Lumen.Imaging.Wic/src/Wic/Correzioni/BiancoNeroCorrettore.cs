using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Digiphoto.Lumen.Windows.Media.Effects;
using System.ComponentModel;
using System.Globalization;

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

		public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType ) {

			return sourceType == typeof( GrayscaleEffect );
		}

		public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value ) {

			if( value is GrayscaleEffect )
				return new BiancoNero();
			else
				return base.ConvertFrom( value );
		}

		public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType ) {

			return destinationType.IsAssignableFrom( typeof( GrayscaleEffect ) );
		}

		public override object ConvertTo( ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object objCorrezione, Type destinationType ) {
			
			if( objCorrezione is BiancoNero )
				return new GrayscaleEffect();
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + objCorrezione.GetType() + " valore=" + objCorrezione );
		}

		public override Type getTypeOfCorrezione() {
			return typeof( BiancoNero );
		}
	}
}
