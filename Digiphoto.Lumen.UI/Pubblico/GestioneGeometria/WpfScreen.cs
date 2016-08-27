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

		/// <summary>
		/// Se trovo un device con lo stesso nome bene.
		/// Altrimenti torno null
		/// </summary>
		/// <param name="deviceName">Nome del device esempio: \\.\DEVICE1</param>
		/// <returns>null se non lo trovo</returns>
		public static WpfScreen GetScreenFrom(string deviceName)
		{
			WpfScreen wpfScreen = null;
			
			foreach (Screen screen in System.Windows.Forms.Screen.AllScreens)
			{
				if(screen.DeviceName.Equals(deviceName)){
					wpfScreen = new WpfScreen(screen);
					break;
				}
			}
			return wpfScreen;
		}

		/// Cerco nel vettore degli schermi alla posizione indicata.
		/// Se l'indice è fuori dal vettore non viene sollevata eccezione, ma ritorno NULL
		/// </summary>
		/// <param name="deviceEnum">indice intero nel vettore degli schermi</param>
		/// <returns>null se non lo trovo</returns>
		public static WpfScreen GetScreenFrom( short deviceEnum )
		{
			WpfScreen wpfScreen = null;

            if( deviceEnum >= 0 && deviceEnum < Screen.AllScreens.Count<Screen>() )
				wpfScreen = new WpfScreen( Screen.AllScreens.ElementAt(deviceEnum) );
				
			return wpfScreen;
		}

		/// <summary>
		/// Prende il primo schermo dal vettore di tutti gli schermi
		/// </summary>
		/// <returns>il primo schermo del vettore (alla posizione 0)</returns>
		public static WpfScreen GetFirstScreen() {
			return new WpfScreen( Screen.AllScreens.First() );
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

		public string ToDebugString() {
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat( "DeviceName={0}, DeviceEnum={1}, IsPrimary={2}\n", DeviceName, deviceEnum, IsPrimary );
			sb.AppendFormat( "WorkingArea={0}, displayResolution={1}", WorkingArea, DeviceBounds );
			return sb.ToString();
		}

	}
}
