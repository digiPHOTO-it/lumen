using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Imaging.Correzioni;
using log4net;

namespace Digiphoto.Lumen.Util {
	
	public enum IdrataTarget {
		Tutte = 0xff,
		Originale = 0x02,
		Provino = 0x04,
	}

	public static class AiutanteFoto {

		private static readonly ILog _giornale = LogManager.GetLogger(typeof(AiutanteFoto));

		public static void disposeImmagini( Fotografia foto ) {
			if( foto.imgOrig != null )
				foto.imgOrig.Dispose();
			if( foto.imgProvino != null )
				foto.imgProvino.Dispose();
			if( foto.imgRisultante != null )
				foto.imgRisultante.Dispose();
		}


		/// <summary>
		/// Idrato tutte le immagini associate alla foto.
		/// Per decidere più puntualmente usare l'altro overload dove si indica il target
		/// </summary>
		public static void idrataImmaginiFoto( Fotografia foto ) {
			idrataImmaginiFoto( IdrataTarget.Tutte, foto );
		}

		/** 
		 * Devo caricare gli attributi transienti della fotografia 
		 */
		public static void idrataImmaginiFoto( IdrataTarget target, Fotografia foto ) {

			IGestoreImmagineSrv gis = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();

			try {	        
		
				//
				if( foto.imgProvino == null && (target & IdrataTarget.Provino) != 0 ) 
					foto.imgProvino = gis.load( PathUtil.nomeCompletoProvino( foto ) );

				//
				if( foto.imgOrig == null  && (target & IdrataTarget.Originale) != 0 )
					foto.imgOrig = gis.load( PathUtil.nomeCompletoFoto( foto ) );
	
					// TODO manca l'immagine risultante (se la gestiamo per davvero)
			} catch (Exception ee) {
				// Se non riesco a caricare una immagine, non posso farci niente qui. Devo tirare dritto.
				_giornale.Debug( "Impossibile caricare immagine della foto " + foto.ToString() );
			}

		}

		public static void creaProvinoFoto( Fotografia foto ) {
			creaProvinoFoto( PathUtil.nomeCompletoFoto( foto ), foto );
		}

		public static void creaProvinoFoto( string nomeFileFoto, Fotografia foto ) {

			IGestoreImmagineSrv gis = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();

			// Carico l'immagine grande originale (solo la prima volta)
			if( foto.imgOrig == null )
				foto.imgOrig = gis.load( nomeFileFoto );

			// Scale per ridimensionare
			IImmagine immaginePiccola = gis.creaProvino( foto.imgOrig );

			// applico eventuali correzioni
			if( foto.correzioniXml != null ) {
				CorrezioniList correzioni = SerializzaUtil.stringToObject<CorrezioniList>( foto.correzioniXml );
				immaginePiccola = gis.applicaCorrezioni( immaginePiccola, correzioni );
			}

			// Se avevo già un provino caricato, qui lo vado a sovrascrivere, quindi devo rilasciarlo
			if( foto.imgProvino != null )
				foto.imgProvino.Dispose();

			// Salvo su disco l'immagine risultante
			foto.imgProvino = immaginePiccola;
			gis.save( immaginePiccola, PathUtil.nomeCompletoProvino( foto ) );
		}

	}
}
