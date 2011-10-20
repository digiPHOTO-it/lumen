using System;
using System.Drawing;
using System.Xml.Serialization;

namespace Digiphoto.Lumen.Imaging.Ritocco {

	public class Crop : Correzione {

		#region Proprietà

		/** Queste sono le dimensioni originali dell'immagine che ho ritagliato.
		 * Mi serviranno per calcolare le proporzioni della nuova area di ritaglio
		 * quando vado applicare il crop sull'immagine buona (infatti la prima che taglio sarà
		 * soltanto un provino
		 */
		public Size sizeImgOrig {
			get;
			set;
		}

		/** Questa area è relativa alla size originale (del provino) */
		public Rectangle areaDiRitaglio {
			get;
			set;
		}

		#endregion

		public Crop() {
		}

		/** Uso dei parametri semplici e primitivi.
		 * Avrei potuto usare Size e Rectangle ma volutamente non l'ho fatto perché
		 * non vorrei referenziare il package System.Drawing nel progetto chiamante
		 */
		public Crop( int oriImgWidth, int oriImgHeight, int x, int y, int width, int height ) 
			: this(  new Size( oriImgWidth, oriImgHeight ), new Rectangle( x, y, width, height ) ) {
		}

		public Crop( Size sizeOriImg, Rectangle areaDiRitaglio ) {

			this.sizeImgOrig = sizeImgOrig;
			this.areaDiRitaglio = areaDiRitaglio;
		}

		public override Cardinalita getCardinalita() {
			return Cardinalita.DISTINTI;
		}
	}
}
