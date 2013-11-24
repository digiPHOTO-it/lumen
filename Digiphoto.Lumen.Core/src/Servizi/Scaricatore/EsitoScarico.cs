using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Digiphoto.Lumen.Servizi.Scaricatore {

	public class EsitoScarico {

		public EsitoScarico() {
			fotoDaLavorare = new List<FileInfo>();
			tempo = DateTime.Now;
		}

		public IList<FileInfo> fotoDaLavorare {
			get;
			set;
		}

		public bool riscontratiErrori {
			get;
			set;
		}

		/// <summary>
		/// Indica il numero di foto totali scaricate dalla memory card / HD.
		/// E' un progressivo che viene incrementato di 20 unita per volta
		/// </summary>
		public int totFotoScaricateProg
		{
			get;
			set;
		}

		/// <summary>
		/// Indica il numero di foto totali scaricate dalla memory card / HD.
		/// </summary>
		public int totFotoScaricate
		{
			get;
			set;
		}

		/// <summary>
		/// Indica il numero di foto provinate.
		/// E' un progressivo che viene incrementato di 20 unita per volta
		/// </summary>
		public int totFotoProvinateProg
		{
			get;
			set;
		}

		/// <summary>
		/// Indica il numero di foto provinate.
		/// </summary>
		public int totFotoProvinate
		{
			get;
			set;
		}

		public int totFotoCopiateOk {
			get;
			set;
		}

		public int totFotoNonCopiate {
			get;
			set;
		}

		public int totFotoNonEliminate {
			get;
			set;
		}

		/// <summary>
		/// Questo è il tempo di inizio dello scarico.
		/// Questo valore verrà anche copiato e persistito sulla entità "Fotografia"
		/// in modo da creare un legame tra lo scarico e tutte le sue foto.
		/// </summary>
		public DateTime tempo {
			get;
			set;
		}
		
	}
}
