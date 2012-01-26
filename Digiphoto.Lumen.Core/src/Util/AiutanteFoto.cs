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

		public static void disposeImmagini( Fotografia foto ) {
			if( foto.imgOrig != null )
				foto.imgOrig.Dispose();
			if( foto.imgProvino != null )
				foto.imgProvino.Dispose();
			if( foto.imgRisultante != null )
				foto.imgRisultante.Dispose();
		}

		/** 
		 * Devo caricare gli attributi transienti della fotografia 
		 * ATTENZIONE:
		 * chi chiama questo metodo, deve enche preoccuparsi di fare la Dispose() delle immagini
		 * caricate.
		 * Purtoppo non ho trovato un modo sicuro per farlo automaticamente.
		 * Avrei voluto fare l'override di Dispose() dentro Fotografia, ma 
		 * non viene chiamato da E.F.
		 * TODO da studiare!
		 */
		public static void idrataImmaginiFoto( Fotografia foto ) {

			IGestoreImmagineSrv gis = LumenApplication.Instance.getGestoreImmaginiSrv();

			//
			if( foto.imgProvino == null )
				foto.imgProvino = gis.load( PathUtil.nomeCompletoProvino( foto ) );

			//
			if( foto.imgOrig == null )
				foto.imgOrig = gis.load( PathUtil.nomeCompletoFoto( foto ) );

			// TODO manca l'immagine risultante (se la gestiamo per davvero)
		}

		public static void creaProvinoFoto( Fotografia foto ) {
			creaProvinoFoto( PathUtil.nomeCompletoFoto( foto ), foto );
		}

		public static void creaProvinoFoto( string nomeFileFoto, Fotografia foto ) {

			IGestoreImmagineSrv gis = LumenApplication.Instance.getGestoreImmaginiSrv();

			// Carico l'immagine grande originale (solo la prima volta)
			if( foto.imgOrig == null )
				foto.imgOrig = gis.load( nomeFileFoto );

			// Scale per ridimensionare
			IImmagine immaginePiccola = gis.creaProvino( foto.imgOrig );

			// applico eventuali correzioni
			if( foto.correzioni != null && foto.correzioni.Count > 0 )
				immaginePiccola = gis.applicaCorrezioni( immaginePiccola, foto.correzioni );

			// Se avevo già un provino caricato, qui lo vado a sovrascrivere, quindi devo rilasciarlo
			if( foto.imgProvino != null )
				foto.imgProvino.Dispose();

			// Salvo su disco l'immagine risultante
			foto.imgProvino = immaginePiccola;
			gis.save( immaginePiccola, PathUtil.nomeCompletoProvino( foto ) );
		}

	}
}
