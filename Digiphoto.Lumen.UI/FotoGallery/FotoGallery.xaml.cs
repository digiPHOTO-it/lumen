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

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for FotoGallery.xaml
	/// </summary>
	public partial class FotoGallery : UserControl {

		private FotoGalleryViewModel _fotoGalleryViewModel;

		public FotoGallery() {
			InitializeComponent();

			this._fotoGalleryViewModel = (FotoGalleryViewModel) this.DataContext;
		}

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

		/// modifico il valore dello slider
		private void cambiaDimensioneImmagini( double newWidth ) {
			dimensioneIconeSlider.Value = newWidth;
		}



	}
}
