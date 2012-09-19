using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Digiphoto.Lumen.Imaging {

	public static class Geometrie {

		/// <summary>
		///   
		///  a : b = x : c
		///   Questa mi serve nel crop e nelle maschere.
		///   Io ho un rettangolo (A) di crop riferita ad una immagine ridotta che vedo nello schermo (B)
		///   voglio applicare il crop ad un'altra immagine (C) di grandezza naturale.
		///   Che area devo croppare ???? (x)
		///   
		///   <param name="a">Rettangolo di crop</param>
		///   <param name="b">Rettangolo con la foto ridotta per stare nello schermo</param>
		///   <param name="c">Dimensione della foto originale a grandezza naturale da croppare</param>
		/// 
		///    A : B = x : C
		///    x = A * C / B
		/// 
		/// </summary>
		public static Rect proporziona( Rect a, Rect b, Size c ) {

			Rect ris = new Rect();
			ris.X = (((a.X -b.X) * c.Width) / b.Width);
			ris.Y = (((a.Y - b.Y) * c.Height) / b.Height);
			ris.Width = ((a.Width * c.Width) / b.Width);
			ris.Height = ((a.Height * c.Height) / b.Height);
			return ris;
		}


		public static Int32Rect proporziona( Int32Rect a, Int32Size b, Int32Size c ) {

			Int32Rect ris = new Int32Rect();
			ris.X = ((a.X * c.Width) / b.Width);
			ris.Y = ((a.Y * c.Height) / b.Height);
			ris.Width = ((a.Width * c.Width) / b.Width);
			ris.Height = ((a.Height * c.Height) / b.Height);
			return ris;
		}
	}
}
