using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Digiphoto.Lumen.Imaging.Correzioni {

	/// <summary>
	/// Questa correzione è soltanto un segnaposto che viene serializzato insieme alle altre correzioni
	/// per dire che la foto è stata 
	/// modificata con un editor esterno, e qunindi non è più trattabile con le nostre forze.
	/// </summary>
	public class Gimp : Correzione {
	}
}
