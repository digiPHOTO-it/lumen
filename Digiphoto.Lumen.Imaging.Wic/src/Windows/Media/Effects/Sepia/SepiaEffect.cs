using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;


namespace Digiphoto.Lumen.Windows.Media.Effects.Sepia {
	
	public class SepiaEffect : ShaderEffectBase {

		public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(SepiaEffect), 0);
	
		public SepiaEffect() {

			this.UpdateShaderValue(InputProperty);
		}
		public Brush Input {
			get {
				return ((Brush)(this.GetValue(InputProperty)));
			}
			set {
				this.SetValue(InputProperty, value);
			}
		}
	}
}
