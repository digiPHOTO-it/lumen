using Digiphoto.Lumen.Services;
using Digiphoto.Lumen.Services.Fingerprint;
using System;
using System.ServiceModel;
using System.Windows;

namespace Digiphoto.Lumen.Services.Host {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		public MainWindow() {

			InitializeComponent();

			this.DataContext = this;
		}

		private ServiceHost theServiceHost;
		private ServiceHost fingerprintServiceHost;


		private void StartVenditoreServiceButton_Click( object sender, RoutedEventArgs e ) {

			try {
				theServiceHost = new ServiceHost( typeof( SpoolerServiceImpl ) );
				theServiceHost.Open();
				veditoreServiceStatusLabel.Content = theServiceHost.State;

			} catch( Exception ee ) {
				MessageBox.Show( ee.Message );
			}
			
		}

		private void StopVenditoreServiceButton_Click( object sender, RoutedEventArgs e ) {

			try {

				theServiceHost.Close();
				veditoreServiceStatusLabel.Content = theServiceHost.State;

			} catch( Exception ee ) {
				MessageBox.Show( ee.Message );
			}
		}

		private void StartFingerprintServiceButton_Click( object sender, RoutedEventArgs e ) {
			try {
				fingerprintServiceHost = new ServiceHost( typeof( FingerprintServiceImpl ) );
				fingerprintServiceHost.Open();
				fingerprintServiceStatusLabel.Content = fingerprintServiceHost.State;
			} catch( Exception ee ) {
				MessageBox.Show( ee.Message );
			}

		}

		private void StopFingerprintServiceButton_Click( object sender, RoutedEventArgs e ) {
			try {

				fingerprintServiceHost.Close();
				fingerprintServiceStatusLabel.Content = fingerprintServiceHost.State;

			} catch( Exception ee ) {
				MessageBox.Show( ee.Message );
			}
		}
	}
}
