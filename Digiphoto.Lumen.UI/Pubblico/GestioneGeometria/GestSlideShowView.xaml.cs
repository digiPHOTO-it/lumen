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
using Digiphoto.Lumen.UI.Adorners;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.Mvvm;
using System.Diagnostics;

namespace Digiphoto.Lumen.UI.Pubblico
{
	/// <summary>
	/// Interaction logic for GestSlideShowView.xaml
	/// </summary>
	public partial class GestSlideShowView : UserControlBase
	{

		AdornerLayer aLayer;

		bool _isDown;
		bool _isDragging;
		bool selected = false;
		UIElement selectedElement = null;

		Point _startPoint;
		private double _originalLeft;
		private double _originalTop;

		private UserConfigLumen cfg = Configurazione.UserConfigLumen;

		public GestSlideShowView()
		{
			InitializeComponent();
		}

		#region Proprieta

		private GestSlideShowViewModel gestSlideShowViewModel
		{
			get
			{
				return (GestSlideShowViewModel)base.viewModelBase;
			}
		}

		#endregion

		

		private void proprietaMonitorButton_Click(object sender, RoutedEventArgs e) {

			// String path = Environment.GetFolderPath( Environment.SpecialFolder.System );
			String exe = "rundll32.exe";
			String arguments = "shell32.dll,Control_RunDLL desk.cpl,,3";
            Process.Start( exe, arguments );
		}
	}
}
