using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using System.Drawing;

namespace Digiphoto.Lumen.Imaging.Nativa  {
	
	public class GestoreImmagineSrvImpl : ServizioImpl, IGestoreImmagineSrv {

		ProvinatoreNet _provinatoreNet;

		public GestoreImmagineSrvImpl() {
			_provinatoreNet = new ProvinatoreNet();
		}

		public Immagine load( string fileName ) {
			return new ImmagineNet( Image.FromFile( fileName ) );
		}

		public Immagine creaProvino( Immagine immagineGrande ) {
			return _provinatoreNet.creaProvino( immagineGrande );
		}

		public void save( Immagine immagine, string fileName ) {
			ImmagineNet immagineNet = (ImmagineNet)immagine;
			immagineNet.image.Save( fileName );
		}

	}
}
