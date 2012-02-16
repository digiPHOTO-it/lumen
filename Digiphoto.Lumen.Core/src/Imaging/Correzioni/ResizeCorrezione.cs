using System;


namespace Digiphoto.Lumen.Imaging.Correzioni {

	public class ResizeCorrezione : Correzione {

		/// <summary>
		/// Non mi interessa di specificare puntualmente le 2 dimensioni nuove (w,h)
		/// perché tanto i miei provini sono sempre di un unica dimensione massima.
		/// L'altra dimensione si deve adeguare.
		/// </summary>
		public long latoMax;
	}
}
