using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls.Primitives;

namespace Digiphoto.Lumen.UI.TrayIcon
{
	/// <summary>
	/// Interaction logic for TrayIcon.xaml
	/// </summary>
	public partial class ShowTrayIcon : UserControl
	{
		public enum IconType
		{
			Info,
			Error,
			Warning,
			About,
			AboutCloud
		};

		public ShowTrayIcon()
		{
			InitializeComponent();

			TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
		}

		private bool isClosing = false;

		#region BalloonText dependency property

		/// <summary>
		/// Description
		/// </summary>
		public static readonly DependencyProperty TrayIconTitleProperty =
			DependencyProperty.Register("TrayIconTitle",
										typeof(string),
										typeof(TrayIcon),
										new FrameworkPropertyMetadata(""));

		/// <summary>
		/// Description
		/// </summary>
		public static readonly DependencyProperty TrayIconMessageProperty =
			DependencyProperty.Register("TrayIconMessage",
										typeof(string),
										typeof(TrayIcon),
										new FrameworkPropertyMetadata(""));

		/// <summary>
		/// Description
		/// </summary>
		public static readonly DependencyProperty TypeIconProperty =
			DependencyProperty.Register("TypeIcon",
										typeof(string),
										typeof(TrayIcon),
										new FrameworkPropertyMetadata(""));

		/// <summary>
		/// Description
		/// </summary>
		public static readonly DependencyProperty TrayIconBackgroundProperty =
			DependencyProperty.Register("TrayIconBackground",
										typeof(string),
										typeof(TrayIcon),
										new FrameworkPropertyMetadata(""));

		/// <summary>
		/// A property wrapper for the <see cref="BalloonTextProperty"/>
		/// dependency property:<br/>
		/// Description
		/// </summary>
		public string TrayIconTitle
		{
			get { return (string)GetValue(TrayIconTitleProperty); }
			set { SetValue(TrayIconTitleProperty, value); }
		}


		/// <summary>
		/// A property wrapper for the <see cref="TrayIconMessageProperty"/>
		/// dependency property:<br/>
		/// Description
		/// </summary>
		public string TrayIconMessage
		{
			get { return (string)GetValue(TrayIconMessageProperty); }
			set { SetValue(TrayIconMessageProperty, value); }
		}

		/// <summary>
		/// A property wrapper for the <see cref="TypeIconProperty"/>
		/// dependency property:<br/>
		/// Description
		/// </summary>
		public string TypeIcon
		{
			get { return (string)GetValue(TypeIconProperty); }
			set { SetValue(TypeIconProperty, value); }
		}

		/// <summary>
		/// A property wrapper for the <see cref="TrayIconMessageProperty"/>
		/// dependency property:<br/>
		/// Description
		/// </summary>
		public string TrayIconBackground
		{
			get { return (string)GetValue(TrayIconBackgroundProperty); }
			set { SetValue(TrayIconBackgroundProperty, value); }
		}

		#endregion

		/// <summary>
		/// By subscribing to the <see cref="TaskbarIcon.BalloonClosingEvent"/>
		/// and setting the "Handled" property to true, we suppress the popup
		/// from being closed in order to display the fade-out animation.
		/// </summary>
		private void OnBalloonClosing(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			isClosing = true;
		}


		/// <summary>
		/// Resolves the <see cref="TaskbarIcon"/> that displayed
		/// the balloon and requests a close action.
		/// </summary>
		private void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
		{
			//the tray icon assigned this attached property to simplify access
			TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
			taskbarIcon.CloseBalloon();
		}

		/// <summary>
		/// If the users hovers over the balloon, we don't close it.
		/// </summary>
		private void grid_MouseEnter(object sender, MouseEventArgs e)
		{
			//if we're already running the fade-out animation, do not interrupt anymore
			//(makes things too complicated for the sample)
			if (isClosing) return;

			//the tray icon assigned this attached property to simplify access
			TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
			taskbarIcon.ResetBalloonCloseTimer();
		}


		/// <summary>
		/// Closes the popup once the fade-out animation completed.
		/// The animation was triggered in XAML through the attached
		/// BalloonClosing event.
		/// </summary>
		private void OnFadeOutCompleted(object sender, EventArgs e)
		{
			Popup pp = (Popup)Parent;
			pp.IsOpen = false;
		}

		public void showAbout(string title, String msg, int? sleep)
		{
			showTryIcon(IconType.About, title, msg, sleep);
		}

		public void showAboutCloud(string title, String msg, int? sleep)
		{
			showTryIcon(IconType.AboutCloud, title, msg, sleep);
		}

		public void showError(string title, String msg, int? sleep)
		{
			showTryIcon(IconType.Error, title, msg, sleep);
		}

		public void showInfo(string title, String msg, int? sleep)
		{
			showTryIcon(IconType.Info, title, msg, sleep);
		}

		public void showWarning(string title, String msg, int? sleep)
		{
			showTryIcon(IconType.Warning, title, msg, sleep);
		}

		public void showTryIcon(IconType iconType, string title, String msg, int? sleep)
		{
			Digiphoto.Lumen.UI.TrayIcon.TrayIcon balloon = new Digiphoto.Lumen.UI.TrayIcon.TrayIcon();
			balloon.TrayIconTitle = title;
			balloon.TrayIconMessage = msg;
			balloon.TypeIcon = ConvertTypeToSourcePath(iconType);
			balloon.TrayIconBackground = ConvertTypeToColor(iconType).ToString();
			//show and close after sleep seconds
			TaskbarIcon tb = (TaskbarIcon)FindResource("MyNotifyIcon"); ;
			tb.ShowCustomBalloon(balloon, PopupAnimation.Slide, sleep);
		}

		private string ConvertTypeToSourcePath(IconType iconType)
		{
			string uriTemplate = @"pack://application:,,,/Digiphoto.Lumen.UI;component/Resources/##.png";

			Uri uri = null;

			if (iconType == IconType.Info)
			{
				uri = new Uri(uriTemplate.Replace("##", "info"));
			}

			if (iconType == IconType.Warning)
			{
				uri = new Uri(uriTemplate.Replace("##", "warning"));
			}

			if (iconType == IconType.Error)
			{
				uri = new Uri(uriTemplate.Replace("##", "error"));
			}

			if (iconType == IconType.AboutCloud)
			{
				uri = new Uri(uriTemplate.Replace("##", "about"));
			}

			if (iconType == IconType.AboutCloud)
			{
				uri = new Uri(uriTemplate.Replace("##", "about2"));
			}

			return uri.AbsolutePath;
		}

		private Color ConvertTypeToColor(IconType iconType)
		{
			if (iconType == IconType.Info)
			{
				return Colors.Aqua;
			}

			if (iconType == IconType.Warning)
			{
				return Colors.RoyalBlue;
			}

			if (iconType == IconType.Error)
			{
				return Colors.Red;
			}

			if (iconType == IconType.AboutCloud)
			{
				return Colors.Azure;
			}

			if (iconType == IconType.AboutCloud)
			{
				return Colors.Azure;
			}

			return Colors.Black;
		}

	}
}
