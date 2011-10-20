using System;
using System.Drawing;
using System.Xml.Serialization;

namespace Digiphoto.Lumen.Imaging.Ritocco {

	public class Ruota : Correzione {

		#region Proprietà

		/** da -360 a +360 */
		public short gradi {	get; set; }

		/** Serve per tagliarel automaticamente la foto per evitare lo scarto vuoto nei 4 angoli */
		public bool autoRitaglioScarto { get; set; }
		
		/** Viene utilizzato solo quando autoRitaglioScarto = false */
		public Color backgroundColorScarto { get; set; }

		#endregion

		public Ruota() {
		}

		public Ruota( short gradi ) {
			this.gradi = gradi;
			this.autoRitaglioScarto = true;
		}

		public override Cardinalita getCardinalita() {
			return Cardinalita.SOMMABILE;
		}
	}
}
