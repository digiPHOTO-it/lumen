using System;


namespace Digiphoto.Lumen.Util {
	
	/** Indica su quale set di foto eseguire l'azione */
	public enum Target {
		Nessuna,   // tutti si chiederanno a che cosa serve...
		Corrente,
		Tutte,
		Selezionate,
		Album
	}

}
