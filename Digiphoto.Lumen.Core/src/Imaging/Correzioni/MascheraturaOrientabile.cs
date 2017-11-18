using System;


namespace Digiphoto.Lumen.Imaging.Correzioni {

	/// <summary>
	/// Composition di due mascherature: Una Orizzontale, l'altra Verticale.
	/// </summary>
	public class MascheraturaOrientabile : Correzione {

		public Mascheratura mascheraturaH { get; set; }

		public Mascheratura mascheraturaV { get; set; }

	}
}
