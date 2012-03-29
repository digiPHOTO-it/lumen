using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Servizi.Ritoccare {

	/// <summary>
	/// Con questo messaggio, il foto-explorer chiede che qualcuno (il foto ritocco) si prenda
	/// in carico queste foto per essere modificate
	/// </summary>
	public class FotoDaModificareMsg : Messaggio {



		public FotoDaModificareMsg( object sender )	: base( sender ) {
			fotosDaModificare = new List<Fotografia>();
		}

		public List<Fotografia> fotosDaModificare {
			get;
			set;
		}
	}
}
