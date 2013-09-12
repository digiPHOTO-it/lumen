using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;

namespace Digiphoto.Lumen.Windows.Media.Effects {

	/// <summary>An effect that controls brightness and contrast.</summary>
	public class LuminositaContrastoEffect : ShaderEffectBase {

		
		public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty( "Input", typeof( LuminositaContrastoEffect ), 0 );
		public static readonly DependencyProperty BrightnessProperty = DependencyProperty.Register( "Brightness", typeof( double ), typeof( LuminositaContrastoEffect ), new UIPropertyMetadata( ((double)(0D)), PixelShaderConstantCallback( 0 ) ) );
		public static readonly DependencyProperty ContrastProperty = DependencyProperty.Register( "Contrast", typeof( double ), typeof( LuminositaContrastoEffect ), new UIPropertyMetadata( ((double)(1.0D)), PixelShaderConstantCallback( 1 ) ) );

		public LuminositaContrastoEffect() : base() {
			
			this.UpdateShaderValue( InputProperty );
			this.UpdateShaderValue( BrightnessProperty );
			this.UpdateShaderValue( ContrastProperty );
		}

		public Brush Input {
			get {
				return ((Brush)(this.GetValue( InputProperty )));
			}
			set {
				this.SetValue( InputProperty, value );
			}
		}
		/// <summary>The brightness offset.</summary>
		public double Brightness {
			get {
				return ((double)(this.GetValue( BrightnessProperty )));
			}
			set {
				this.SetValue( BrightnessProperty, value );
			}
		}
		/// <summary>The contrast multiplier.</summary>
		public double Contrast {
			get {
				return ((double)(this.GetValue( ContrastProperty )));
			}
			set {
				this.SetValue( ContrastProperty, value );
			}
		}
	}
}
