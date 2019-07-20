using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi.Event;
using Digiphoto.Lumen.SelfService.MobileUI.Util;
using System;
using System.Threading;
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

        public Logo(SelfMainWindow main)
        {
            InitializeComponent();
            this.main = main;

            Servizi.Event.EventManager.Instance.setIEventManager(this);

            imageFoto.Source = FotoSrv.Instance.loadPhoto("Logo", Guid.Empty);

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
			SSClientSingleton.Instance.Close();
         
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

			//
			// Non so perché ma questa operazione dura 6 secondi inspiegabilmente
			// sui server in cui c'è la doppia inirizzo di rete
			//
			//
			using( new Clessidra() ) {

				if( !SelfMainWindow.isShowCarrelli ) {
					String setting = SSClientSingleton.Instance.getSettings()["tipo-ricerca"];
					switch( setting ) {
						case "carrelli":
							main.ContentArea.Content = new Carrelli( main );
							break;
						case "fotografi":
							main.ContentArea.Content = new Fotografi( main );
							break;
						default:
							main.ContentArea.Content = new Fotografi( main );
							break;
					}

					MoveTimeCounter.Instance.updateLastTime();
				}
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
