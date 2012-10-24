using System;
using System.Collections;
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
using Digiphoto.Lumen.UI.ScreenCapture;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for FotoGallery.xaml
	/// </summary>
	public partial class FotoGallery : UserControlBase {

		public FotoGallery() {
			InitializeComponent();

			DataContextChanged += new DependencyPropertyChangedEventHandler(fotoGallery_DataContextChanged);
        }

		void fotoGallery_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {
			associaDialogProvider();
		}

		#region Proprietà
		private FotoGalleryViewModel fotoGalleryViewModel {
			get {
				return (FotoGalleryViewModel)base.viewModelBase;
			}
		}
		#endregion

		#region ToggleButton per dimensione lato immagine

		private void radioButtonQuanteNeVedo_Checked( object sender, RoutedEventArgs e ) {
			
			int quanteRighe = Convert.ToInt16( ((Control)sender).Tag );
			if( quanteRighe > 0 ) {
				double dimensione = (LsImageGallery.ActualHeight / quanteRighe) - 6;
				cambiaDimensioneImmagini( dimensione );
			}
		}

		#endregion

		#region Metodi

		/// modifico il valore dello slider
		private void cambiaDimensioneImmagini( double newWidth ) {
			dimensioneIconeSlider.Value = newWidth;
		}

		#endregion

		private void oggiButton_Click( object sender, RoutedEventArgs e ) {
			calendario.SelectedDates.Clear();
			calendario.SelectedDates.AddRange( fotoGalleryViewModel.oggi, fotoGalleryViewModel.oggi );
		}

		private void ieriButton_Click( object sender, RoutedEventArgs e ) {
			calendario.SelectedDates.Clear();
			TimeSpan unGiorno = new TimeSpan(1,0,0,0);
			DateTime ieri = fotoGalleryViewModel.oggi.Subtract( unGiorno );
			calendario.SelectedDates.AddRange( ieri, ieri );
		}

		private void ieriOggiButton_Click( object sender, RoutedEventArgs e ) {
			calendario.SelectedDates.Clear();
			TimeSpan unGiorno = new TimeSpan( 1, 0, 0, 0 );
			DateTime ieri = fotoGalleryViewModel.oggi.Subtract( unGiorno );
			calendario.SelectedDates.AddRange( ieri, fotoGalleryViewModel.oggi );
		}

		private void calendario_SelectedDatesChanged( object sender, SelectionChangedEventArgs e ) {
			IList giorni = e.AddedItems;
			
			if( giorni.Count > 0 ) {

				// A seconda di come si esegue la selezione, il range può essere ascendente o discendente.
				// A me serve sempre prima la più piccola poi la più grande
				DateTime aa = (DateTime) giorni[0];
				DateTime bb = (DateTime) giorni[giorni.Count-1];

				// Metto sempre per prima la data più piccola
				fotoGalleryViewModel.paramCercaFoto.giornataIniz = minDate( aa, bb );
				fotoGalleryViewModel.paramCercaFoto.giornataFine = maxDate( aa, bb );
			}
		}

		public static DateTime minDate( DateTime aa, DateTime bb ) {
			return aa > bb ? bb : aa;
		}
		public static DateTime maxDate( DateTime aa, DateTime bb ) {
			return aa > bb ? aa : bb;
		}

		/// <summary>
		/// Tramite il doppio click sulla foto, mando direttamente in modifica quella immagine.
		/// </summary>
		private void listBoxItemImageGallery_MouseDoubleClick( object sender, RoutedEventArgs e ) {

			ListBoxItem lbItem = (ListBoxItem) sender;
			fotoGalleryViewModel.mandareInModificaImmediata( lbItem.Content as Fotografia );
		}

		private void LsImageGallery_PreviewMouseRightButtonDown( object sender, MouseButtonEventArgs e ) {
			// Questo mi evita di selezionare la foto quando clicco con il destro.
			e.Handled = true;
		}

	}
}
