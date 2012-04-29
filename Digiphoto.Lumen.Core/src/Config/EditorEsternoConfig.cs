using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Config {

	public class EditorEsternoConfig {

		public string commandLine { get; set; }

		/// <summary>
		/// Se true, significa che posso lanciare l'eseguibile passando tutti i nomi dei files sulla command line.
		/// Per esempio:
		/// GIMP.EXE img1.jpg img2.jpg img3.jpg
		/// </summary>
		public bool gestisceMultiArgs { get; set; }

		// La lunghezza massima del comando + gli argomenti non deve superare questo valore
		public static int MaxLenCmd = 2080;
	}
}
