using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.UI.Mvvm;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for FotoGallery.xaml
	/// </summary>
	public partial class FotoGallery : UserControlBase {

		public FotoGallery() {
			InitializeComponent();
		}

		#region Proprietà
		private FotoGalleryViewModel fotoGalleryViewModel {
			get {
				return (FotoGalleryViewModel)base.viewModelBase;
			}
		}
		#endregion

		#region ToggleButton per dimensione lato immagine
		private void viewGrandeRadioButton_Checked( object sender, RoutedEventArgs e ) {
			cambiaDimensioneImmagini( Convert.ToDouble( ((RadioButton)sender).Tag ) );
		}

		private void viewMediaRadioButton_Checked( object sender, RoutedEventArgs e ) {
			cambiaDimensioneImmagini( Convert.ToDouble( ((RadioButton)sender).Tag ) );
		}

		private void viewPiccolaRadioButton_Checked( object sender, RoutedEventArgs e ) {
			cambiaDimensioneImmagini( Convert.ToDouble( ((RadioButton)sender).Tag ) );
		}
		#endregion

		#region Metodi

		/// modifico il valore dello slider
		private void cambiaDimensioneImmagini( double newWidth ) {
			dimensioneIconeSlider.Value = newWidth;
		}

		#endregion
	}
}
