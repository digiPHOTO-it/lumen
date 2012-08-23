using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;

namespace Digiphoto.Lumen.UI.Pubblico.GestioneGeometria
{
	public class WpfScreen
	{
		public static IEnumerable<WpfScreen> AllScreens()
		{
			foreach (Screen screen in System.Windows.Forms.Screen.AllScreens)
			{
				yield return new WpfScreen(screen);
			}
		}

		public static WpfScreen GetScreenFrom(Window window)
		{
			WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
			Screen screen = System.Windows.Forms.Screen.FromHandle(windowInteropHelper.Handle);
			WpfScreen wpfScreen = new WpfScreen(screen);
			return wpfScreen;
		}

		public static WpfScreen GetScreenFrom(System.Windows.Point point)
		{
			int x = (int)Math.Round(point.X);
			int y = (int)Math.Round(point.Y);
			// are x,y device-independent-pixels ??
			System.Drawing.Point drawingPoint = new System.Drawing.Point(x, y);
			Screen screen = System.Windows.Forms.Screen.FromPoint(drawingPoint);
			WpfScreen wpfScreen = new WpfScreen(screen);
			return wpfScreen;
		}

		public static WpfScreen GetScreenFrom(string deviceName)
		{
			WpfScreen wpfScreen = new WpfScreen(System.Windows.Forms.Screen.AllScreens.First());
			
			foreach (Screen screen in System.Windows.Forms.Screen.AllScreens)
			{
				if(screen.DeviceName.Equals(deviceName)){
					return new WpfScreen(screen);
				}
			}
			return wpfScreen;
		}

		public static WpfScreen GetScreenFrom(short deviceEnum)
		{
			WpfScreen wpfScreen = new WpfScreen(System.Windows.Forms.Screen.AllScreens.First());

			if (deviceEnum != 0 && deviceEnum < System.Windows.Forms.Screen.AllScreens.Count<Screen>())
			{
				wpfScreen = new WpfScreen(System.Windows.Forms.Screen.AllScreens.ElementAt(deviceEnum));
			}

			return wpfScreen;
		}

		public static WpfScreen Primary
		{
			get
			{
				return new WpfScreen(System.Windows.Forms.Screen.PrimaryScreen);
			}
		}

		private readonly Screen screen;

		internal WpfScreen(System.Windows.Forms.Screen screen)
		{
			this.screen = screen;
		}

		public Rect DeviceBounds
		{
			get
			{
				return this.GetRect(this.screen.Bounds);
			}
		}

		public Rect WorkingArea
		{
			get
			{
				return this.GetRect(this.screen.WorkingArea);
			}
		}

		public short deviceEnum
		{
			get
			{
				short index = 0;
				foreach (Screen screen in System.Windows.Forms.Screen.AllScreens)
				{
					if (screen.DeviceName.Equals(this.DeviceName))
					{
						return index;
					}
					index++;
				}
				return 0;
			}
		}

		private Rect GetRect(Rectangle value)
		{
			// should x, y, width, hieght be device-independent-pixels ??
			return new Rect
							  {
								  X = value.X,
								  Y = value.Y,
								  Width = value.Width,
								  Height = value.Height
							  };
		}

		public bool IsPrimary
		{
			get
			{
				return this.screen.Primary;
			}
		}

		public string DeviceName
		{
			get
			{
				return this.screen.DeviceName;
			}
		}
	}
}
