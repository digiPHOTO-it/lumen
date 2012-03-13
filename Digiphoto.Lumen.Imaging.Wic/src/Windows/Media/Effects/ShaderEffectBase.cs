using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Effects;

namespace Digiphoto.Lumen.Windows.Media.Effects {

	public abstract class ShaderEffectBase : ShaderEffect {

		protected ShaderEffectBase() {
			this.PixelShader = CreatePixelShader();
		}


		/// <summary>
		/// Ho strutturato le cartelle del progetto in modo tale che posso evitare 
		/// di nominare una per una le risorse, ma determino i nomi a runtime.
		/// </summary>
		/// <returns></returns>
		private PixelShader CreatePixelShader() {

			Uri uriRisorsa = new Uri( calcolaUriRisorsaPs(), UriKind.RelativeOrAbsolute );

			// Qui volendo si può scegliere se lavorare in solo software o solo hardware
			// ps.ShaderRenderMode = ShaderRenderMode.Auto;

			return new PixelShader() { UriSource = uriRisorsa };
		}


		public virtual void reset() {
		}


		/// <summary>
		///   Mi sono inventato una regola per comporre il nome della risorsa per puntare
		///   al compilato di pixel-shader (.PS).
		///   
		//   Questa stringa alla fine viene per esempio :
		//   "/NomeAssembly;component/Windows/Media/Effects/Grayscale/GrayscaleEffect.ps"
		///  
		/// </summary>
		/// <returns></returns>
		private string calcolaUriRisorsaPs() {

			// Compongo la stringa con l'URI per caricare la risorsa del file .ps (pixel shader)
			StringBuilder sbUri = new StringBuilder( "/Digiphoto.Lumen.Imaging.Wic;component/src/Windows/Media/Effects/" );

			// piccolo vezzo. Tolgo la desinenza "Effect" dal nome della classe perchè non mi piace.
			if( this.GetType().Name.EndsWith( "Effect" ) )
				sbUri.Append( this.GetType().Name.Substring( 0, this.GetType().Name.Length - 6 ) );
			else
				sbUri.Append( this.GetType().Name );
			sbUri.Append( "/" );

			sbUri.Append( this.GetType().Name );

			sbUri.Append( ".ps" );
			return sbUri.ToString();
		}


	}
}
