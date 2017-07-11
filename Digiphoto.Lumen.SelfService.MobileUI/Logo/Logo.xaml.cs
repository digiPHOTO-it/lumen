using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi.Event;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Digiphoto.Lumen.SelfService.MobileUI
{
    /// <summary>
    /// Interaction logic for Logo.xaml
    /// </summary>
    public partial class Logo : UserControl, IEventManager
    {
        private SelfMainWindow main;

        private SelfServiceClient ssClient;

        public Logo(SelfMainWindow main, SelfServiceClient ssClient)
        {
            InitializeComponent();

            this.ssClient = ssClient;
            this.main = main;

            Servizi.Event.EventManager.Instance.setIEventManager(this);

            imageFoto.Source = FotoSrv.Instance.loadPhoto(ssClient, "Logo", Guid.Empty);

            SelfMainWindow.isShowLogo = true;
            SelfMainWindow.isShowSlideShow = false;
            SelfMainWindow.isShowCarrelli = false;
        }

        private void Home_Click(object sender, System.Windows.Input.TouchEventArgs e)
        {
            Home();
        }

        private void Home_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Home();
        }

        public void Home()
        {
            if (ssClient != null)
            {
                ssClient.Close();
                ssClient = null;
            }

            Application.Current.Shutdown();
        }

        private void Go_Click(object sender, System.Windows.Input.TouchEventArgs e)
        {
            Go();
        }

		private void Go_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Go();
		}

		private void Go(object sender, System.Windows.RoutedEventArgs e)
		{
			Go();
		}

		public void Go()
        {
            if (!SelfMainWindow.isShowCarrelli)
            {
                main.ContentArea.Content = new Carrelli(main, ssClient);
                MoveTimeCounter.Instance.updateLastTime();
            }
        }

        public void Next()
        {
        }

        public void Previous()
        {
        }
    }
}
