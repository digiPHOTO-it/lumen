using System;
using Digiphoto.Lumen.Imaging;


namespace Digiphoto.Lumen.Imaging {

	public interface IProvinatore {

		int latoMax {
			get;
			set;
		}

		IImmagine immagine {
			get;
		}

//		IImmagine creaImmagine( string nomeFile );
		IImmagine creaProvino( IImmagine immagineGrande );
	}

}
