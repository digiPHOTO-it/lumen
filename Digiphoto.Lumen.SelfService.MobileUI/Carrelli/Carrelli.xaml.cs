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

        public Carrelli(SelfServiceClient ssClient)
        {
            InitializeComponent();

            this.DataContext = this;
            this.ssClient = ssClient;

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
            var item = (sender as ListView).SelectedItem;
            if (item != null)
            {
                CarrelloDto c = (CarrelloDto)item;
                this.Content = new SlideShow(ssClient, c);
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
                    }
                }

                //myScrollViewer.ScrollToVerticalOffset(myScrollViewer.ScrollableHeight / 2);
                //myScrollViewer.ScrollToHorizontalOffset(myScrollViewer.ScrollableWidth / 2);

            }
        }

        private void ScrollUp(object sender, RoutedEventArgs e)
        {
            myScrollViewer.PageLeft();
        }

        private void ScrollDown(object sender, RoutedEventArgs e)
        {
            myScrollViewer.PageRight();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Content = new Logo(ssClient);
        }

    }
}
