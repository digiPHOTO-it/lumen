using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Util;
using System.IO;
using Digiphoto.Lumen.Applicazione;
using System.Windows.Media;


namespace Digiphoto.Lumen.UI.Pubblico {


	public class SlideShow {

		/// <summary>
		/// Crea lo show vuoto
		/// </summary>
		public SlideShow() {
			this.slides = new List<Fotografia>();
		}

		/// <summary>
		/// Crea lo show riempendolo già con le foto passate
		/// </summary>
		/// <param name="fotografie"></param>
		public SlideShow( IEnumerable<Fotografia> fotografie ) : this() {
			this.slides.AddRange( fotografie );
		}

		public void svuota() {
			if( slides == null )
				slides = new List<Fotografia>();
			else
				slides.Clear();
		}

		/// <summary>
		/// Sostituisce le foto passate a quelle attualmente nello show
		/// </summary>
		/// <param name="fotografie"></param>
		public void sostituisciFoto( IEnumerable<Fotografia> fotografie ) {
			svuota();
			slides.AddRange( fotografie );
		}
	
		public int millisecondiIntervallo {
			get;
			set;
		}

		/// <summary>
		///  Elenco completo delle slide che voglio visualizzare ciclicamente
		/// </summary>
		public List<Fotografia> slides {
			get;
			set;
		}
	}
}
