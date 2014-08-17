using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.SelfService {

	/// <summary>
	/// Questa è una classe di trasporto dati (Data Transfer Object) per non dover serializzare 
	/// tutto l'oggetto del modello (Fotografia) ed evitare anche il problema dei proxy delle associazioni serializzati.
	/// </summary>
	public class FotografiaDto {

		public Guid id { get; set; }
		public string nomeFotografo { get; set; }
		public int numero { get; set; }
		public DateTime giornata { get; set; }

	}
}
