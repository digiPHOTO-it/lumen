﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using System.Drawing;
using Digiphoto.Lumen.Imaging.Nativa.Correzioni;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Model;
using System.Drawing.Imaging;
using System.IO;

namespace Digiphoto.Lumen.Imaging.Nativa  {
	
	public class GestoreImmagineSrvImpl : ServizioImpl, IGestoreImmagineSrv {

		ProvinatoreNet _provinatoreNet;

		public GestoreImmagineSrvImpl() {
			_provinatoreNet = new ProvinatoreNet();
		}

		public IImmagine load( string fileName ) {

			// Image image = Image.FromFile( fileName );
			Image image = new Bitmap( fileName );
			return new ImmagineNet( image );
		}

		public IImmagine creaProvino( IImmagine immagineGrande ) {
			return _provinatoreNet.creaProvino( immagineGrande );
		}

		/** Salvo l'immagine con il nome del file indicato */
		public void save( IImmagine immagine, string fileName ) {
			((ImmagineNet)immagine).image.Save( fileName );
		}


		public IImmagine applicaCorrezioni( IImmagine immaginePartenza, ICollection<Correzione> correzioni ) {

			CorrettoreFactory factory = new CorrettoreFactory();

			IImmagine modificata = immaginePartenza;

			foreach( Correzione correzione in correzioni ) {
				Correttore correttore = factory.creaCorrettore( correzione.GetType() );
				modificata = correttore.applica( modificata, correzione );
			}

			return modificata;
		}
	}
}
