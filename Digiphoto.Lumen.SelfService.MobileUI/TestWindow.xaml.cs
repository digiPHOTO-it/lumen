using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.SelfService.MobileUI {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		SelfServiceClient ssClient;

		public ObservableCollection<CarrelloDto> listaCarrelli {
			get;
			set;
		}

		public ObservableCollection<FotografiaDto> listaFotografie {
			get;
			set;
		}

		public MainWindow() {
			
			InitializeComponent();

			listaCarrelli = new ObservableCollection<CarrelloDto>();
			listaFotografie = new ObservableCollection<FotografiaDto>();

			this.DataContext = this;
        }

		private void Window_Loaded( object sender, RoutedEventArgs e ) {

			// Mi connetto con il servizio SelfService.
			ssClient = new SelfServiceClient( "myNetTcpEndPoint" );
            ssClient.Open();

		}

		private void Window_Closed( object sender, EventArgs e ) {
			if( ssClient != null ) {
				ssClient.Close();
				ssClient = null;
			}

		}

		/// <summary>
		/// Ricavo l'elenco dei carrelli dal servizio
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonGetCarrelli_Click( object sender, RoutedEventArgs e ) {

			var lista = ssClient.getListaCarrelli();
			listaCarrelli.Clear();
			foreach( var carrelloDto in lista ) {
				listaCarrelli.Add( carrelloDto );
			}

			MessageBox.Show( "Recuperati " + listaCarrelli.Count + " carrelli." );
		}

		/// <summary>
		/// Ricavo l'elenco delle fotografie dal servizio
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonGetFotografie_Click( object sender, RoutedEventArgs e ) {

			// Ricavo il carrello selezionato
			Guid carrelloId = ((CarrelloDto)listBoxCarrelli.SelectedItem).id;

			// Chiamo il servizio che mi ritorna tutte le fotografie di quel carrello
			var lista = ssClient.getListaFotografie( carrelloId );
			listaFotografie.Clear();
			foreach( var fotografiaDto in lista ) {
				listaFotografie.Add( fotografiaDto );
			}
			MessageBox.Show( "Recuperate " + listaFotografie.Count + " foto." );
		}


		private void buttonGetImage_Click( object sender, RoutedEventArgs e ) {


			byte[] bytes = null;
			string quale = (String)((Button)sender).Tag;
			if( quale == "Provino" ) {
				Guid fotografiaId = ((FotografiaDto)listBoxFotografie.SelectedItem).id;
				bytes = ssClient.getImageProvino( fotografiaId );
			} else if( quale == "Logo" ) {
				bytes = ssClient.getImageLogo();
			} else if( quale == "Risultante" ) {
				Guid fotografiaId = ((FotografiaDto)listBoxFotografie.SelectedItem).id;
				bytes = ssClient.getImage( fotografiaId );
			}

			// Salvo il file su disco
			string filename = Path.ChangeExtension( Path.GetTempFileName(), ".jpg" );

			File.WriteAllBytes( filename, bytes );

			ImageSource imageSource = new BitmapImage( new Uri( filename ) );
			imageSource.Freeze();
			imageFoto.Source = imageSource;

			MessageBox.Show( "size immagine = " + bytes.Length + " bytes" );
		}

		private void buttonMiPiace_Click( object sender, RoutedEventArgs e ) {
			Guid fotografiaId = ((FotografiaDto)listBoxFotografie.SelectedItem).id;
			ssClient.setMiPiace( fotografiaId, true );
			MessageBox.Show( "Impostazione eseguita" );
		}

		private void buttonNonMiPiace_Click( object sender, RoutedEventArgs e ) {
			Guid fotografiaId = ((FotografiaDto)listBoxFotografie.SelectedItem).id;
			ssClient.setMiPiace( fotografiaId, false );
			MessageBox.Show( "Impostazione eseguita" );
		}

		private void buttonGetCarrello_Click( object sender, RoutedEventArgs e ) {
			var carrelloDto = ssClient.getCarrello( new Guid( textCarrelloId.Text ) );
			listaCarrelli.Clear();
			if( carrelloDto != null )
				listaCarrelli.Add( carrelloDto );
			MessageBox.Show( "Recuperato " + listaCarrelli.Count + " carrello." );
		}
	}
}
