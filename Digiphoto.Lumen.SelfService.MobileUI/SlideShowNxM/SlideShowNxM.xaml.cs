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

        private FotografoDto fotografo;
		private string strFaseDelGiorno;

        private bool isControlliUtenteAttivi = false;

        private bool isLoadingRisultante = false;

        private int currentPageIndex = 0;

        private static int _PAGE_SIZE = 6;

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
				this.BorderEmptyFeedback.Visibility = Visibility.Hidden;
				if (_fotografieCW != value)
                {
                    _fotografieCW = value;
					if (_fotografieCW.IsEmpty)
					{
						this.BorderEmptyFeedback.Visibility = Visibility.Visible;
					}
                    OnPropertyChanged("fotografieCW");
                }
            }
        }

        public SlideShowNxM(SelfMainWindow main, FotografoDto fotografo, String faseDelGiorno )
        {
            InitializeComponent();

            this.DataContext = this;
            this.main = main;
            this.fotografo = fotografo;
			this.strFaseDelGiorno = faseDelGiorno;

            SelfMainWindow.isShowLogo = false;
            SelfMainWindow.isShowSlideShow = true;
            SelfMainWindow.isShowCarrelli = false;

            Servizi.Event.EventManager.Instance.setIEventManager(this);

            //Controllo Mouse
            _MouseTicker.Tick += new EventHandler(dispatcherTimer_MouseTicker);
            _MouseTicker.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _MouseTicker.Start();

            SetMyImagePopStoryBoards();
            isControlliUtenteAttivi = true;

            ShowCurrentPageIndex();

            FotoSrv.Instance.setFotografo(fotografo.id);
			FotoSrv.Instance.faseDelGiorno = faseDelGiorno;
		}

        private void loadRisultante()
        {
            isLoadingRisultante = true;
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
            currentPageIndex--;
            if (currentPageIndex < 0)
            {
                currentPageIndex = 0;
            }

            ShowCurrentPageIndex();

			MoveTimeCounter.Instance.updateLastTime();
        }

        public void Next()
        {
            // This is for Fade In
            currentPageIndex++;
            
            ShowCurrentPageIndex();

			MoveTimeCounter.Instance.updateLastTime();
        }

        private void ShowCurrentPageIndex()
        {

			IList fotografie = SSClientSingleton.Instance.getListaFotografieDelFotografo(fotografo.id, strFaseDelGiorno, currentPageIndex * _PAGE_SIZE, _PAGE_SIZE);
            if (fotografie.Count==0)
            {
                currentPageIndex = 0;
                fotografie = SSClientSingleton.Instance.getListaFotografieDelFotografo(fotografo.id, strFaseDelGiorno, currentPageIndex * _PAGE_SIZE, _PAGE_SIZE);
			}
            fotografieCW = CollectionViewSource.GetDefaultView(fotografie);
		}
    
        public void Home()
        {
            main.ContentArea.Content = new Logo(main);
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
