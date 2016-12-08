﻿using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
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
			ssClient = new SelfServiceClient();
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
		}

		/// <summary>
		/// Ricavo l'elenco delle fotografie dal servizio
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonGetFotografie_Click( object sender, RoutedEventArgs e ) {

			var lista = ssClient.getListaFotografie( Guid.NewGuid() );
			listaFotografie.Clear();
			foreach( var fotografiaDto in lista ) {
				listaFotografie.Add( fotografiaDto );
			}
		}


		private void buttonGetImage_Click( object sender, RoutedEventArgs e ) {

			byte[] bytes = ssClient.getImage( Guid.NewGuid() );

			// Salvo il file su disco
			string filename = Path.ChangeExtension( Path.GetTempFileName(), ".jpg" );

			File.WriteAllBytes( filename, bytes );

			ImageSource imageSource = new BitmapImage( new Uri( filename ) );
			imageSource.Freeze();
			imageFoto.Source = imageSource;
		}


	}
}
