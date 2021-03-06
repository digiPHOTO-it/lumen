﻿using System;
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


		public override int ww {
			get {
				return _image.Width;
			}
		}

		public override int hh {
			get {
				return _image.Height;
			}
		}


		public override void Dispose() {

			if( image != null ) {
				try {
					image.Dispose();
				} finally {
					image = null;
				}
			}
		}
	}
}
