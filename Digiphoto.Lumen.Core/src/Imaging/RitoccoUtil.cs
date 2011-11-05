using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.Imaging {

	public static class RitoccoUtil {

		// TODO : forse da togliere da qui e creare una serie di AzioneGrafica?


		public static void creaProvinoFoto( Fotografia foto ) {
			creaProvinoFoto( PathUtil.nomeCompletoFoto( foto ), foto );
		}

		public static void creaProvinoFoto( string nomeFileFoto, Fotografia foto ) {
			
			IGestoreImmagineSrv gis = LumenApplication.Instance.getGestoreImmaginiSrv();

			Immagine immagineGrande = gis.load( nomeFileFoto );
			foto.imgOrig = immagineGrande;

			// TODO l'immagine risultante non ho ancora deciso se e come la gestirò.
			// per ora non faccio niente
			foto.imgRisultante = null; // immagineGrande;

			Immagine immaginePiccola = gis.creaProvino( immagineGrande );
			foto.imgProvino = immaginePiccola;
			gis.save( immaginePiccola, PathUtil.nomeCompletoProvino( foto ) );
		}


	

	}
}
