using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi.Ricerca;

namespace Digiphoto.Lumen.UI.Pubblico {


	/// <summary>
	/// Lo slideshow è automatico perchè invece di indicare quali foto devono girare, 
	/// indico il filtro di ricerca che voglio eseguire ad ogni passaggio.
	/// In questo modo se si aggiungono delle foto all'archivio, dopo che lo show è partito,
	/// al successivo giro, verranno ripescate.
	/// </summary>
	public class SlideShowAutomatico : SlideShow {

		public SlideShowAutomatico( ParamCercaFoto param ) {
			paramCercaFoto = param;
		}

		public ParamCercaFoto paramCercaFoto {
			get;
			private set;
		}
	}
}
