using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Digiphoto.Lumen.UI.Pubblico.GestioneGeometria
{
	public class ParamSlideShowGeom : INotifyPropertyChanged
	{
		private short _deviceEnum;
		public short deviceEnum
		{
			get
			{
				return _deviceEnum;
			}
			set
			{
				if (_deviceEnum != value)
				{
					_deviceEnum = value;
					OnPropertyChanged("deviceEnum");
				}
			}
		}

		private int _screenHeight;
		public int screenHeight
		{
			get
			{
				return _screenHeight;
			}
			set
			{
				if (_screenHeight != value)
				{
					_screenHeight = value;
					OnPropertyChanged("screenHeight");
				}
			}
		}

		private int _screenWidth;
		public int screenWidth
		{
			get
			{
				return _screenWidth;
			}
			set
			{
				if (_screenWidth != value)
				{
					_screenWidth = value;
					OnPropertyChanged("screenWidth");
				}
			}
		}

		private bool _fullScreen;
		public bool fullScreen
		{
			get
			{
				return _fullScreen;
			}
			set
			{
				if (_fullScreen != value)
				{
					_fullScreen = value;
					OnPropertyChanged("fullScreen");
				}
			}
		}

		private int _slideHeight;
		public int slideHeight
		{
			get
			{
				return _slideHeight;
			}
			set
			{
				if (_slideHeight != value)
				{
					_slideHeight = value;
					OnPropertyChanged("slideHeight");
				}
			}
		}

		private int _slideWidth;
		public int slideWidth
		{
			get
			{
				return _slideWidth;
			}
			set
			{
				if (_slideWidth != value)
				{
					_slideWidth = value;
					OnPropertyChanged("slideWidth");
				}
			}
		}

		private int _slideTop;
		public int slideTop
		{
			get
			{
				return _slideTop;
			}
			set
			{
				if (_slideTop != value)
				{
					_slideTop = value;
					OnPropertyChanged("slideTop");
				}
			}
		}

		private int _slideLeft;
		public int slideLeft
		{
			get
			{
				return _slideLeft;
			}
			set
			{
				if (_slideLeft != value)
				{
					_slideLeft = value;
					OnPropertyChanged("slideLeft");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		// Create the OnPropertyChanged method to raise the event
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
