﻿using System;
using Digiphoto.Lumen.Imaging;


namespace Digiphoto.Lumen.Imaging {

	public interface IProvinatore {

		int latoMax {
			get;
			set;
		}

		Immagine immagine {
			get;
		}

		Immagine creaProvino( Immagine immagineGrande );
	}

}
