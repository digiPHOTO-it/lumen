using Digiphoto.Lumen.Imaging.Correzioni;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Imaging.Correzioni {
	
	public class Scritta : ImgOverlay {

		public string testo { set; get; }

		public int fontSize { set; get; }

		public string fontFamily { set; get; }

		#region Bordo del font

		/// <summary>
		/// larghezza del bordo del font
		/// </summary>
		public int strokeThickness { set; get; }

		/// <summary>
		/// colore del bordo del font : stringa nel formato "#F5F7F8"
		/// </summary>
		public string strokeColor { set; get; }

		#endregion Bordo del font

		#region Corpo del font

		/// <summary>
		/// colore del corpo interno del font : stringa nel formato "#F5F7F8"
		/// </summary>
		public string fillColor { set; get; }

		/// <summary>
		/// Immagine di riempimento del corpo del font : stringa nel formato "riempimento1.jpg" senza path
		/// </summary>
		public string fillImage { get; set; }

		#endregion Corpo del font


		/// <summary>
		/// Posizione X della scritta rispetto alla immagine che la contiene
		/// </summary>
		public int left { set; get; }

		/// <summary>
		/// Posizione Y della scritta rispetto alla immagine che la contiene
		/// </summary>
		public int top { set; get; }

		/// <summary>
		/// Larghezza della scrittta comprensiva delle trasformazioni di zoom
		/// </summary>
		public int width { set; get; }

		/// <summary>
		/// Altezza della scrittta comprensiva delle trasformazioni di zoom
		/// </summary>
		public int height { set; get; }

		/// <summary>
		/// Dimensione x della immagine di riferimento che contiene la scritta
		/// </summary>
		public int rifContenitoreW { set; get; }

		/// <summary>
		/// Dimensione y della immagine di riferimento che contiene la scritta
		/// </summary>
		public int rifContenitoreH { set; get; }

		/// <summary>
		/// Questo attributo mi dice se la posizione è vuota (ovvero non ho nessuna trasformazione)
		/// </summary>
		public bool isPosizioneOriginaria {
			get {
				return( zoom == null && rotazione == null && traslazione == null ) ;
			}
		}
	}
}
