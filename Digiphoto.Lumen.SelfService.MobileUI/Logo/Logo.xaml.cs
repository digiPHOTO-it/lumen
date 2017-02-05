using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Digiphoto.Lumen.SelfService.MobileUI
{
    /// <summary>
    /// Interaction logic for Logo.xaml
    /// </summary>
    public partial class Logo : UserControl
    {
        private SelfMainWindow main;

        private SelfServiceClient ssClient;

        public Logo(SelfMainWindow main, SelfServiceClient ssClient)
        {
            InitializeComponent();

            this.ssClient = ssClient;
            this.main = main;

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

        private void Home()
        {
            if (ssClient != null)
            {
                ssClient.Close();
                ssClient = null;
            }

            Application.Current.Shutdown();
        }

        private void Go(object sender, System.Windows.Input.TouchEventArgs e)
        {
            Go();
        }

        private void Go()
        {
            if (!SelfMainWindow.isShowCarrelli)
                main.ContentArea.Content = new Carrelli(main, ssClient);
        }
    
    }
}
