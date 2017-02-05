using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Digiphoto.Lumen.SelfService.MobileUI
{
    /// <summary>
    /// Interaction logic for Carrelli.xaml
    /// </summary>
    public partial class Carrelli : UserControl
    {

        private SelfMainWindow main;

        public static bool _slideShowRun = false;

        private SelfServiceClient ssClient;

        public ObservableCollection<CarrelloDto> listaCarrelli
        {
            get;
            set;
        }

        public ICollectionView CarrelliSalvatiCv
        {
            get;
            private set;
        }

        public Carrelli(SelfMainWindow main, SelfServiceClient ssClient)
        {
            InitializeComponent();

            this.DataContext = this;
            this.ssClient = ssClient;
            this.main = main;

            SelfMainWindow.isShowLogo = false;
            SelfMainWindow.isShowSlideShow = false;
            SelfMainWindow.isShowCarrelli = true;

            listaCarrelli = new ObservableCollection<CarrelloDto>();

            var lista = ssClient.getListaCarrelli();
            Console.WriteLine("Lista Carrelli " + lista.Count());
            listaCarrelli.Clear();
            foreach (var carrelloDto in lista)
            {
                listaCarrelli.Add(carrelloDto);
            }

            CarrelliSalvatiCv = CollectionViewSource.GetDefaultView(listaCarrelli);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _slideShowRun = false;
        }

        public ImageSource ImageSource
        {
            get
            {
                return FotoSrv.Instance.loadPhoto(ssClient, "Logo", Guid.Empty);
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView_Selected(sender);
        }

        private void ListView_TouchDown(object sender, TouchEventArgs e)
        {
            ListView_Selected(sender);
        }

        private void ListView_Selected(object sender)
        {
            if (!SelfMainWindow.isShowSlideShow)
            {
                var item = (sender as ListView).SelectedItem;
                if (item != null)
                {
                    CarrelloDto c = (CarrelloDto)item;
                    main.ContentArea.Content = new SlideShow(main, ssClient, c);
                    MoveTimeCounter.Instance.updateLastTime();
                }
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            {
                if (e.Delta < 0) // wheel down
                {
                    if (myScrollViewer.HorizontalOffset + e.Delta > 0)
                    {
                        myScrollViewer.ScrollToHorizontalOffset(myScrollViewer.HorizontalOffset + e.Delta);
                    }
                    else
                    {
                        myScrollViewer.ScrollToLeftEnd();
                        MoveTimeCounter.Instance.updateLastTime();
                    }
                }
                else //wheel up
                {
                    if (myScrollViewer.ExtentWidth > myScrollViewer.HorizontalOffset + e.Delta)
                    {
                        myScrollViewer.ScrollToHorizontalOffset(myScrollViewer.HorizontalOffset + e.Delta);
                    }
                    else
                    {
                        myScrollViewer.ScrollToRightEnd();
                        MoveTimeCounter.Instance.updateLastTime();
                    }
                }

                //myScrollViewer.ScrollToVerticalOffset(myScrollViewer.ScrollableHeight / 2);
                //myScrollViewer.ScrollToHorizontalOffset(myScrollViewer.ScrollableWidth / 2);

            }
        }

        private void ScrollUp(object sender, RoutedEventArgs e)
        {
            myScrollViewer.PageLeft();
            MoveTimeCounter.Instance.updateLastTime();
        }

        private void ScrollDown(object sender, RoutedEventArgs e)
        {
            myScrollViewer.PageRight();
            MoveTimeCounter.Instance.updateLastTime();
        }

        private void Home_Click(object sender, EventArgs e)
        {
            Home();
        }

        
        private void Home_Click(object sender, TouchEventArgs e)
        {
            Home();
        }
        
        private void Home()
        {
            if (!SelfMainWindow.isShowLogo)
            {
                main.ContentArea.Content = new Logo(main, ssClient);
            }
        }
    }
}
