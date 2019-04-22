using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi.Event;
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
    public partial class Fotografi : UserControl , IEventManager
    {

        private SelfMainWindow main;

        public static bool _slideShowRun = false;

        public ObservableCollection<FotografoDto> listaFotografi
        {
            get;
            set;
        }

        public ICollectionView FotografiSalvatiCv
        {
            get;
            private set;
        }
		
		/// <summary>
		/// Mantengo solo una stringa 
		/// Mattino
		/// Pomeriggio
		/// Sera
		/// null = nessun filtro (quindi è tutto il giorno)
		/// </summary>

		public string strFaseDelGiorno {
			get;
			set;
		}

		public DateTime[] giornate {
			get;
			set;
		}

		public DateTime giornataFiltro {

			get;
			set;
		}


		public Fotografi(SelfMainWindow main)
        {
            InitializeComponent();

            this.DataContext = this;
            this.main = main;

            Servizi.Event.EventManager.Instance.setIEventManager(this);

            SelfMainWindow.isShowLogo = false;
            SelfMainWindow.isShowSlideShow = false;
            SelfMainWindow.isShowCarrelli = true;

            listaFotografi = new ObservableCollection<FotografoDto>();

            FotografoDto[] lista = SSClientSingleton.Instance.getListaFotografi();
            Console.WriteLine("Lista Carrelli " + lista.Count());
            listaFotografi.Clear();
            foreach (var fotografoDto in lista)
            {
                listaFotografi.Add(fotografoDto);
            }

            FotografiSalvatiCv = CollectionViewSource.GetDefaultView(listaFotografi);


			// carico la lista delle date
			giornate = new DateTime[7];
			for( int gg = 0; gg < 7; gg++ )
				giornate[gg] = DateTime.Today.AddDays( -1 * gg );
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _slideShowRun = false;
        }

        public ImageSource ImageSource
        {
            get
            {
                return FotoSrv.Instance.loadPhoto("Logo", Guid.Empty);
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
                    FotografoDto c = (FotografoDto)item;
                    main.ContentArea.Content = new SlideShowNxM( main, c, giornataFiltro, strFaseDelGiorno );
                    MoveTimeCounter.Instance.updateLastTime();
                }
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            {
                if (e.Delta < 0) // wheel down
                {
                    if (myScrollViewer2.HorizontalOffset + e.Delta > 0)
                    {
                        myScrollViewer2.ScrollToHorizontalOffset(myScrollViewer2.HorizontalOffset + e.Delta);
                    }
                    else
                    {
                        myScrollViewer2.ScrollToLeftEnd();
                        MoveTimeCounter.Instance.updateLastTime();
                    }
                }
                else //wheel up
                {
                    if (myScrollViewer2.ExtentWidth > myScrollViewer2.HorizontalOffset + e.Delta)
                    {
                        myScrollViewer2.ScrollToHorizontalOffset(myScrollViewer2.HorizontalOffset + e.Delta);
                    }
                    else
                    {
                        myScrollViewer2.ScrollToRightEnd();
                        MoveTimeCounter.Instance.updateLastTime();
                    }
                }
                myScrollViewer2.Focus();

                //myScrollViewer.ScrollToVerticalOffset(myScrollViewer.ScrollableHeight / 2);
                //myScrollViewer.ScrollToHorizontalOffset(myScrollViewer.ScrollableWidth / 2);

            }
        }

        private void ScrollUp(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void ScrollDown(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void Home_Click(object sender, EventArgs e)
        {
            Home();
        }
    
        private void Home_Click(object sender, TouchEventArgs e)
        {
            Home();
        }
        
        public void Home()
        {
            if (!SelfMainWindow.isShowLogo)
            {
                main.ContentArea.Content = new Logo(main);
            }
        }

        public void Go()
        {

        }

        public void Next()
        {
            myScrollViewer2.PageRight();
            MoveTimeCounter.Instance.updateLastTime();
        }

        public void Previous()
        {
            myScrollViewer2.PageLeft();
            MoveTimeCounter.Instance.updateLastTime();
        }

    }
}
