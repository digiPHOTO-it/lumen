using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Digiphoto.Lumen.SelfService.MobileUI
{
    /// <summary>
    /// Interaction logic for Logo.xaml
    /// </summary>
    public partial class Logo : UserControl
    {
        private SelfServiceClient ssClient;

        public Logo(SelfServiceClient ssClient)
        {
            InitializeComponent();

            this.ssClient = ssClient;

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
    }
}
