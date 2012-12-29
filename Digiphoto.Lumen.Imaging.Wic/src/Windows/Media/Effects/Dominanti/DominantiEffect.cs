//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice ? stato generato da uno strumento.
//     Versione runtime:4.0.30319.17929
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;


namespace Digiphoto.Lumen.Windows.Media.Effects {
	
	public class DominantiEffect : ShaderEffectBase {

		public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty( "Input", typeof( DominantiEffect ), 0 );
		public static readonly DependencyProperty RedProperty = DependencyProperty.Register( "Red", typeof( double ), typeof( DominantiEffect ), new UIPropertyMetadata( ((double)(0D)), PixelShaderConstantCallback( 0 ) ) );
		public static readonly DependencyProperty GreenProperty = DependencyProperty.Register( "Green", typeof( double ), typeof( DominantiEffect ), new UIPropertyMetadata( ((double)(0D)), PixelShaderConstantCallback( 1 ) ) );
		public static readonly DependencyProperty BlueProperty = DependencyProperty.Register( "Blue", typeof( double ), typeof( DominantiEffect ), new UIPropertyMetadata( ((double)(0D)), PixelShaderConstantCallback( 2 ) ) );

		public DominantiEffect() : base() {

			this.UpdateShaderValue(InputProperty);
			this.UpdateShaderValue(RedProperty);
			this.UpdateShaderValue(GreenProperty);
			this.UpdateShaderValue(BlueProperty);
		}


		public override void reset() {
			base.reset();

			// Forse si potrebbe fare tutto tramite reflection nella classe padre... boh vedremo.
			Red   = (double)RedProperty.GetMetadata( this.DependencyObjectType ).DefaultValue;
			Green = (double)GreenProperty.GetMetadata( this.DependencyObjectType ).DefaultValue;
			Blue  = (double)BlueProperty.GetMetadata( this.DependencyObjectType ).DefaultValue;
		}


		public Brush Input {
			get {
				return ((Brush)(this.GetValue(InputProperty)));
			}
			set {
				this.SetValue(InputProperty, value);
			}
		}
		
		public double Red {
			get {
				return ((double)(this.GetValue(RedProperty)));
			}
			set {
				this.SetValue(RedProperty, value);
			}
		}
		
		public double Green {
			get {
				return ((double)(this.GetValue(GreenProperty)));
			}
			set {
				this.SetValue(GreenProperty, value);
			}
		}
		
		public double Blue {
			get {
				return ((double)(this.GetValue(BlueProperty)));
			}
			set {
				this.SetValue(BlueProperty, value);
			}
		}
	}
}
