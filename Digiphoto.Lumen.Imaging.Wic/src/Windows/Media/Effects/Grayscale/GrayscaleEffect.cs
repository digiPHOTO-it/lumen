﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:4.0.30319.239
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

	public class GrayscaleEffect : ShaderEffectBase {

		public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty( "Input", typeof( GrayscaleEffect ), 0 );
		public static readonly DependencyProperty FactorProperty = DependencyProperty.Register( "Factor", typeof( double ), typeof( GrayscaleEffect ), new UIPropertyMetadata( ((double)(0D)), PixelShaderConstantCallback( 0 ) ) );
		
		public GrayscaleEffect() {

			this.UpdateShaderValue( InputProperty );
			this.UpdateShaderValue( FactorProperty );
		}
		public Brush Input {
			get {
				return ((Brush)(this.GetValue( InputProperty )));
			}
			set {
				this.SetValue( InputProperty, value );
			}
		}
		public double Factor {
			get {
				return ((double)(this.GetValue( FactorProperty )));
			}
			set {
				this.SetValue( FactorProperty, value );
			}
		}
	}
}
