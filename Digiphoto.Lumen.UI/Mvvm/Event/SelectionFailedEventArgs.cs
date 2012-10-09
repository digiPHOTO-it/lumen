using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Digiphoto.Lumen.UI.Mvvm.Event
{
	public class SelectionFailedEventArgs : RoutedEventArgs
	{
		private int _maxNumSelected;

		public SelectionFailedEventArgs(int maxNumSelected)
		{
			this._maxNumSelected = maxNumSelected;
		}

		public int maxNumSelected
		{
			get { return _maxNumSelected; }
		}
	}
}
