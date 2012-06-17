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
using Digiphoto.Lumen.UI.Mvvm;

namespace Digiphoto.Lumen.UI.Logging
{
    /// <summary>
    /// Interaction logic for NotificationView.xaml
    /// </summary>
	public partial class NotificationView : UserControlBase
    {
        public NotificationView()
        {
            InitializeComponent();
        }

		private void autoScroll(object sender, TextChangedEventArgs e)
		{
			this.TextAreaLog.ScrollToEnd();
		}
    }
}
