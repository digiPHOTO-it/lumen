using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Imaging.Nativa {

	
	/**
	 *  Implemento l'immagine con la classe Image del framewor
	 */
	public class ImmagineNet : Immagine {

		private Image _image;

		protected internal Image image {
			get {
				return _image;
			}
			set {
				_image = value;
			}
		}

		public ImmagineNet( Image image ) {
			this._image = image;
		}

		public Orientamento orientamento {
			get { 
				return( this.ww >= this.hh ? Orientamento.Orizzontale : Orientamento.Verticale );
			}
		}

		public float rapporto {
			get {
				return (float)this.ww / (float)this.hh;
			}
		}

		public int ww {
			get {
				return _image.Width;
			}
		}

		public int hh {
			get {
				return _image.Height;
			}
		}

	}
}
