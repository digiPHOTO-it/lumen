using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using System;
using System.Windows;
using System.Windows.Input;
namespace Digiphoto.Lumen.SelfService.MobileUI {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class SelfMainWindow : Window {

        SelfServiceClient ssClient;

        // Gestisco l'autoExit dalle schermate
        private Point _mousePoint = new Point(0, 0);
        private System.Windows.Threading.DispatcherTimer _MouseTicker = new System.Windows.Threading.DispatcherTimer();

        public static bool isShowLogo = false;
        public static bool isShowCarrelli = false;
        public static bool isShowSlideShow = false;

        public SelfMainWindow() {
			
			InitializeComponent();
            this.DataContext = this;

            this.TouchMove += new EventHandler<TouchEventArgs>(TouchableThing_TouchMove);

            // Mi connetto con il servizio SelfService.
            ssClient = new SelfServiceClient();
            ssClient.Open();

            // AutoExit
            _MouseTicker.Tick += new EventHandler(dispatcherTimer_MouseTicker);
            _MouseTicker.Interval = new TimeSpan(0, 0, 0, 10);
            _MouseTicker.Start();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowLogo();
        }

        private void TouchableThing_TouchMove(object sender, TouchEventArgs e)
        {
            if(!isShowCarrelli)
                ShowCarrelli();
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ShowCarrelli();
            }
        }

        private void Window_Closed( object sender, EventArgs e ) {
			if( ssClient != null ) {
				ssClient.Close();
				ssClient = null;
			}
		}

        #region dispatcherTimer

        private void dispatcherTimer_MouseTicker(object sender, EventArgs e)
        {
            if (_mousePoint != Mouse.GetPosition(this))
            {
                _mousePoint.X = Mouse.GetPosition(this).X;
                _mousePoint.Y = Mouse.GetPosition(this).Y;
            }
            else
            {
                ShowLogo();
            }
        }

        #endregion

        #region Show

        private void ShowLogo()
        {
            this.Content = new Logo(ssClient);
        }

        private void ShowCarrelli()
        {
            this.Content = new Carrelli(ssClient);
        }

        #endregion

    }
}
