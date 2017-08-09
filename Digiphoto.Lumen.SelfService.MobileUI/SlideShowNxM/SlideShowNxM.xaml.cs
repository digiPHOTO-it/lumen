using Digiphoto.Lumen.SelfService.MobileUI.Control;
using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Digiphoto.Lumen.SelfService.MobileUI
{
    /// <summary>
    /// Interaction logic for SlideShow.xaml
    /// </summary>
    public partial class SlideShowNxM : UserControlBase, IEventManager
    {
        private SelfMainWindow main;

        private SelfServiceClient ssClient;

        private FotografoDto fotografo;

        private bool isControlliUtenteAttivi = false;

        private bool isLoadingRisultante = false;

        private int currentPageIndex = 0;

        private static int _PAGE_SIZE = 6;

        // Risultante
        private System.Windows.Threading.DispatcherTimer _RisultantePanelTicker = new System.Windows.Threading.DispatcherTimer();

        // Feedback
        private System.Windows.Threading.DispatcherTimer _FeedbackTicker = new System.Windows.Threading.DispatcherTimer();

        //Controllo Movimento Mouse
        private Point _mousePoint = new Point(0, 0);
        private System.Windows.Threading.DispatcherTimer _MouseTicker = new System.Windows.Threading.DispatcherTimer();

        public ObservableCollection<FotografiaDto> listaCarrelli
        {
            get;
            set;
        }

        private ICollectionView _fotografieCW;
        public ICollectionView fotografieCW
        {
            get
            {
                return _fotografieCW;
            }
            set
            {
                if (_fotografieCW != value)
                {
                    _fotografieCW = value;
                    OnPropertyChanged("fotografieCW");
                }
            }
        }

        public SlideShowNxM(SelfMainWindow main, SelfServiceClient ssClient, FotografoDto fotografo)
        {
            InitializeComponent();

            this.DataContext = this;
            this.ssClient = ssClient;
            this.main = main;
            this.fotografo = fotografo;

            SelfMainWindow.isShowLogo = false;
            SelfMainWindow.isShowSlideShow = true;
            SelfMainWindow.isShowCarrelli = false;

            Servizi.Event.EventManager.Instance.setIEventManager(this);

            _FeedbackTicker.Tick += new EventHandler(dispatcherTimer_FeedbackTicker);
            _FeedbackTicker.Interval = new TimeSpan(0, 0, 0, 1);

            // For Risultante
            _RisultantePanelTicker.Tick += new EventHandler(dispatcherTimer_PanelTicker);
            _RisultantePanelTicker.Interval = new TimeSpan(0, 0, 1);

            //Controllo Mouse
            _MouseTicker.Tick += new EventHandler(dispatcherTimer_MouseTicker);
            _MouseTicker.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _MouseTicker.Start();

            SetMyImagePopStoryBoards();
            isControlliUtenteAttivi = true;

            ShowCurrentPageIndex();

            FotoSrv.Instance.setFotografo(fotografo.id);

            Load();
        }

        private void dispatcherTimer_FeedbackTicker(object sender, EventArgs e)
        {
            //this.LoadingFeedback.Visibility = Visibility.Hidden;
            _FeedbackTicker.Stop();
        }

        #region Image Loaders

        private void Load()
        {
            _RisultantePanelTicker.Stop();
            _RisultantePanelTicker.Start();
        }

        #endregion

        private void dispatcherTimer_PanelTicker(object sender, EventArgs e)
        {
            loadRisultante();
            _RisultantePanelTicker.Stop();
        }

        private void loadRisultante()
        {
            isLoadingRisultante = true;
            _FeedbackTicker.Start();
			isLoadingRisultante = false;
        }

        #region Click

        private void Previous(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void Next(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void Home(object sender, RoutedEventArgs e)
        {
            Home();
        }

        #endregion

        #region Touch

        private void Home(object sender, TouchEventArgs e)
        {
            Home();
        }

        private void Next(object sender, TouchEventArgs e)
        {
            Next();
        }

        private void Previous(object sender, TouchEventArgs e)
        {
            Previous();
        }

        #endregion

        #region Command

        public void Previous()
        {
            _RisultantePanelTicker.Stop();

            currentPageIndex--;
            if (currentPageIndex < 0)
            {
                currentPageIndex = 0;
            }

            ShowCurrentPageIndex();

            _RisultantePanelTicker.Start();
			MoveTimeCounter.Instance.updateLastTime();
        }

        public void Next()
        {
            _RisultantePanelTicker.Stop();
            // This is for Fade In
            currentPageIndex++;
            
            ShowCurrentPageIndex();

            _RisultantePanelTicker.Start();
			MoveTimeCounter.Instance.updateLastTime();
        }

        private void ShowCurrentPageIndex()
        {
            IList fotografie = ssClient.getListaFotografieDelFotografo(fotografo.id, currentPageIndex * _PAGE_SIZE, _PAGE_SIZE);
            if (fotografie.Count==0)
            {
                currentPageIndex = 0;
                fotografie = ssClient.getListaFotografieDelFotografo(fotografo.id, currentPageIndex * _PAGE_SIZE, _PAGE_SIZE);
            }
            fotografieCW = CollectionViewSource.GetDefaultView(fotografie);
        }
    
        public void Home()
        {
            main.ContentArea.Content = new Logo(main, ssClient);
            SelfMainWindow.isShowSlideShow = false;
            MoveTimeCounter.Instance.updateLastTime();
        }

        public void Go()
        {

        }

        private void attivaControlliUtente()
        {
            if (!isControlliUtenteAttivi)
            {
                this.myFadeInStoryboard.Begin(PlaybackPanel);
                isControlliUtenteAttivi = true;
            }
        }
        
        private void disattivaControlliUtente()
        {
            if (isControlliUtenteAttivi && !isLoadingRisultante)
            {
                this.myFadeOutStoryboard.Begin(PlaybackPanel);
                isControlliUtenteAttivi = false;
            }
        }
        
        private void EventMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                Next();
            }
            else
            {
                Previous();
            }
        }

        #endregion

        #region Animations
        
        private Storyboard myFadeInStoryboard = new Storyboard();
        private Storyboard myFadeOutStoryboard = new Storyboard();
        private void SetMyImagePopStoryBoards()
        {
            DoubleAnimation myDoubleAnimation;

            // Ingresso
            myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.To = 1;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(5));
            myDoubleAnimation.Completed += delegate (object sender2, EventArgs e2)
            {
            };
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Border.OpacityProperty));
            myFadeInStoryboard.Children.Add(myDoubleAnimation);

            // Uscita
            myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.To = 0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(8));
            myDoubleAnimation.Completed += delegate (object sender2, EventArgs e2)
            {
            };
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Border.OpacityProperty));
            myFadeOutStoryboard.Children.Add(myDoubleAnimation);
        }
        
        #endregion

        #region Mouse

        private void dispatcherTimer_MouseTicker(object sender, EventArgs e)
        {
            if (_mousePoint != Mouse.GetPosition(this))
            {
                _mousePoint.X = Mouse.GetPosition(this).X;
                _mousePoint.Y = Mouse.GetPosition(this).Y;
                attivaControlliUtente();
            }
            else
            {
                disattivaControlliUtente();
            }
        }

        #endregion

    }

}
