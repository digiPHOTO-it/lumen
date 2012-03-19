using System;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using Digiphoto.Lumen.Windows.Media.Effects;
using System.Windows.Media;


namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for FotoRitocco.xaml
	/// </summary>
	public partial class FotoRitocco : UserControlBase {

		FotoRitoccoViewModel _viewModel;

		public FotoRitocco() {
			
			InitializeComponent();

			_viewModel = (FotoRitoccoViewModel) this.DataContext;
		}

		private void sliderLuminosita_ValueChanged( object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e ) {

			if( _viewModel != null )
				if( _viewModel.forseCambioEffettoCorrente( typeof( LuminositaContrastoEffect ) ) )
					bindaSliderLuminositaContrasto();
		}

		private void sliderContrasto_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {

			if( _viewModel != null )
				if( _viewModel.forseCambioEffettoCorrente( typeof( LuminositaContrastoEffect ) ) )
					bindaSliderLuminositaContrasto();
		}

		private void sliderRuota_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {

			if( _viewModel != null )
				if( _viewModel.forseCambioTrasformazioneCorrente( typeof( RotateTransform ) ) )
					bindaSliderRuota();
		}

		private void bindaSliderRuota() {

			// Bindings con i componenti per i parametri
			Binding binding = new Binding();
			binding.Source = sliderRuota;
			binding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			binding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( _viewModel.trasformazioneCorrente, RotateTransform.AngleProperty, binding );
		}

		/// <summary>
		/// Siccome gli slider sono due, ma l'effetto è uno solo, allora bindo sempre entrambi
		/// </summary>
		private void bindaSliderLuminositaContrasto() {
			bindaSliderLuminosita();
			bindaSliderContrasto();
		}


		/// <summary>
		/// Creo il binding tra la property dell'effetto e lo slider che lo muove
		/// </summary>
		private void bindaSliderContrasto() {
			Binding contrBinding = new Binding();
			contrBinding.Source = sliderContrasto;
			contrBinding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			contrBinding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( _viewModel.luminositaContrastoEffect, LuminositaContrastoEffect.ContrastProperty, contrBinding );
		}

		/// <summary>
		/// Creo il binding tra la property dell'effetto e lo slider che lo muove
		/// </summary>
		private void bindaSliderLuminosita() {
			Binding lumBinding = new Binding();
			lumBinding.Mode = BindingMode.TwoWay;  // Mi serve bidirezionale perché posso resettare tutto dal ViewModel
			lumBinding.Source = sliderLuminosita;
			lumBinding.Path = new PropertyPath( Slider.ValueProperty );
			BindingOperations.SetBinding( _viewModel.luminositaContrastoEffect, LuminositaContrastoEffect.BrightnessProperty, lumBinding );
		}

	}
}
