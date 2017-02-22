﻿using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi;
using Digiphoto.Lumen.SelfService.MobileUI.Servizi.Event;
using System;
using System.Windows;
using System.Windows.Input;
namespace Digiphoto.Lumen.SelfService.MobileUI {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class SelfMainWindow : Window, IEventManager
    {

        SelfServiceClient ssClient;

        // Gestisco l'autoExit dalle schermate
        private Point _mousePoint = new Point(0, 0);
        private System.Windows.Threading.DispatcherTimer _MouseTicker = new System.Windows.Threading.DispatcherTimer();

        public static bool isShowLogo = false;
        public static bool isShowCarrelli = false;
        public static bool isShowSlideShow = false;

        public SelfMainWindow() {
			
			InitializeComponent();
            this.DataContext = this;

            Servizi.Event.EventManager.Instance.setIEventManager(this);

            // Mi connetto con il servizio SelfService.
            ssClient = new SelfServiceClient();
            ssClient.Open();

            // AutoExit
            _MouseTicker.Tick += new EventHandler(dispatcherTimer_MouseTicker);
            _MouseTicker.Interval = new TimeSpan(0, 0, 0, 60);
            _MouseTicker.Start();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowLogo();
        }

        private void Window_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Go();
        }

        private void Window_Closed( object sender, EventArgs e ) {
			if( ssClient != null ) {
				ssClient.Close();
				ssClient = null;
			}
		}

        private void ArrowKey_Press(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter || e.Key == Key.Space)
            {
                Servizi.Event.EventManager.Instance.Go();
            }
            if (e.Key == Key.Right || e.Key == Key.Down)
            {
                Servizi.Event.EventManager.Instance.Next();
            }
            else if (e.Key == Key.Left || e.Key == Key.Up)
            {
                Servizi.Event.EventManager.Instance.Previous();
            }
            else if (e.Key == Key.Escape)
            {
                Servizi.Event.EventManager.Instance.Home();
            }
        }

        #region dispatcherTimer

        private void dispatcherTimer_MouseTicker(object sender, EventArgs e)
        {
            if(!MoveTimeCounter.Instance.evaluateTime()){
                ShowLogo();
            }
        }

        #endregion

        #region Show

        private void ShowLogo()
        {
            if(!isShowLogo)
                ContentArea.Content = new Logo(this, ssClient);
        }

        private void ShowCarrelli()
        {
            if(!isShowCarrelli)
                ContentArea.Content = new Carrelli(this, ssClient);
        }

        #endregion

        public void Go()
        {
            if (isShowLogo)
            {
                ShowCarrelli();
            }
        }

        public void Home()
        {
        }

        public void Next()
        {
        }

        public void Previous()
        {
        }
    }
}