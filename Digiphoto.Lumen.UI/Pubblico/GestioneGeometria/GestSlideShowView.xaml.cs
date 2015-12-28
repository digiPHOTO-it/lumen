using System;
using System.Windows;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.Mvvm;
using System.Diagnostics;

namespace Digiphoto.Lumen.UI.Pubblico {
	/// <summary>
	/// Interaction logic for GestSlideShowView.xaml
	/// </summary>
	public partial class GestSlideShowView : UserControlBase
	{

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
