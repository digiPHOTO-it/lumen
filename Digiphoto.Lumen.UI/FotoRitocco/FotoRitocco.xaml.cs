using System;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using Digiphoto.Lumen.Windows.Media.Effects;
using System.Windows.Media;
using Digiphoto.Lumen.UI.Adorners;
using System.Windows.Documents;
using Digiphoto.Lumen.UI.Util;
using System.Collections.Generic;


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



		/// <summary>
		/// A discapito del nome, questa rappresenta l'unica immagine selezionata,
		/// su cui ho attivato un Selettore Adorner.
		/// </summary>
		public Image imageToCrop {

			get {

				if( itemsControlImmaginiInModifica == null || itemsControlImmaginiInModifica.Items.Count != 1 ) 
					return null;

				// Veder spiegazione qui:
				// http://msdn.microsoft.com/en-us/library/bb613579.aspx

				// Prendo il primo (e l'unico elemento)
				object myElement = itemsControlImmaginiInModifica.Items.GetItemAt( 0 );

				ContentPresenter contentPresenter = (ContentPresenter)itemsControlImmaginiInModifica.ItemContainerGenerator.ContainerFromItem( myElement );
				if( contentPresenter == null )
					return null;

				// Finding image from the DataTemplate that is set on that ContentPresenter
				DataTemplate myDataTemplate = contentPresenter.ContentTemplate;
				return (Image)myDataTemplate.FindName( "imageModTemplate", contentPresenter );
			}
		}

		private void toggleSelector_Checked( object sender, RoutedEventArgs e ) {

			if( _viewModel.attivareSelectorCommand.CanExecute( null ) )
				_viewModel.attivareSelectorCommand.Execute( imageToCrop );
			else
				toggleSelector.IsChecked = false;  // Rifiuto
		}

		private void toggleSelector_Unchecked( object sender, RoutedEventArgs e ) {
			_viewModel.attivareSelectorCommand.Execute( null );  // Qui vorrei spegnere
		}
 
	}
}
