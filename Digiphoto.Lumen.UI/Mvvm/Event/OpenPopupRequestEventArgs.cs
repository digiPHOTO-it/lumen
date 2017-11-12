using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Digiphoto.Lumen.UI.Mvvm.Event
{
	/// <summary>
	/// Evento che viene lanciato dal viewModel per chiedere alla View di aprire una Popup
	/// </summary>
	public class OpenPopupRequestEventArgs : RoutedEventArgs
	{
		public object param;

		public string requestName { get; set; }

		public ClosableWiewModel viewModel { get; set; }
	}
}
