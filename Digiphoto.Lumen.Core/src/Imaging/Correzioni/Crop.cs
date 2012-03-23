using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Digiphoto.Lumen.Imaging.Correzioni {

	public class Crop : Correzione {

		// Cropping rectangle.
		public int x;
		public int y;
		public int w;
		public int h;

		// Coordinate della immagine di riferimento (servirà per riproporzionare sulla immagine vera finale)
		public int imgWidth;
		public int imgHeight;
	}
}
