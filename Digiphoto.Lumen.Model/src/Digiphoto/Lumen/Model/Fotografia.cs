using System;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;


namespace Digiphoto.Lumen.Model {

	// Questi attributi sono transienti e non li gestisco sul database.
	// Ci penserò io a riempirli a mano

	public partial class Fotografia {

		public Immagine imgOrig { get; set; }

		public Immagine imgProvino { get; set; }

		public Immagine imgRisultante { get; set; }

		public bool selezionata { get; set; }

		public override string ToString() {
			return String.Format( "Num.{0} Oper={1} del={2}", this.numero, this.fotografo.iniziali, dataOraAcquisizione.ToShortDateString() );
		}

	}

}
