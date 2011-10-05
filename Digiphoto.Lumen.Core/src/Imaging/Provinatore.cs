using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging {

	public abstract class Provinatore : IProvinatore {

		public Provinatore() {

			// Determino per default la grandezza massima del lato più grande del provino
			this.latoMax = Properties.Settings.Default.pixelLatoProvino;
		}

		/** E' la dimensione massima del lato del provino. Se non indicata, la ricavo dalla configurazione */
		public int latoMax { get; set; }

		/** E' la larghezza calcolata per il provino */
		protected int calcW { get; set; }

		/** E' la altezza calcolata per il provino */
		protected int calcH { get;	set; }

		/** E' l'immagine da rimpicciolire */
		public Immagine immagine {	get; protected set;	}

		public abstract Immagine creaImmagine( string nomeFile );

		public abstract Immagine creaProvino( Immagine immagineGrande );

		protected void calcolaEsattaWeH() {

			if( immagine.orientamento == Orientamento.Orizzontale ) {
				if( immagine.ww > this.latoMax ) {
					calcW = latoMax;
					calcH = (int) (calcW / immagine.rapporto );
				}
			} else {
				if( immagine.hh > latoMax ) {
					calcH = latoMax;
					calcW = (int)(calcH * immagine.rapporto);
				}
			}
		}

	}
}

