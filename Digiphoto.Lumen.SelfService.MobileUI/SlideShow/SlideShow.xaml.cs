using Digiphoto.Lumen.SelfService.MobileUI.Control;
using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Digiphoto.Lumen.SelfService.MobileUI
{
    /// <summary>
    /// Interaction logic for SlideShow.xaml
    /// </summary>
    public partial class SlideShow : UserControlBase
    {
        private SelfServiceClient ssClient;

        private bool isControlliUtenteAttivi = false;

        private bool isLoadingRisultante = false;

        // Risultante
        private System.Windows.Threading.DispatcherTimer _RisultantePanelTicker = new System.Windows.Threading.DispatcherTimer();

        // Feedback
        private System.Windows.Threading.DispatcherTimer _FeedbackTicker = new System.Windows.Threading.DispatcherTimer();

        //Controllo Movimento Mouse
        private Point _mousePoint = new Point(0, 0);
        private System.Windows.Threading.DispatcherTimer _MouseTicker = new System.Windows.Threading.DispatcherTimer();

        private ImageSource _image;
        public ImageSource Image
        {
            get
            {
                return _image;
            }
            set
            {

                if (_image != value)
                {
                    _image = value;
                    this.OnPropertyChanged("Image");
                }
            }
        }

        private int _currentIndex = 0;

        private FotografiaDto[] listaFotografie;

        public SlideShow(SelfServiceClient ssClient, CarrelloDto carrello)
        {
            InitializeComponent();

            this.DataContext = this;
            this.ssClient = ssClient;

            SelfMainWindow.isShowLogo = false;
            SelfMainWindow.isShowSlideShow = true;
            SelfMainWindow.isShowCarrelli = false;

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

            listaFotografie = ssClient.getListaFotografie(carrello.id);

            FotoSrv.Instance.setCarello(carrello.id);

            Load();
        }

        private void dispatcherTimer_FeedbackTicker(object sender, EventArgs e)
        {
            this.MiPiaceFeedback.Visibility = Visibility.Hidden;
            this.NonMiPiaceFeedback.Visibility = Visibility.Hidden;
            this.LoadingFeedback.Visibility = Visibility.Hidden;
            this.SlideShowImage.Opacity = 1;
            _FeedbackTicker.Stop();
        }

        #region Image Loaders

        private void Load()
        {
            _RisultantePanelTicker.Stop();
            // Add to display
            Image = FotoSrv.Instance.loadPhoto(ssClient, "Provino", listaFotografie[0].id);
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
            this.LoadingFeedback.Visibility = Visibility.Visible;
            Image = FotoSrv.Instance.loadPhoto(ssClient, "Risultante", listaFotografie[_currentIndex].id);
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

        private void MiPiace(object sender, RoutedEventArgs e)
        {
            MiPiace();
        }

        private void NonMiPiace(object sender, RoutedEventArgs e)
        {
            NonMiPiace();
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

        private void MiPiace(object sender, TouchEventArgs e)
        {
            MiPiace();
        }

        private void NonMiPiace(object sender, TouchEventArgs e)
        {
            NonMiPiace();
        }

        #endregion

        #region Command

        private void Previous()
        {
            _RisultantePanelTicker.Stop();
            // This is for Fade In
            _currentIndex--;

            if (_currentIndex < 0)
            {
                _currentIndex = listaFotografie.Length - 1;
            }

            _RisultantePanelTicker.Start();
            Image = FotoSrv.Instance.loadPhoto(ssClient, "Provino", listaFotografie[_currentIndex].id);
        }

        private void Next()
        {
            _RisultantePanelTicker.Stop();
            // This is for Fade In
            _currentIndex++;

            if (_currentIndex >= listaFotografie.Length)
            {
                _currentIndex = 0;
            }

            _RisultantePanelTicker.Start();
            Image = FotoSrv.Instance.loadPhoto(ssClient, "Provino", listaFotografie[_currentIndex].id);
        }

        private void Home()
        {
            this.Content = new Logo(ssClient);
            SelfMainWindow.isShowSlideShow = false;
        }

        private void NonMiPiace()
        {
            _FeedbackTicker.Start();
            ssClient.setMiPiace(listaFotografie[_currentIndex].id, false);
            this.SlideShowImage.Opacity = 0.1;
            this.NonMiPiaceFeedback.Visibility = Visibility.Visible;
        }

        private void MiPiace()
        {
            _FeedbackTicker.Start();
            ssClient.setMiPiace(listaFotografie[_currentIndex].id, true);
            this.SlideShowImage.Opacity = 0.1;
            this.MiPiaceFeedback.Visibility = Visibility.Visible;
        }
        
        private void attivaControlliUtente()
        {
            if (!isControlliUtenteAttivi)
            {
                this.myFadeInStoryboard.Begin(PlaybackPanel);
                this.myFadeInStoryboard.Begin(PreferenzePanel);
                isControlliUtenteAttivi = true;
            }
        }
        
        private void disattivaControlliUtente()
        {
            if (isControlliUtenteAttivi && !isLoadingRisultante)
            {
                this.myFadeOutStoryboard.Begin(PlaybackPanel);
                this.myFadeOutStoryboard.Begin(PreferenzePanel);
                isControlliUtenteAttivi = false;
            }
        }
        
        private void MouseWheel(object sender, MouseWheelEventArgs e)
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
