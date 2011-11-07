using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Comandi {

	/** Indica su quale set di foto eseguire l'azione */
	public enum Target {
		Nessuna,   // tutti si chiederanno a che cosa serve...
		Corrente,
		Tutte,
		Selezionate,
		Album		
	}

	public enum Esito {
		Ok,
		Errore
	}

	public abstract class Comando {

		/** Altri possibili parametri per il comando */
	//	public Object extraParam { get; set; }


		public Comando() {
		}

		internal abstract Esito esegui( Fotografia foto );
		
	}
}
