using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using System.Windows.Media.Effects;
using Digiphoto.Lumen.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.ComponentModel;
using Digiphoto.Lumen.Servizi.Ritoccare;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {
	
	public class OldMovieCorrettore : Correttore {

		public OldMovieCorrettore() {
		}

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			ImmagineWic iw = (ImmagineWic)immagineSorgente;

			// Non spostare da qui questo vettore. Deve essere istanziato ogni volta, altrimenti romper il cavolo con i thread diversi
			ShaderEffect[] localEffetti = new ShaderEffect[] { new OldMovieEffect() };

			BitmapSource modificata = EffectsUtil.RenderImageWithEffectsToBitmap( iw.bitmapSource, localEffetti );

			return new ImmagineWic( modificata );
		}

		public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType ) {

			return sourceType == typeof( OldMovieEffect );			
		}

		public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value ) {

			if( value is OldMovieEffect )
				return new OldMovie();
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + value.GetType() + " valore=" + value );
		}

		public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType ) {

			return destinationType.IsAssignableFrom( typeof( OldMovieEffect ) );
		}

		public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType ) {

			if( value is OldMovie )
				return new OldMovieEffect();
			else
				throw new NotSupportedException( "Impossibile convertire tipo=" + value.GetType() + " valore=" + value );
		}

	}
}
