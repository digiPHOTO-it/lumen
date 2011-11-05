using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Util {
	
	
	
	public static class AiutanteFoto {

		/** Devo caricare gli attributi transienti della fotografia */
		public static void idrataImmaginiFoto( Fotografia foto ) {

			IGestoreImmagineSrv gis = LumenApplication.Instance.getGestoreImmaginiSrv();

			//
			foto.imgProvino = gis.load( PathUtil.nomeCompletoProvino( foto ) );

			//
			foto.imgOrig = gis.load( PathUtil.nomeCompletoFoto( foto ) );

			// TODO manca l'immagine risultante (se la gestiamo per davvero)
		}
	}
}
